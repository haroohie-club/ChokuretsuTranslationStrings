using FolderBrowserEx;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HaruhiChokuretsuEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileSystemFile<EventFile> _evtFile;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenEventsFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "EVT file|evt*.bin"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                _evtFile = FileSystemFile<EventFile>.FromFile(openFileDialog.FileName);
                eventsListBox.ItemsSource = _evtFile.Files;
                eventsListBox.Items.Refresh();
            }
        }

        private void SaveEventsFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "EVT file|evt*.bin"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllBytes(saveFileDialog.FileName, _evtFile.GetBytes());
                MessageBox.Show("Save completed!");
            }
        }

        private void ExportEventsFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "BIN file|*.bin"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllBytes(saveFileDialog.FileName, ((EventFile)eventsListBox.SelectedItem).GetBytes());
            }
        }

        private void ImportEventsFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "BIN file|*.bin"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                EventFile newEventFile = new();
                newEventFile.Initialize(File.ReadAllBytes(openFileDialog.FileName), _evtFile.Files[eventsListBox.SelectedIndex].Offset);
                _evtFile.Files[eventsListBox.SelectedIndex] = newEventFile;
                eventsListBox.Items.Refresh();
            }
        }

        private void EventsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            editStackPanel.Children.Clear();
            frontPointersStackPanel.Children.Clear();
            endPointersStackPanel.Children.Clear();
            if (eventsListBox.SelectedIndex > 0)
            {
                EventFile selectedFile = (EventFile)eventsListBox.SelectedItem;
                foreach (DialogueLine dialogueLine in selectedFile.DialogueLines)
                {
                    StackPanel dialogueStackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    dialogueStackPanel.Children.Add(new TextBlock { Text = $"{dialogueLine.Speaker} ({dialogueLine.SpeakerName}):\t" });
                    dialogueStackPanel.Children.Add(new TextBox { Text = dialogueLine.Text });
                    editStackPanel.Children.Add(dialogueStackPanel);
                }
                foreach (int frontPointer in selectedFile.FrontPointers)
                {
                    StackPanel fpStackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    fpStackPanel.Children.Add(new TextBlock { Text = $"0x{frontPointer:X8}\t\t" });
                    fpStackPanel.Children.Add(new TextBox { Text = $"{BitConverter.ToInt32(selectedFile.Data.Skip(frontPointer).Take(4).ToArray()):X8}" });
                    frontPointersStackPanel.Children.Add(fpStackPanel);
                }
                foreach (int endPointer in selectedFile.EndPointers)
                {
                    StackPanel epStackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    epStackPanel.Children.Add(new TextBlock { Text = $"0x{endPointer:X8}\t\t" });
                    epStackPanel.Children.Add(new TextBox { Text = $"{BitConverter.ToInt32(selectedFile.Data.Skip(endPointer).Take(4).ToArray()):X8}" });
                    endPointersStackPanel.Children.Add(epStackPanel);
                }
            }
        }

        private void ExportStringsEventsFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (eventsListBox.SelectedIndex > 0)
            {
                EventFile selectedFile = (EventFile)eventsListBox.SelectedItem;
                SaveFileDialog saveFileDialog = new()
                {
                    Filter = "RESX files|*.resx"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    selectedFile.WriteResxFile(saveFileDialog.FileName);
                }
            }
        }

        private void ImportStringsEventsFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (eventsListBox.SelectedIndex > 0)
            {
                EventFile selectedFile = (EventFile)eventsListBox.SelectedItem;
            }
        }

        private void ExportAllStringsEventsFileButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowser = new()
            {
                AllowMultiSelect = false
            };
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (EventFile eventFile in _evtFile.Files)
                {
                    eventFile.WriteResxFile(System.IO.Path.Combine(folderBrowser.SelectedFolder, $"{eventFile.Index:D3}.ja.resx"));
                }
            }
        }

        private void CompressFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "All files|*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                byte[] decompressedBytes = File.ReadAllBytes(openFileDialog.FileName);
                byte[] compressedBytes = Helpers.CompressData(decompressedBytes);

                SaveFileDialog saveFileDialog = new()
                {
                    Filter = "All files|*"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllBytes(saveFileDialog.FileName, compressedBytes);
                }
            }
        }

        private void DecompressFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "All files|*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                byte[] compressedBytes = File.ReadAllBytes(openFileDialog.FileName);
                byte[] decompressedBytes = Helpers.DecompressData(compressedBytes);

                SaveFileDialog saveFileDialog = new()
                {
                    Filter = "All files|*"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllBytes(saveFileDialog.FileName, decompressedBytes);
                }
            }
        }
    }
}
