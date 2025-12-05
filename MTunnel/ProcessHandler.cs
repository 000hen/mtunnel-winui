using System;
using System.Diagnostics;

namespace MTunnel
{
    public enum ProcessEventType
    {
        TOKEN,
        DISCONNECT,
        CONNECTED,
        LIST,
        BACKEND_STARTED,
        ERROR,
    }

    public class ProcessEvent(ProcessEventType eventType, string message)
    {
        public ProcessEventType EventType { get; set; } = eventType;
        public string Message { get; set; } = message;
    }

    internal class ProcessHandler
    {
        private static ProcessHandler? _instance;
        public static ProcessHandler Instance => _instance ??= new ProcessHandler();

        private Process? _process;
        public event Action<ProcessEvent>? OnProcessEvent;

        private ProcessHandler() { }
        public void StartProcess()
        {

        }
    }
}
