using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MTunnel.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ServerPage : Page {
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

                case DisconnectPayload disconnectPayload:
                    HandleClientDisconnect(disconnectPayload);
                    break;

                case TokenPayload tokenPayload:
                    Debug.WriteLine($"Token {tokenPayload.Token}");
                    _ = DispatcherQueue.TryEnqueue(() => {
                        ViewModel.Token = tokenPayload.Token;
                    });
                    break;
            }
        }

        private void HandleClientAdd(ConnectedPayload payload) {
            _ = DispatcherQueue.TryEnqueue(() => {
                ViewModel.AddClient(new ConnectedClient {
                    ID = payload.SessionId,
                    ConnectionPath = payload.Addr ?? "Unknown",
                    ConnectedAt = DateTime.Now,
                });
            });
        }

        private void HandleClientDisconnect(DisconnectPayload payload) {
            _ = DispatcherQueue.TryEnqueue(() => {
                var client = ViewModel.Clients.FirstOrDefault(c => c.ID == payload.SessionId);
                if (client != null) {
                    ViewModel.RemoveClient(client);
                }
            });
        }

        private void OnDisconnectClick(object sender, RoutedEventArgs e) {
            if (sender is not FrameworkElement element || element.Tag is not ConnectedClient client)
                return;

            ProcessHandler.Instance.DisconnectClient(client.ID);
        }

        private void TerminateClick(object sender, RoutedEventArgs e) {
            if (InputKeyboardSource
                .GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift)
                .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down)) {
                var dialog = new ContentDialog {
                    Title = "Force terminate tunnel",
                    Content = "Are you sure you want to forcibly terminate the tunnel process? This may cause data loss for connected clients.",
                    CloseButtonText = "Cancel",
                    PrimaryButtonText = "Terminate",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = XamlRoot
                };

                dialog.PrimaryButtonClick += (s, args) => {
                    ProcessHandler.Instance.KillProcess();
                };

                _ = dialog.ShowAsync();
                return;
            }

            ProcessHandler.Instance.ShutdownBackend();
        }
    }

    public partial class ServerPageViewModel : INotifyPropertyChanged {
        public ObservableCollection<ConnectedClient> Clients { get; set; } = [];
        public event PropertyChangedEventHandler? PropertyChanged;

        private string token = string.Empty;
        public string Token {
            get => token;
            set {
                if (token == value)
                    return;

                token = value;
                OnPropertyChanged(nameof(Token));
                OnPropertyChanged(nameof(IsTokenEmpty));
                OnPropertyChanged(nameof(IsTokenNotEmpty));
            }
        }

        public bool IsTokenEmpty => string.IsNullOrEmpty(token);
        public bool IsTokenNotEmpty => !IsTokenEmpty;
        public bool IsNoClients => Clients.Count == 0;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void AddClient(ConnectedClient client) {
            Clients.Add(client);
            OnPropertyChanged(nameof(IsNoClients));
        }

        public void RemoveClient(ConnectedClient client) {
            Clients.Remove(client);
            OnPropertyChanged(nameof(IsNoClients));
        }
    }

    public class ConnectedClient {
        public string ID { get; set; } = string.Empty;
        public string ConnectionPath { get; set; } = string.Empty;
        public DateTime ConnectedAt { get; set; } = DateTime.Now;
    }
}
