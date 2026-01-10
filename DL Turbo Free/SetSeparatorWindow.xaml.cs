using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace DL_Turbo_Free
{
    /// <summary>
    /// Interaction logic for SetSeparatorWindow.xaml
    /// </summary>
    public partial class SetSeparatorWindow : Window
    {
        public SetSeparatorWindow()
        {
            InitializeComponent();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem comboboxSelectedItem = (ComboBoxItem)SeparatorTypeComboBox.SelectedItem;
            string? value = comboboxSelectedItem.Content.ToString() ?? string.Empty;

            GlobalSettings.Default.SelectedSeparatorType = value; 
            GlobalSettings.Default.SelectedSeparatorIndex = SeparatorTypeComboBox.SelectedIndex;
            GlobalSettings.Default.Save();

            this.DialogResult = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SeparatorTypeComboBox.SelectedIndex = GlobalSettings.Default.SelectedSeparatorIndex;
        }
    }
}
