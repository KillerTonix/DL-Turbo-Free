using DL_Turbo_Free.Helper;
using Microsoft.Win32;
using System.IO;
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
                SrtSettingsStackPanel.Visibility = Visibility.Visible;


                ParsedSubtitle = ParseSubtitle.ParseSrt(file);
                string jsonString = System.Text.Encoding.UTF8.GetString(ParsedSubtitle);

                File.WriteAllText("debug_parsed_srt.json", jsonString);

                //MessageBox.Show($"Success! Json Size: {jsonString.Length} bytes.\n\nPreview:\n" + jsonString[..200] + "...");
            }
            else if (CheckFileType.GetFileType(file) == "ass")
            {
                InputFileFormat = "ass";
                XtoYRadioButton.Content = "ASS to SRT";
                XtoDocxRadioButton.Content = "ASS to DOCX";
                SrtSettingsStackPanel.Visibility = Visibility.Hidden;


                //ParsedSubtitle = ParseSubtitle.ParseSrt(file);
            }
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
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
    }
}