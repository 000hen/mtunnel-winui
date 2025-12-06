using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MTunnel.Pages {
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public enum LogType {
        Undefined,
        StdOut,
        StdErr,
        StdIn
    }

    public sealed partial class TunnelLog : Window {
        public ObservableCollection<LogEntry> LogEntries { get; set; } = [];
        public event Action? OnClosed;

        private bool _showStdErr = true;
        private bool _showStdOut = true;
        private bool _showStdIn = true;

        public TunnelLog() {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;

            ProcessHandler.Instance.OnStdErr += AddStdErr;
            ProcessHandler.Instance.OnStdOut += AddStdOut;
            ProcessHandler.Instance.OnStdIn += AddStdIn;
        }

        private void AddStdErr(string message) {
            if (!_showStdErr)
                return;

            AddLogEntry(LogType.StdErr, message);
        }

        private void AddStdOut(string message) {
            if (!_showStdOut)
                return;

            AddLogEntry(LogType.StdOut, message);
        }

        private void AddStdIn(string message) {
            if (!_showStdIn)
                return;

            AddLogEntry(LogType.StdIn, message);
        }

        public void AddLogEntry(LogType type, string message) {
            _ = DispatcherQueue.TryEnqueue(() => {
                LogEntries.Add(new LogEntry {
                    Timestamp = DateTime.Now,
                    Message = message,
                    Type = type
                });
                LogListView.ScrollIntoView(LogEntries[^1]);
            });
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e) {
            _ = DispatcherQueue.TryEnqueue(() => {
                LogEntries.Clear();
            });
        }

        private void Window_Closed(object sender, WindowEventArgs args) {
            OnClosed?.Invoke();
            ProcessHandler.Instance.OnStdErr -= AddStdErr;
            ProcessHandler.Instance.OnStdOut -= AddStdOut;
            ProcessHandler.Instance.OnStdIn -= AddStdIn;
        }

        private void StdInCheckBox_Checked(object sender, RoutedEventArgs e) {
            _showStdIn = StdInCheckBox.IsChecked ?? false;
        }

        private void StdOutCheckBox_Checked(object sender, RoutedEventArgs e) {
            _showStdOut = StdOutCheckBox.IsChecked ?? false;
        }

        private void StdErrCheckBox_Checked(object sender, RoutedEventArgs e) {
            _showStdIn = StdErrCheckBox.IsChecked ?? false;
        }
    }

    public class LogEntry {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; } = string.Empty;
        public LogType Type { get; set; } = LogType.Undefined;
    }
}
