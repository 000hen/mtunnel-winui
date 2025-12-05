using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using MTunnel.Pages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MTunnel
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;

            AppWindow.Resize(new Windows.Graphics.SizeInt32(400, 600));
            var _presenter = AppWindow.Presenter as OverlappedPresenter;

            if (_presenter != null)
            {
                _presenter.IsResizable = false;
                _presenter.IsMaximizable = false;
            }

            MainFrame.Navigate(typeof(DefaultPage));
        }
    }
}
