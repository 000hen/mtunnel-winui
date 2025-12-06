using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTunnel.Components;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MTunnel.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DefaultPage : Page {
        private bool IsLogviewerOpen {
            get => ProcessHandler.Instance.IsLogViewerEnabled;
        }

        public DefaultPage() {
            InitializeComponent();
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
}
