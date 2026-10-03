using System.Diagnostics;
using System.IO;
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

        private static Process _process;
        private static readonly StringBuilder _stderr = new();
        private static readonly Stopwatch _stopwatch = new();
        private static string _outputPath;

        public static bool IsRunning => _process != null;

        public static void Run(BalanceSimSettings settings)
        {
            if (IsRunning)
            {
                Debug.LogWarning("[BalanceSim] 前回の実行が終わっていません");
                return;
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string exePath = Path.Combine(projectRoot, ExeRelativePath);
            if (!File.Exists(exePath))
            {
                Debug.LogError($"[BalanceSim] 実行ファイルが見つかりません: {exePath}");
                return;
            }

            string workDir = Path.Combine(projectRoot, WorkRelativePath);
            Directory.CreateDirectory(workDir);
            string inputPath = Path.Combine(workDir, "input.json");
            _outputPath = Path.Combine(workDir, "output.json");
            if (File.Exists(_outputPath))
            {
                File.Delete(_outputPath);
            }

            SimInput input = SimInputBuilder.Build(settings);
            File.WriteAllText(inputPath, JsonUtility.ToJson(input, true));

            var startInfo = new ProcessStartInfo(exePath)
            {
                Arguments = $"\"{inputPath}\" \"{_outputPath}\"",
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
            _stopwatch.Restart();

            EditorApplication.update += Poll;
            AssemblyReloadEvents.beforeAssemblyReload += Abort;
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
            _stopwatch.Stop();
            Cleanup();

            string stderr;
            lock (_stderr)
            {
                stderr = _stderr.ToString();
            }

            if (exitCode != 0)
            {
                Debug.LogError($"[BalanceSim] 外部プログラムが失敗しました (終了コード {exitCode})\n{stderr}");
                return;
            }

            SimResult result = JsonUtility.FromJson<SimResult>(File.ReadAllText(_outputPath));
            Debug.Log($"[BalanceSim] 完了 ({_stopwatch.Elapsed.TotalSeconds:F2}秒): {result.message}\n結果: {_outputPath}");
        }

        private static void Abort()
        {
            if (_process != null && !_process.HasExited)
            {
                _process.Kill();
            }
            Cleanup();
        }

        private static void Cleanup()
        {
            EditorApplication.update -= Poll;
            AssemblyReloadEvents.beforeAssemblyReload -= Abort;
            _process?.Dispose();
            _process = null;
        }
    }
}
