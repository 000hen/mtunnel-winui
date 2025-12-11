using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MTunnel.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ClientPage : Page {
        private ClientPageViewModel ViewModel { get; } = new ClientPageViewModel();

        public ClientPage() {
            InitializeComponent();

            ProcessHandler.Instance.OnProcessEvent += HandleProcessEvent;
        }

        private void ForceTerminate(object sender, RoutedEventArgs e) {
            ProcessHandler.Instance.KillProcess();
        }

        private void Terminate(object sender, RoutedEventArgs e) {
            ProcessHandler.Instance.ShutdownBackend();
        }

        private void HandleProcessEvent(ProcessEvent evt) {
            switch (evt) {
                case ConnectedPayload connected:
                    HandleConnected(connected);
                    break;
            }
        }

        private void HandleConnected(ConnectedPayload payload) {
            _ = DispatcherQueue.TryEnqueue(() => {
                ViewModel.SessionID = payload.SessionId;
                ViewModel.Port = payload.Port ?? 0;
            });
        }
    }

    public partial class ClientPageViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string sessionId = string.Empty;
        public string SessionID {
            get => sessionId;
            set {
                if (sessionId == value)
                    return;
                sessionId = value;
                NotifyAll();
            }
        }

        private int port = 0;
        public int Port {
            get => port;
            set {
                if (port == value)
                    return;
                port = value;
                NotifyAll();
            }
        }

        private void NotifyAll() {
            OnPropertyChanged(nameof(SessionID));
            OnPropertyChanged(nameof(Port));
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(IsNotConnected));
            OnPropertyChanged(nameof(ConnectionAddress));
        }

        public bool IsConnected => !string.IsNullOrEmpty(sessionId) && Port != 0;
        public bool IsNotConnected => !IsConnected;
        public string ConnectionAddress => IsConnected ? $"localhost:{Port}" : "N/A";
    }
}
