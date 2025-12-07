using MTunnel.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MTunnel {
    public enum RawActionType {
        TOKEN,
        DISCONNECT,
        CONNECTED,
        LIST,
        BACKEND_STARTED,
        ERROR,
        SHUTDOWN
    }

    public enum ProcessType {
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

    public class StartedInfo {
        [JsonPropertyName("process_id")]
        public required string ProcessId { get; set; }

        [JsonPropertyName("process_type")]
        public required string ProcessType { get; set; }
    }

    public class StdoutEvent {
        [JsonPropertyName("action")]
        public required string Action { get; set; }

        [JsonPropertyName("session_id")]
        public string? SessionId { get; set; }

        [JsonPropertyName("sessions")]
        public List<string>? Sessions { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("started_info")]
        public StartedInfo? StartedInfo { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("port")]
        public ushort? Port { get; set; }

        [JsonPropertyName("addr")]
        public string? Addr { get; set; }
    }

    [JsonSourceGenerationOptions(WriteIndented = false)]
    [JsonSerializable(typeof(StdoutEvent))]
    internal partial class StdoutJsonContext : JsonSerializerContext { }

    internal class ProcessHandler {
        private static ProcessHandler? _instance;
        private static string _executablePath = Path.Combine(AppContext.BaseDirectory, "executable", "tunnel.exe");
        public static ProcessHandler Instance => _instance ??= new ProcessHandler();

        private Process? _process;
        private TunnelLog? _logViewer;

        public event Action<ProcessEvent>? OnProcessEvent;
        public event Action<string>? OnStdErr;
        public event Action<string>? OnStdOut;
        public event Action<string>? OnStdIn;

        public bool IsProcessRunning => _process != null && !_process.HasExited;
        public bool IsLogViewerEnabled => _logViewer != null;

        public string Token { get; private set; } = string.Empty;

        private ProcessHandler() { }
        private void StartProcess(string arguemnts) {
            if (_process != null && !_process.HasExited) {
                throw new InvalidOperationException("Process is already running.");
            }

            OnStdErr?.Invoke($"Starting process with arguments: {arguemnts}");
            _process = new Process {
                StartInfo = new ProcessStartInfo {
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

            _process.OutputDataReceived += (sender, e) => {
                if (!string.IsNullOrEmpty(e.Data)) {
                    HandleStdOut(e.Data);
                }
            };

            _process.ErrorDataReceived += (sender, e) => {
                if (!string.IsNullOrEmpty(e.Data)) {
                    HandleStdErr(e.Data);
                }
            };

            _process.Exited += (sender, e) => {
                ProcessExit();
            };

            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }

        public void StartAsHost(int port, string network) {
            string args = $"--port {port} --network {network}";
            StartProcess(args);
        }

        public void StartAsClient(string token, int port) {
            string args = $"--token {token} --port {port}";
            StartProcess(args);
        }

        public void KillProcess() {
            if (_process != null && !_process.HasExited) {
                OnStdErr?.Invoke("Stopping process...");

                _process.Kill();
                _process.Dispose();
                _process = null;

                ProcessExit();
            }
        }

        private void ProcessExit() {
            OnProcessEvent?.Invoke(new BackendStopped());
            OnStdErr?.Invoke("Process has exited.");
        }

        private void SendStdIn(string input) {
            if (_process == null || _process.HasExited) {
                OnStdErr?.Invoke("Process is not running.");
                return;
            }

            _process.StandardInput.WriteLine(input);
            OnStdIn?.Invoke(input);
        }

        public void DisconnectClient(string sessionId) {
            var command = new JsonObject {
                ["action"] = RawActionType.DISCONNECT.ToString(),
                ["session_id"] = sessionId
            };
            SendStdIn(command.ToJsonString());
        }

        public void ShutdownBackend() {
            var command = new JsonObject {
                ["action"] = RawActionType.SHUTDOWN.ToString()
            };
            SendStdIn(command.ToJsonString());
        }

        private void HandleStdOut(string line) {
            var dec = JsonSerializer.Deserialize(line, StdoutJsonContext.Default.StdoutEvent);
            if (dec == null) {
                Console.WriteLine($"Failed to deserialize stdout: {line}");
                OnStdErr?.Invoke($"Failed to deserialize stdout: {line}");
                return;
            }

            OnStdOut?.Invoke(line);

            if (!Enum.TryParse<RawActionType>(dec.Action, true, out var actionType)) {
                Console.WriteLine($"Unknown action: {dec.Action}");
                OnStdErr?.Invoke($"Unknown action: {dec.Action}");
                return;
            }

            switch (actionType) {
                case RawActionType.TOKEN:
                    if (dec.Token == null) {
                        return;
                    }
                    Token = dec.Token;
                    OnProcessEvent?.Invoke(new TokenPayload(dec.Token));
                    break;

                case RawActionType.CONNECTED:
                    if (dec.SessionId == null) {
                        return;
                    }
                    OnProcessEvent?.Invoke(new ConnectedPayload(
                        dec.SessionId,
                        dec.Port,
                        dec.Addr
                    ));
                    break;

                case RawActionType.DISCONNECT:
                    if (dec.SessionId == null) {
                        return;
                    }
                    OnProcessEvent?.Invoke(new DisconnectPayload(dec.SessionId));
                    break;

                case RawActionType.LIST:
                    if (dec.Sessions == null) {
                        return;
                    }
                    OnProcessEvent?.Invoke(new ListPayload([.. dec.Sessions]));
                    break;

                case RawActionType.ERROR:
                    if (dec.Error == null) {
                        return;
                    }
                    OnProcessEvent?.Invoke(new ErrorPayload(dec.Error));
                    break;

                case RawActionType.BACKEND_STARTED:
                    if (dec.StartedInfo == null) {
                        return;
                    }
                    var type = dec.StartedInfo.ProcessType.Equals("host", StringComparison.CurrentCultureIgnoreCase)
                        ? ProcessType.Host
                        : ProcessType.Client;
                    OnProcessEvent?.Invoke(new BackendStarted(
                        dec.StartedInfo.ProcessId,
                        type
                    ));
                    break;

                default:
                    Console.WriteLine($"Unknown action: {dec.Action}");
                    OnStdErr?.Invoke($"Unknown action: {dec.Action}");
                    break;
            }
        }

        private void HandleStdErr(string line) {
            Console.WriteLine($"Tunnel stderr: {line}");
            OnStdErr?.Invoke(line);
        }

        public void OpenLogViewer() {
            if (_logViewer != null) {
                return;
            }

            _logViewer = new TunnelLog();
            _logViewer.OnClosed += () => { _logViewer = null; };
            _logViewer.Activate();
        }

        public void CloseLogViewer() {
            if (_logViewer == null) {
                return;
            }

            _logViewer.Close();
            _logViewer = null;
        }
    }
}
