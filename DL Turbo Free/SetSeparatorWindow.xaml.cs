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
            if (value == "Custom")
            {
                if (CustomSeparatorTextBox.Text == "Enter custom separator here..." || string.IsNullOrWhiteSpace(CustomSeparatorTextBox.Text) || !CustomSeparatorTextBox.Text.Contains("actor"))
                {
                    MessageBox.Show("Please enter a valid custom separator.\nE.g. actor*  or *actor*", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                value = CustomSeparatorTextBox.Text;
            }
            GlobalSettings.Default.SelectedSeparatorType = value;
            GlobalSettings.Default.SelectedSeparatorIndex = SeparatorTypeComboBox.SelectedIndex;
            GlobalSettings.Default.Save();

            this.DialogResult = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SeparatorTypeComboBox.SelectedIndex = GlobalSettings.Default.SelectedSeparatorIndex;
            if (SeparatorTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string? value = selectedItem.Content.ToString() ?? string.Empty;
                if (value == "Custom")
                {
                    CustomSeparatorSP.Visibility = Visibility.Visible;
                    CustomSeparatorTextBox.Text = GlobalSettings.Default.SelectedSeparatorType;
                }
                else
                {
                    CustomSeparatorSP.Visibility = Visibility.Collapsed;
                }
                UpdatePreview();
            }
        }


        private void SeparatorTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SeparatorTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string? value = selectedItem.Content.ToString() ?? string.Empty;

                if (value == "Custom")
                    CustomSeparatorSP.Visibility = Visibility.Visible;
                else
                    CustomSeparatorSP.Visibility = Visibility.Collapsed;

                UpdatePreview();
            }
        }
        private void UpdatePreview()
        {
            // 1. Get the current template
            string template = string.Empty;

            if (SeparatorTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                template = selectedItem.Content.ToString() ?? string.Empty;
            }

            // 2. If Custom, grab text from the TextBox
            if (template == "Custom")
            {
                template = CustomSeparatorTextBox.Text;
                // Handle the placeholder text
                if (template == "Enter custom separator here..." || string.IsNullOrWhiteSpace(template))
                {
                    PreviewTextBlock.Text = "Waiting for input...";
                    PreviewTextBlock.Foreground = System.Windows.Media.Brushes.Gray;
                    return;
                }
            }
                    
            if (template.Contains("actor"))
            {
                string sample = template.Replace("actor", "John Doe");
                PreviewTextBlock.Text = $"{sample} Hello there!";
                PreviewTextBlock.Foreground = System.Windows.Media.Brushes.LightGreen; // Success color
            }
            else
            {
                // Error state: The pattern doesn't contain the required keyword
                PreviewTextBlock.Text = "Error: Pattern must contain 'actor'";
                PreviewTextBlock.Foreground = System.Windows.Media.Brushes.Red; // Error color
            }
        }

        private void HintBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "You can use any character(s) as a custom separator.\n" +
                "For example, you can use a dash (actor\\), semicolon (actor;), pipe (actor|).\n\n" +
                "Separator can surround an actor:\n" +
                "e.g (\\actor\\) or (*actor*).\n\n" +
                "Just type your desired separator with actor keyword into the text box " +
                "and click Apply to set it.",
                "Custom Separator Hint",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void CustomSeparatorTextBox_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (CustomSeparatorTextBox.Text == "Enter custom separator here...")
            {
                CustomSeparatorTextBox.Text = string.Empty;
                UpdatePreview();
            }
        }

        private void CustomSeparatorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SeparatorTypeComboBox.SelectedItem is ComboBoxItem item && item.Content.ToString() == "Custom")
            {
                UpdatePreview();
            }
        }
    }
}
