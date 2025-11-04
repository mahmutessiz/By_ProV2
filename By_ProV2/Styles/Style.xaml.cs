using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace By_ProV2.Styles
{
    public partial class Style : ResourceDictionary
    {
        public Style()
        {
            InitializeComponent(); // XAML'i yükle
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.Dispatcher.BeginInvoke(new Action(() =>
                {
                    textBox.SelectAll();
                }), DispatcherPriority.Input);
            }
        }
    }
}