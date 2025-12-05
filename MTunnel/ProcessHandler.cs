using System;
using System.Diagnostics;
using System.IO;

namespace MTunnel
{
    public enum ProcessType
    {
        Host,
        Client
    }

    public abstract record ProcessEvent;
    public sealed record TokenPayload(string Token) : ProcessEvent;
    public sealed record DisconnectPayload(string SessionId) : ProcessEvent;
    public sealed record ConnectedPayload(
        string SessionId,
        int? Port = null,
        string? Addr = null
    ) : ProcessEvent;
    public sealed record ListPayload(string[] Sessions) : ProcessEvent;
    public sealed record ErrorPayload(string Error) : ProcessEvent;
    public sealed record BackendStarted(string ProcessId, ProcessType Type) : ProcessEvent;
    public sealed record BackendStopped() : ProcessEvent;

    internal class ProcessHandler
    {
        private static ProcessHandler? _instance;
        private static string _executablePath = Path.Combine(AppContext.BaseDirectory, "executable", "tunnel.exe");
        public static ProcessHandler Instance => _instance ??= new ProcessHandler();

        private Process? _process;
        public event Action<ProcessEvent>? OnProcessEvent;

        private ProcessHandler() { }
        private void StartProcess(string arguemnts)
        {
            if (_process != null && !_process.HasExited)
            {
                throw new InvalidOperationException("Process is already running.");
            }

            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _executablePath,
                    Arguments = arguemnts,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
                EnableRaisingEvents = true
            };

            _process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    HandleStdOut(e.Data);
                }
            };

            _process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    HandleStdErr(e.Data);
                }
            };

            _process.Exited += (sender, e) =>
            {
                OnProcessEvent?.Invoke(new BackendStopped());
            };

            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }

        public void StartAsHost(int port, string network)
        {
            string args = $"--port {port} --network {network}";
            StartProcess(args);
        }

        public void StartAsClient(string token, int port)
        {
            string args = $"--token {token} --port {port}";
            StartProcess(args);
        }

        public void StopProcess()
        {
            if (_process != null && !_process.HasExited)
            {
                _process.Kill();
                _process.Dispose();
                _process = null;
            }
        }

        private static void HandleStdOut(string line)
        {

        }

        private static void HandleStdErr(string line)
        {
            Console.WriteLine($"Tunnel stderr: {line}");
        }
    }
}
