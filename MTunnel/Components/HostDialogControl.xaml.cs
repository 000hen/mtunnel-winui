using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MTunnel.Components {
    public sealed partial class HostDialogControl : UserControl {
        public string Network => ProtocolBox.SelectionBoxItem.ToString() ?? string.Empty;
        public int Port => PortTextBox.Port;

        public HostDialogControl() {
            InitializeComponent();
        }
    }
}
