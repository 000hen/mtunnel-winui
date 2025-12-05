using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MTunnel.Components
{
    public sealed partial class ButtonIconText : UserControl
    {
        public event EventHandler<RoutedEventArgs> Click;

        public ButtonIconText()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ButtonIconText), new PropertyMetadata(string.Empty));

        public string IconGlyph
        {
            get => (string)GetValue(IconGlyphProperty);
            set => SetValue(IconGlyphProperty, value);
        }

        public static readonly DependencyProperty IconGlyphProperty =
            DependencyProperty.Register(nameof(IconGlyph), typeof(string), typeof(ButtonIconText), new PropertyMetadata("\uE700"));

        private void OnInternalButtonClick(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }
    }
}
