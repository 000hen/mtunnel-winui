using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MTunnel.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DefaultPage : Page
    {
        public DefaultPage()
        {
            InitializeComponent();
        }

        private void OnPortTextBoxBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            // Check the new text content that would result from this change
            string newText = args.NewText;

            // Allow empty string so the user can delete everything to re-type
            if (string.IsNullOrEmpty(newText))
            {
                return;
            }

            // Try to parse the text as an integer
            // And check if it falls within the valid port range (0 - 65535)
            bool isNumeric = int.TryParse(newText, out int portNumber);

            if (!isNumeric || portNumber < 0 || portNumber > 65535)
            {
                // If not numeric or out of range, cancel the text change
                args.Cancel = true;
            }
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ContentDialog
            {
                Title = "Notice",
                Content = "Clicked!",
                CloseButtonText = "OK"
            };
            dlg.XamlRoot = Content.XamlRoot;
            await dlg.ShowAsync();
        }
    }
}
