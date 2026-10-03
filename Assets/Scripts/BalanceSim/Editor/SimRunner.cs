using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace BalanceSim.Editor
{
    public static class SimRunner
    {
        private const string ExeRelativePath = "Tools/BalanceSim/Release/BalanceSim.exe";
        private const string WorkRelativePath = "Temp/BalanceSim";
        private const string SecondsPerRunKey = "BalanceSim.SecondsPerRun";

        private static Process _process;
        private static readonly StringBuilder _stderr = new();
        private static readonly Stopwatch _caseStopwatch = new();
        private static readonly Stopwatch _totalStopwatch = new();
        private static string _exePath;
        private static string _terrainSummary;
        private static List<SimValueCase> _cases;
        private static int _caseIndex;
        private static int _runCount;
        private static bool _cancelRequested;
        private static SimResultSet _resultSet;

        public static bool IsRunning => _process != null;

        public static string ProgressText => _cases != null && _cases.Count > 1 ? $"{_caseIndex + 1}/{_cases.Count}" : string.Empty;

        public static float SecondsPerRun => SessionState.GetFloat(SecondsPerRunKey, 0f);

        public static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;

        public static string LastResultPath => Path.Combine(WorkDir, "result.json");

        private static string WorkDir => Path.Combine(ProjectRoot, WorkRelativePath);

        private static string InputDir => Path.Combine(WorkDir, "inputs");

        private static string OutputPath => Path.Combine(WorkDir, "output.json");

        private static string InputPath(int index) => Path.Combine(InputDir, $"input_{index:0000}.json");

        public static void Run(BalanceSimSettings settings)
        {
            if (IsRunning)
            {
                Debug.LogWarning("[BalanceSim] 前回の実行が終わっていません");
                return;
            }

            _exePath = Path.Combine(ProjectRoot, ExeRelativePath);
            if (!File.Exists(_exePath))
            {
                Debug.LogError($"[BalanceSim] 実行ファイルが見つかりません: {_exePath}");
                return;
            }

            if (Directory.Exists(InputDir))
            {
                Directory.Delete(InputDir, true);
            }
            Directory.CreateDirectory(InputDir);

            var warnings = new List<string>();
            List<SimValueCase> cases;
            try
            {
                cases = settings.Values != null ? settings.Values.BuildCases() : new List<SimValueCase> { SimValueCase.None };
                if (cases.Count == 0)
                {
                    throw new InvalidOperationException($"{settings.Values.name} から試す組み合わせが1つも作られませんでした");
                }
                SimInputBuilder.Build(settings, cases, (i, input) => File.WriteAllText(InputPath(i), JsonUtility.ToJson(input, true)), warnings, out _terrainSummary);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[BalanceSim] 値の読み出しを中止しました");
                return;
            }
            catch (Exception e)
            {
                Debug.LogError($"[BalanceSim] Unity 側の値の読み出しに失敗しました: {e.Message}\n{e}");
                return;
            }

            foreach (string warning in warnings.Distinct())
            {
                Debug.LogWarning($"[BalanceSim] {warning}");
            }

            _cases = cases;
            _caseIndex = 0;
            _runCount = settings.RunCount;
            _cancelRequested = false;
            _resultSet = new SimResultSet
            {
                createdAt = DateTime.Now.ToString(SimResultSet.DateFormat, CultureInfo.InvariantCulture),
                settingsName = settings.name,
                sceneName = settings.TargetScene != null ? settings.TargetScene.name : string.Empty,
                valuesName = settings.Values != null ? settings.Values.name : string.Empty,
                runCount = settings.RunCount,
                fixedValues = SimCaseValue.Of(cases[0].Fixed),
            };

            _totalStopwatch.Restart();
            StartCase();
            EditorApplication.update += Poll;
            AssemblyReloadEvents.beforeAssemblyReload += Abort;
        }

        public static void Cancel()
        {
            if (!IsRunning)
            {
                return;
            }

            _cancelRequested = true;
            if (!_process.HasExited)
            {
                _process.Kill();
            }
        }

        private static void StartCase()
        {
            if (File.Exists(OutputPath))
            {
                File.Delete(OutputPath);
            }

            var startInfo = new ProcessStartInfo(_exePath)
            {
                Arguments = $"\"{InputPath(_caseIndex)}\" \"{OutputPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                StandardErrorEncoding = Encoding.UTF8,
            };

            lock (_stderr)
            {
                _stderr.Clear();
            }

            _process = new Process { StartInfo = startInfo };
            _process.ErrorDataReceived += OnErrorData;
            _process.Start();
            _process.BeginErrorReadLine();
            _caseStopwatch.Restart();
        }

        private static void OnErrorData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }

            lock (_stderr)
            {
                _stderr.AppendLine(e.Data);
            }
        }

        private static void Poll()
        {
            if (!_process.HasExited)
            {
                return;
            }

            _process.WaitForExit();
            int exitCode = _process.ExitCode;
            _caseStopwatch.Stop();
            _process.Dispose();
            _process = null;

            if (_cancelRequested)
            {
                Debug.Log($"[BalanceSim] 中止しました（{_cases.Count} 通りのうち {_resultSet.cases.Count} 通りが完了）");
                Finish(true);
                return;
            }

            if (exitCode != 0)
            {
                string stderr;
                lock (_stderr)
                {
                    stderr = _stderr.ToString();
                }
                Debug.LogError($"[BalanceSim] 外部プログラムが失敗しました (終了コード {exitCode}){CaseSuffix(_caseIndex)}\n{stderr}");
                Finish(true);
                return;
            }

            SimResult result;
            try
            {
                result = JsonUtility.FromJson<SimResult>(File.ReadAllText(OutputPath));
            }
            catch (Exception e)
            {
                Debug.LogError($"[BalanceSim] 外部プログラムの結果を読めませんでした{CaseSuffix(_caseIndex)}: {e.Message}\n{e}");
                Finish(true);
                return;
            }
            SessionState.SetFloat(SecondsPerRunKey, (float)(_caseStopwatch.Elapsed.TotalSeconds / Math.Max(1, _runCount)));
            _resultSet.cases.Add(new SimResultCase { values = SimCaseValue.Of(_cases[_caseIndex].Swept), result = result });

            _caseIndex++;
            if (_caseIndex >= _cases.Count)
            {
                LogCompleted();
                Finish(true);
                return;
            }
            StartCase();
        }

        private static void LogCompleted()
        {
            string elapsed = $"{_totalStopwatch.Elapsed.TotalSeconds:F2}秒";
            if (_resultSet.cases.Count == 1)
            {
                Debug.Log($"[BalanceSim] 完了 ({elapsed}): {_resultSet.cases[0].result.message}\n{_terrainSummary}\n結果: {LastResultPath}");
                return;
            }

            var text = new StringBuilder($"[BalanceSim] 探索が完了 ({_resultSet.cases.Count} 通り、{elapsed})\n{_terrainSummary}\n結果: {LastResultPath}");
            for (int i = 0; i < _resultSet.cases.Count; i++)
            {
                text.Append($"\n{i + 1}. {_resultSet.cases[i].Label}: {_resultSet.cases[i].result.message}");
            }
            Debug.Log(text.ToString());
        }

        private static string CaseSuffix(int index)
        {
            return _cases.Count > 1 ? $"（{index + 1}/{_cases.Count} 通り目: {string.Join(", ", SimCaseValue.Of(_cases[index].Swept))}）" : string.Empty;
        }

        private static void Abort()
        {
            if (_process != null && !_process.HasExited)
            {
                _process.Kill();
            }
            Debug.LogWarning($"[BalanceSim] スクリプトの再コンパイルのため中断しました（{_cases.Count} 通りのうち {_resultSet.cases.Count} 通りが完了）");
            Finish(false);
        }

        private static void Finish(bool show)
        {
            EditorApplication.update -= Poll;
            AssemblyReloadEvents.beforeAssemblyReload -= Abort;
            _process?.Dispose();
            _process = null;
            _totalStopwatch.Stop();

            if (_resultSet.cases.Count > 0)
            {
                File.WriteAllText(LastResultPath, JsonUtility.ToJson(_resultSet));
                if (show)
                {
                    SimResultWindow.ShowResult(_resultSet, LastResultPath);
                }
            }

            _cases = null;
            _resultSet = null;
            _cancelRequested = false;
        }
    }
}
