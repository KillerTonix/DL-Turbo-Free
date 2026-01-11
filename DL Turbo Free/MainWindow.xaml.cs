using DL_Turbo_Free.Contexts;
using DL_Turbo_Free.Helper;
using DL_Turbo_Free.Models;
using DL_Turbo_Free.Services;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
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

        public MainWindow()
        {
            InitializeComponent();
            ConvertingSettingsStackPanel.Visibility = Visibility.Hidden;
            FileOpenOrDragAndDropBorder.Visibility = Visibility.Visible;
        }

        private void FileOpenOrDragAndDropBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                OpenFileDialog openFileDialog = new()
                {
                    Filter = "Subtitle files (*.ass;*.srt)| *.ass; *.srt",
                    Multiselect = false,
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string file = openFileDialog.FileName;
                    FileOpenOrDragAndDropBorder.Visibility = Visibility.Hidden;
                    DoUIChanges(file); //do ui changes
                }
            }
        }

        private void FileOpenOrDragAndDropBorder_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                string fileType = CheckFileType.GetFileType(files[0]);
                if (fileType == "Unknown File Type")
                {
                    MessageBox.Show("Unsupported file type.\nPlease upload a valid subtitle file.\nIt must be '.ass' and '.srt' subtitle format!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                FileOpenOrDragAndDropBorder.Visibility = Visibility.Hidden;
                DoUIChanges(files[0]); //do ui changes
            }
        }

        private void FileOpenOrDragAndDropBorder_DragEnter(object sender, DragEventArgs e)
        {
            FileOpenOrDragAndDropLabel.Content = "Release to drop the file";
        }

        private void FileOpenOrDragAndDropBorder_DragLeave(object sender, DragEventArgs e)
        {
            FileOpenOrDragAndDropLabel.Content = "Drag and Drop your file here or click to browse or double click";
        }


        private void DoUIChanges(string file)
        {

            FilePathTextBox.Text = file;
            if (CheckFileType.GetFileType(file) == "srt")
            {
                InputFileFormat = "srt";
                XtoYRadioButton.Content = "SRT to ASS";
                XtoDocxRadioButton.Content = "SRT to DOCX";
                SrtSettingsStackPanel.Visibility = Visibility.Hidden;
                SetSeparatorBtn.Visibility = Visibility.Hidden;

                ParsedSubtitle = ParseSubtitle.ParseSrt(file);


            }
            else if (CheckFileType.GetFileType(file) == "ass")
            {
                InputFileFormat = "ass";
                XtoYRadioButton.Content = "ASS to SRT";
                XtoDocxRadioButton.Content = "ASS to DOCX";
                SrtSettingsStackPanel.Visibility = Visibility.Visible;
                SetSeparatorBtn.Visibility = Visibility.Visible;

                ParsedSubtitle = ParseSubtitle.ParseAss(file);
            }

            ConvertingSettingsStackPanel.Visibility = Visibility.Visible;
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            string folder = Path.GetDirectoryName(FilePathTextBox.Text) ?? string.Empty;
            string filename = Path.GetFileNameWithoutExtension(FilePathTextBox.Text);
            string outputPath = string.Empty;

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
                    var mergedLines = merger.MergeScript(correctedData);
                    var generator = new DocxService();

                    outputPath = Path.Combine(folder, $"{filename}.docx");
                    generator.GenerateDocx(outputPath, mergedLines);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
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
                    var merger = new ScriptMerger();
                    var mergedLines = merger.MergeScript(correctedData);
                    var generator = new DocxService();

                    outputPath = Path.Combine(folder, $"{filename}.srt");
                    bool separateFiles = CharactersInSeparateFilesRadioButton.IsChecked ?? false;
                    AssToSrt.ConvertAssToSrt(outputPath, mergedLines, separateFiles);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }

            }


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

        private void OpenTheFolderAfterConvertion(string outputPath)
        {
            Process.Start(new ProcessStartInfo { FileName = "explorer", Arguments = $"/n, /select,{outputPath}" });
            if (CloseTheAppAfterConvertionChkBox.IsChecked == true)
                Application.Current.Shutdown();
        }


        private void XtoDocxRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (SrtSettingsStackPanel.Visibility == Visibility.Visible)
                SrtSettingsStackPanel.Visibility = Visibility.Hidden;
        }

        private void XtoYRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (InputFileFormat == "ass")
                SrtSettingsStackPanel.Visibility = Visibility.Visible;
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
                DoUIChanges(file); //do ui changes
            }
        }

        private void SrtSettingsStackPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (SrtSettingsStackPanel.Visibility == Visibility.Hidden)
            {
                SetSeparatorBtn.Visibility = Visibility.Hidden;
            }
            else
            {
                SetSeparatorBtn.Visibility = Visibility.Visible;
            }
        }
    }
}