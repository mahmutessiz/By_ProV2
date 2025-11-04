using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace By_ProV2.Helpers
{
    public static class PlaceholderService
    {
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached(
                "Placeholder",
                typeof(string),
                typeof(PlaceholderService),
                new PropertyMetadata(string.Empty, OnPlaceholderChanged));

        private static readonly DependencyProperty IsPlaceholderActiveProperty =
            DependencyProperty.RegisterAttached(
                "IsPlaceholderActive",
                typeof(bool),
                typeof(PlaceholderService),
                new PropertyMetadata(false));

        public static string GetPlaceholder(DependencyObject obj)
            => (string)obj.GetValue(PlaceholderProperty);

        public static void SetPlaceholder(DependencyObject obj, string value)
            => obj.SetValue(PlaceholderProperty, value);

        public static bool GetIsPlaceholderActive(DependencyObject obj)
            => (bool)obj.GetValue(IsPlaceholderActiveProperty);

        public static void SetIsPlaceholderActive(DependencyObject obj, bool value)
            => obj.SetValue(IsPlaceholderActiveProperty, value);

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.Loaded -= TextBox_Loaded;
                textBox.GotFocus -= TextBox_GotFocus;
                textBox.LostFocus -= TextBox_LostFocus;

                if (!string.IsNullOrEmpty(GetPlaceholder(textBox)))
                {
                    textBox.Loaded += TextBox_Loaded;
                    textBox.GotFocus += TextBox_GotFocus;
                    textBox.LostFocus += TextBox_LostFocus;
                }
            }
        }

        private static void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    SetPlaceholderVisual(textBox);
                }
            }
        }

        private static void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && GetIsPlaceholderActive(textBox))
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
                SetIsPlaceholderActive(textBox, false);
            }
        }

        private static void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    SetPlaceholderVisual(textBox);
                }
            }
        }

        private static void SetPlaceholderVisual(TextBox textBox)
        {
            textBox.Text = GetPlaceholder(textBox);
            textBox.Foreground = Brushes.Gray;
            SetIsPlaceholderActive(textBox, true);
        }
    }
}
