using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MTunnel.Components {
    public sealed partial class ControlOptionsControl : UserControl {
        public event EventHandler<RoutedEventArgs>? TerminateClick;
        public event EventHandler<RoutedEventArgs>? ForceTerminateClick;

        public ControlOptionsControl() {
            InitializeComponent();
        }

        private void OnTerminateClick(object sender, RoutedEventArgs e) {
            if (InputKeyboardSource
                .GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift)
                .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down)) {
                var dialog = new ContentDialog {
                    Title = "Force terminate tunnel",
                    Content = "Are you sure you want to forcibly terminate the tunnel process? This may cause data loss for connected peers.",
                    CloseButtonText = "Cancel",
                    PrimaryButtonText = "Terminate",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = XamlRoot
                };

                dialog.PrimaryButtonClick += (s, args) => {
                    ForceTerminateClick?.Invoke(this, e);
                };

                _ = dialog.ShowAsync();
                return;
            }

            TerminateClick?.Invoke(this, e);
        }

        public string Token {
            get { return (string)GetValue(TokenProperty); }
            set { SetValue(TokenProperty, value); }
        }
        public static readonly DependencyProperty TokenProperty =
            DependencyProperty.Register(nameof(Token), typeof(string), typeof(CopyButton), new PropertyMetadata(string.Empty));
    }
}
