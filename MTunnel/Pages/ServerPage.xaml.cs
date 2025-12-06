using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MTunnel.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ServerPage : Page {
        public ObservableCollection<ConnectedClient> Clients { get; set; } = [];
        public ServerPageViewModel ViewModel { get; } = new();

        public ServerPage() {
            InitializeComponent();
            DataContext = ViewModel;

            ProcessHandler.Instance.OnProcessEvent += HandleProcessEvent;
        }

        private void HandleProcessEvent(ProcessEvent evt) {
            switch (evt) {
                case ConnectedPayload connectedPayload:
                    HandleClientAdd(connectedPayload);
                    break;

                case TokenPayload tokenPayload:
                    Debug.WriteLine($"Token {tokenPayload.Token}");
                    _ = DispatcherQueue.TryEnqueue(() => {
                        ViewModel.Token = tokenPayload.Token;
                    });
                    break;
            }
        }

        private void ToggleShowToken() {
            if (Clients.Count > 0) {
                ConnectedViewPanel.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                TokenViewPanel.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                return;
            }

            ConnectedViewPanel.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            TokenViewPanel.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        }

        private void HandleClientAdd(ConnectedPayload payload) {
            _ = DispatcherQueue.TryEnqueue(() => {
                Clients.Add(new ConnectedClient {
                    ID = payload.SessionId,
                    ConnectionPath = $"{payload.Addr}:{payload.Port}",
                    ConnectedAt = DateTime.Now
                });
                ToggleShowToken();
            });
        }

        private void TerminateClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
            ProcessHandler.Instance.StopProcess();
        }
    }

    public partial class ServerPageViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string token = "Generating token, please wait...";
        public string Token {
            get => token;
            set {
                if (token == value)
                    return;

                token = value;
                OnPropertyChanged(nameof(Token));
            }
        }
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class ConnectedClient {
        public string ID { get; set; } = string.Empty;
        public string ConnectionPath { get; set; } = string.Empty;
        public DateTime ConnectedAt { get; set; } = DateTime.Now;
    }
}
