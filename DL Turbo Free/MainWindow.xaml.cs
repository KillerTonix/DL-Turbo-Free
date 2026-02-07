using DL_Turbo_Free.Contexts;
using DL_Turbo_Free.Helper;
using DL_Turbo_Free.Models;
using DL_Turbo_Free.Services;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DL_Turbo_Free
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] ParsedSubtitle = [];
        private string InputFileFormat = string.Empty;
        private string InputFilePath = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            FileOpenOrDragAndDropBorder.Visibility = Visibility.Visible;
        }

        private void FileOpenOrDragAndDropBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Subtitle files (*.ass;*.srt)| *.ass; *.srt",
                Multiselect = false,
            };

            if (openFileDialog.ShowDialog() == true)
            {
                FileOpenOrDragAndDropBorder.Visibility = Visibility.Hidden;


                DoUIChanges(openFileDialog.FileName);
                InputFilePath = openFileDialog.FileName;
            }
        }

        private void FileOpenOrDragAndDropBorder_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string fileType = CheckFileType.GetFileType(files[0]);

                if (fileType == "Unknown File Type")
                {
                    MessageBox.Show("Unsupported file type.\nPlease upload a valid subtitle file.\nIt must be '.ass' and '.srt' subtitle format!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                FileOpenOrDragAndDropBorder.Visibility = Visibility.Hidden;
                DoUIChanges(files[0]);
                InputFilePath = files[0];
            }
        }

        private void DoUIChanges(string file)
        {
            
            string? version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

            bool openFolderAfterConversion = GlobalSettings.Default.OpenTheFolderAfterConvertion;
            bool closeAppAfterConversion = GlobalSettings.Default.CloseTheAppAfterConvertion;
            int fontSizeIndex = GlobalSettings.Default.DocxFontSizeIndex;
            int tableStyleIndex = GlobalSettings.Default.DocxTableStyleIndex;
            int timingFormatIndex = GlobalSettings.Default.DocxTimingFormatIndex;


            OpenTheFolderAfterConvertionChkBox.IsChecked = openFolderAfterConversion;
            CloseTheAppAfterConvertionChkBox.IsChecked = closeAppAfterConversion;
            FontSizeComboBox.SelectedIndex = fontSizeIndex;
            TableStyleComboBox.SelectedIndex = tableStyleIndex;
            TimingFormatComboBox.SelectedIndex = timingFormatIndex;
            VersionLabel.Content = $"Version: {version}";

            ClearAllData();
            FilePathTextBox.Text = file;


            if (CheckFileType.GetFileType(file) == "srt")
            {
                InputFileFormat = "srt";
                XtoYRadioButton.Content = "SRT to ASS";
                XtoDocxRadioButton.Content = "SRT to DOCX";
                SrtSettingsStackPanel.Visibility = Visibility.Collapsed;
                ProcessingRulesBorder.Visibility = Visibility.Visible;
            }
            else if (CheckFileType.GetFileType(file) == "ass")
            {
                InputFileFormat = "ass";
                XtoYRadioButton.Content = "ASS to SRT";
                XtoDocxRadioButton.Content = "ASS to DOCX";
                ProcessingRulesBorder.Visibility = Visibility.Collapsed;
            }

        }



        // <<<-----Conversion Button----->>>
        private async void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            ConvertButton.IsEnabled = false; // Prevent double clicks
            ParsedSubtitle = [];

            string folder = Path.GetDirectoryName(FilePathTextBox.Text) ?? string.Empty;
            string filename = Path.GetFileNameWithoutExtension(FilePathTextBox.Text);
            string outputPath = string.Empty;


            if (InputFileFormat == "srt")
                ParsedSubtitle = ParseSubtitle.ParseSrt(InputFilePath);
            else if (InputFileFormat == "ass")
                ParsedSubtitle = ParseSubtitle.ParseAss(InputFilePath);
            bool isDynamic = false;
            if (SetDynamicTiming.IsChecked == true)
                isDynamic = true;

            if (XtoDocxRadioButton.IsChecked == true)
            {
                try
                {
                    List<SubtitleItem>? rawData = JsonSerializer.Deserialize(ParsedSubtitle, AppJsonContext.Default.ListSubtitleItem);
                    if (rawData == null || rawData.Count == 0)
                    {
                        MessageBox.Show("Error: No subtitles found in file.");
                        return;
                    }

                    var correctedData = SrtService.FillMissingActors(rawData);
                    var merger = new ScriptMerger();
                    var mergedLines = merger.MergeScript(correctedData, isDynamic);
                    var generator = new DocxService();

                    outputPath = Path.Combine(folder, $"{filename}.docx");
                    generator.GenerateDocx(outputPath, mergedLines);
                }
                catch (Exception ex)
                {
                    ExceptionMessages.ShowMessage(ex);
                }
            }
            else
            {
                try
                {
                    List<SubtitleItem>? rawData = JsonSerializer.Deserialize(ParsedSubtitle, AppJsonContext.Default.ListSubtitleItem);
                    if (rawData == null || rawData.Count == 0)
                    {
                        MessageBox.Show("Error: No subtitles found in file.");
                        return;
                    }

                    var correctedData = SrtService.FillMissingActors(rawData);
                    var generator = new DocxService();
                    if (InputFileFormat == "srt")
                    {
                        outputPath = Path.Combine(folder, $"{filename}.ass");
                        SrtToAss.ConvertSrtToAss(outputPath, correctedData);
                    }
                    else if (InputFileFormat == "ass")
                    {
                        outputPath = Path.Combine(folder, $"{filename}.srt");
                        bool separateFiles = CharactersInSeparateFilesRadioButton.IsChecked ?? false;
                        AssToSrt.ConvertAssToSrt(outputPath, correctedData, separateFiles);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionMessages.ShowMessage(ex);
                }
            }

            ConvertButton.IsEnabled = true;
            SuccessMessageBox(folder, outputPath); //show success message box
        }


        private void SuccessMessageBox(string folder, string outputPath)
        {
            if (OpenTheFolderAfterConvertionChkBox.IsChecked == true)
                OpenTheFolderAfterConvertion(outputPath);
            else
            {
                if (MessageBox.Show("Conversion completed successfully!\nDo you want to open the output folder?", "Success", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    if (Directory.Exists(folder))
                        OpenTheFolderAfterConvertion(outputPath);
                    else
                        MessageBox.Show("Output folder not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearAllData()
        {
            ParsedSubtitle = [];
            InputFileFormat = string.Empty;
            InputFilePath = string.Empty;
            FilePathTextBox.Text = string.Empty;
            SrtSettingsStackPanel.Visibility = Visibility.Collapsed;
            ProcessingRulesBorder.Visibility = Visibility.Collapsed;
        }

        private void OpenTheFolderAfterConvertion(string outputPath)
        {
            Process.Start(new ProcessStartInfo { FileName = "explorer", Arguments = $"/n, /select,{outputPath}" });
            if (CloseTheAppAfterConvertionChkBox.IsChecked == true)
                Application.Current.Shutdown();
        }


        private void XtoDocxRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SrtSettingsStackPanel?.Visibility = Visibility.Collapsed;
            DocxSettingsStackPanel?.Visibility = Visibility.Visible;
        }

        private void XtoYRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (InputFileFormat == "ass")
                SrtSettingsStackPanel.Visibility = Visibility.Visible;

            DocxSettingsStackPanel.Visibility = Visibility.Collapsed;
        }

        private void SetSeparatorBtn_Click(object sender, RoutedEventArgs e)
        {
            SetSeparatorWindow separatorWindow = new();
            separatorWindow.ShowDialog();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Subtitle files (*.ass;*.srt)| *.ass; *.srt",
                Multiselect = false,
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string file = openFileDialog.FileName;
                DoUIChanges(file);
                InputFilePath = file;

            }
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow about = new();
            about.ShowDialog();
        }

        private void OpenTheFolderAfterConvertionChkBox_Checked(object sender, RoutedEventArgs e)
        {
            GlobalSettings.Default.OpenTheFolderAfterConvertion = true;
            GlobalSettings.Default.Save();
        }

        private void OpenTheFolderAfterConvertionChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            GlobalSettings.Default.OpenTheFolderAfterConvertion = false;
            GlobalSettings.Default.Save();
        }

        private void CloseTheAppAfterConvertionChkBox_Checked(object sender, RoutedEventArgs e)
        {
            GlobalSettings.Default.CloseTheAppAfterConvertion = true;
            GlobalSettings.Default.Save();
        }

        private void CloseTheAppAfterConvertionChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            GlobalSettings.Default.CloseTheAppAfterConvertion = false;
            GlobalSettings.Default.Save();
        }

        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            GlobalSettings.Default.DocxFontSizeIndex = cmb.SelectedIndex;
            if (cmb.SelectedItem is ComboBoxItem selectedItem)
            {
                if (int.TryParse(selectedItem.Content.ToString(), out int size))                
                    GlobalSettings.Default.DocxFontSizeValue = size;                
            }
            GlobalSettings.Default.Save();
        }

        private void TableStyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            GlobalSettings.Default.DocxTableStyleIndex = cmb.SelectedIndex;
            if (cmb.SelectedItem is ComboBoxItem selectedItem)
            {
                if (selectedItem.Content.ToString() != null)
                    GlobalSettings.Default.DocxTableStyleValue = selectedItem.Content.ToString();
            }
            GlobalSettings.Default.Save();
        }

        private void TimingFormatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            GlobalSettings.Default.DocxTimingFormatIndex = cmb.SelectedIndex;
            if (cmb.SelectedItem is ComboBoxItem selectedItem)
            {
                if (selectedItem.Content.ToString() != null)
                    GlobalSettings.Default.DocxTimingFormatValue = selectedItem.Content.ToString();
            }
            GlobalSettings.Default.Save();
        }

    }
}