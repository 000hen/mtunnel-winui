using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MTunnel.Components {
    public sealed partial class CopyButton : UserControl {
        public CopyButton() {
            InitializeComponent();
        }

        public string Token {
            get { return (string)GetValue(TokenProperty); }
            set { SetValue(TokenProperty, value); }
        }
        public static readonly DependencyProperty TokenProperty =
            DependencyProperty.Register(nameof(Token), typeof(string), typeof(CopyButton), new PropertyMetadata(string.Empty));

        private void OnCopyClick(object sender, RoutedEventArgs e) {
            var data = new DataPackage {
                RequestedOperation = DataPackageOperation.Copy
            };
            data.SetText(Token);

            Clipboard.SetContent(data);
        }
    }
}
