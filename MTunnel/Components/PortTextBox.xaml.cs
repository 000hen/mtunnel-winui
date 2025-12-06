using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MTunnel.Components {
    public sealed partial class PortTextBox : UserControl {
        public int Port {
            get {
                if (int.TryParse(PortTextBlock.Text, out int portNumber)) {
                    return portNumber;
                }
                return -1;
            }
        }

        public PortTextBox() {
            InitializeComponent();
        }

        private void OnPortTextBoxBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args) {
            string newText = args.NewText;
            if (string.IsNullOrEmpty(newText)) {
                return;
            }

            bool isNumeric = int.TryParse(newText, out int portNumber);
            if (!isNumeric || portNumber < 0 || portNumber > 65535) {
                args.Cancel = true;
            }
        }
    }
}
