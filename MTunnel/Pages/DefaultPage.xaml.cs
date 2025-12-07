using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTunnel.Components;
using System;
using System.ComponentModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MTunnel.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DefaultPage : Page {
        private DefaultPageViewModel ViewModel { get; } = new DefaultPageViewModel();

        private bool IsLogviewerOpen {
            get => ProcessHandler.Instance.IsLogViewerEnabled;
        }

        public DefaultPage() {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e) {
            if (!ViewModel.CanConnect)
                return;

            ProcessHandler.Instance.StartAsClient(ViewModel.Token, PortTextBox.Port);
            Frame.Navigate(typeof(ClientPage));
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e) {
            var control = new HostDialogControl();
            var dialog = new ContentDialog {
                Title = "Create New Host",
                Content = control,
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Create",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };

            dialog.PrimaryButtonClick += (s, args) => {
                ProcessHandler.Instance.StartAsHost(control.Port, control.Network);
                Frame.Navigate(typeof(ServerPage));
            };

            await dialog.ShowAsync();
        }

        private void LogviewerDisable(object sender, RoutedEventArgs e) {
            ProcessHandler.Instance.CloseLogViewer();
        }

        private void LogviewerEnable(object sender, RoutedEventArgs e) {
            ProcessHandler.Instance.OpenLogViewer();
        }
    }

    public partial class DefaultPageViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string token = string.Empty;
        public string Token {
            get => token;
            set {
                if (token == value)
                    return;
                token = value;
                OnPropertyChanged(nameof(Token));
                OnPropertyChanged(nameof(CanConnect));
            }
        }

        public bool CanConnect => !string.IsNullOrEmpty(Token);
    }
}

