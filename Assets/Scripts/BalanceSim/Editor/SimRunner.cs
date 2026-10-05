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
        private const double ReadRetrySeconds = 10;

        private static Process _process;
        private static readonly StringBuilder _stderr = new();
        private static readonly Stopwatch _processStopwatch = new();
        private static readonly Stopwatch _totalStopwatch = new();
        private static readonly Stopwatch _readRetryStopwatch = new();
        private static int _readRetryIndex = -1;
        private static string _terrainSummary;
        private static BalanceSimSettings _settings;
        private static SimValueSource _source;
        private static string _exePath;
        private static List<SimValueCase> _cases;
        private static int _round;
        private static int _roundStart;
        private static string _conclusion;
        private static int _runCount;
        private static bool _cancelRequested;
        private static SimResultSet _resultSet;

        public static bool IsRunning => _process != null;

        public static string ProgressText => _cases != null && _cases.Count > 1
            ? $"{(_round > 1 ? $"{_round}回目 " : string.Empty)}{_resultSet.cases.Count}/{_cases.Count}"
            : string.Empty;

        public static float SecondsPerRun => SessionState.GetFloat(SecondsPerRunKey, 0f);

        public static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;

        public static string LastResultPath => Path.Combine(WorkDir, "result.json");

        private static string WorkDir => Path.Combine(ProjectRoot, WorkRelativePath);

        private static string InputDir => Path.Combine(WorkDir, "inputs");

        private static string OutputDir => Path.Combine(WorkDir, "outputs");

        private static string InputPath(int index) => Path.Combine(InputDir, $"input_{index:0000}.json");

        private static string OutputPath(int index) => Path.Combine(OutputDir, $"output_{index:0000}.json");

        public static void Run(BalanceSimSettings settings)
        {
            if (IsRunning)
            {
                Debug.LogWarning("[BalanceSim] 前回の実行が終わっていません");
                return;
            }

            string exePath = Path.Combine(ProjectRoot, ExeRelativePath);
            if (!File.Exists(exePath))
            {
                Debug.LogError($"[BalanceSim] 実行ファイルが見つかりません: {exePath}");
                return;
            }

            List<SimValueCase> cases;
            try
            {
                cases = settings.Values != null ? settings.Values.BuildCases() : new List<SimValueCase> { SimValueCase.None };
                if (cases.Count == 0)
                {
                    throw new InvalidOperationException($"{settings.Values.name} から試す組み合わせが1つも作られませんでした");
                }
                WriteInputs(settings, cases);
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

            _settings = settings;
            _source = settings.Values;
            _exePath = exePath;
            _round = 1;
            _roundStart = 0;
            _conclusion = null;
            _cases = cases;
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
            StartProcess(exePath);
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

        private static void WriteInputs(BalanceSimSettings settings, List<SimValueCase> cases)
        {
            ResetDirectory(InputDir);
            ResetDirectory(OutputDir);

            var warnings = new List<string>();
            SimInputBuilder.Build(settings, cases, (i, input) => File.WriteAllText(InputPath(i), JsonUtility.ToJson(input)), warnings, out _terrainSummary);
            LogWarnings(warnings);
        }

        private static void LogWarnings(IEnumerable<string> warnings)
        {
            foreach (string warning in warnings.Distinct())
            {
                Debug.LogWarning($"[BalanceSim] {warning}");
            }
        }

        private static void StartNextRound()
        {
            List<SimValueCase> next;
            string conclusion = null;
            try
            {
                var warnings = new List<string>();
                next = _source != null
                    ? _source.BuildNextCases(_cases, _resultSet.cases.Select(c => c.result).ToList(), warnings, out conclusion)
                    : new List<SimValueCase>();
                LogWarnings(warnings);
                if (next.Count > 0)
                {
                    WriteInputs(_settings, next);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[BalanceSim] 値の読み出しを中止しました（{_resultSet.cases.Count} 通りが完了）");
                Finish(true);
                return;
            }
            catch (Exception e)
            {
                Debug.LogError($"[BalanceSim] 次に試す組み合わせを作れませんでした（{_resultSet.cases.Count} 通りが完了）: {e.Message}\n{e}");
                Finish(true);
                return;
            }

            if (next.Count == 0)
            {
                _conclusion = conclusion;
                LogCompleted();
                Finish(true);
                return;
            }

            _round++;
            _roundStart = _cases.Count;
            _cases.AddRange(next);
            StartProcess(_exePath);
        }

        private static void ResetDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            Directory.CreateDirectory(path);
        }

        private static void StartProcess(string exePath)
        {
            var startInfo = new ProcessStartInfo(exePath)
            {
                Arguments = $"\"{InputDir}\" \"{OutputDir}\"",
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
            _processStopwatch.Restart();
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
            bool exited = _process.HasExited;
            if (!CollectOutputs())
            {
                if (!_process.HasExited)
                {
                    _process.Kill();
                }
                Finish(true);
                return;
            }

            if (!exited)
            {
                return;
            }

            if (!_cancelRequested && _readRetryIndex == _resultSet.cases.Count)
            {
                return;
            }

            _process.WaitForExit();
            int exitCode = _process.ExitCode;
            _processStopwatch.Stop();
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
                Debug.LogError($"[BalanceSim] 外部プログラムが失敗しました (終了コード {exitCode}、{_cases.Count} 通りのうち {_resultSet.cases.Count} 通りが完了)\n{stderr}");
                Finish(true);
                return;
            }

            if (_resultSet.cases.Count < _cases.Count)
            {
                Debug.LogError($"[BalanceSim] 外部プログラムの結果が足りません（{_cases.Count} 通りのうち {_resultSet.cases.Count} 通り）");
                Finish(true);
                return;
            }

            SessionState.SetFloat(SecondsPerRunKey, (float)(_processStopwatch.Elapsed.TotalSeconds / Math.Max(1L, (long)(_cases.Count - _roundStart) * _runCount)));
            StartNextRound();
        }

        private static bool CollectOutputs()
        {
            while (_resultSet.cases.Count < _cases.Count)
            {
                int index = _resultSet.cases.Count;
                string path = OutputPath(index - _roundStart);
                if (!File.Exists(path))
                {
                    return true;
                }

                SimResult result;
                try
                {
                    result = JsonUtility.FromJson<SimResult>(File.ReadAllText(path));
                }
                catch (IOException) when (KeepRetryingRead(index))
                {
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[BalanceSim] 外部プログラムの結果を読めませんでした{CaseSuffix(index)}: {e.Message}\n{e}");
                    return false;
                }
                _resultSet.cases.Add(new SimResultCase { values = new List<SimCaseValue>(_cases[index].Labels), result = result });
            }
            return true;
        }

        private static bool KeepRetryingRead(int index)
        {
            if (_readRetryIndex != index)
            {
                _readRetryIndex = index;
                _readRetryStopwatch.Restart();
            }
            return _readRetryStopwatch.Elapsed.TotalSeconds < ReadRetrySeconds;
        }

        private static void LogCompleted()
        {
            string elapsed = $"{_totalStopwatch.Elapsed.TotalSeconds:F2}秒";
            if (_resultSet.cases.Count == 1)
            {
                Debug.Log($"[BalanceSim] 完了 ({elapsed}): {_resultSet.cases[0].result.message}\n{_terrainSummary}\n結果: {LastResultPath}");
                return;
            }

            string rounds = _round > 1 ? $"{_round} 回に分けて実行、" : string.Empty;
            var text = new StringBuilder($"[BalanceSim] 探索が完了 ({_resultSet.cases.Count} 通り、{rounds}{elapsed})");
            if (_conclusion != null)
            {
                text.Append($"\n{_conclusion}");
            }
            text.Append($"\n{_terrainSummary}\n結果: {LastResultPath}");
            for (int i = 0; i < _resultSet.cases.Count; i++)
            {
                text.Append($"\n{i + 1}. {_resultSet.cases[i].Label}: {_resultSet.cases[i].result.message}");
            }
            Debug.Log(text.ToString());
        }

        private static string CaseSuffix(int index)
        {
            return _cases.Count > 1 ? $"（{index + 1}/{_cases.Count} 通り目: {SimResultCase.LabelOf(_cases[index].Labels)}）" : string.Empty;
        }

        private static void Abort()
        {
            if (_process != null && !_process.HasExited)
            {
                _process.Kill();
            }
            CollectOutputs();
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

            _settings = null;
            _source = null;
            _exePath = null;
            _cases = null;
            _round = 0;
            _roundStart = 0;
            _conclusion = null;
            _resultSet = null;
            _cancelRequested = false;
            _readRetryIndex = -1;
        }
    }
}
