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
        private FileSystemFile<GraphicsFile> _grpFile;
        private FileSystemFile<DataFile> _datFile;

        private int _currentImageWidth = 256;

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
            if (eventsListBox.SelectedIndex >= 0)
            {
                EventFile selectedFile = (EventFile)eventsListBox.SelectedItem;
                for (int i = 0; i < selectedFile.DialogueLines.Count; i++)
                {
                    StackPanel dialogueStackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    dialogueStackPanel.Children.Add(new TextBlock { Text = $"{selectedFile.DialogueLines[i].Speaker} ({selectedFile.DialogueLines[i].SpeakerName}):\t" });
                    EventTextBox textBox = new() { EventFile = selectedFile, DialogueIndex = i, Text = selectedFile.DialogueLines[i].Text, AcceptsReturn = true };
                    textBox.TextChanged += TextBox_TextChanged;
                    dialogueStackPanel.Children.Add(textBox);
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EventTextBox textBox = (EventTextBox)sender;
            textBox.EventFile.EditDialogueLine(textBox.DialogueIndex, textBox.Text);
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
            if (eventsListBox.SelectedIndex >= 0)
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

        private void OpenGraphicsFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "GRP file|grp*.bin"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                _grpFile = FileSystemFile<GraphicsFile>.FromFile(openFileDialog.FileName);
                _grpFile.Files.First(f => f.Index == 0xE50).InitializeFontFile(); // initialize the font file
                graphicsStatsStackPanel.Children.Clear();
                graphicsListBox.ItemsSource = _grpFile.Files;
                graphicsListBox.Items.Refresh();

                // Gather stats on the header values for research purposes
                Dictionary<int, Dictionary<ushort, int>> statsDictionaries = new();
                for (int i = 0x06; i < 0x14; i += 2)
                {
                    statsDictionaries.Add(i, new Dictionary<ushort, int>());
                }

                foreach (GraphicsFile file in _grpFile.Files)
                {
                    if (file.Data is null || Encoding.ASCII.GetString(file.Data.Take(6).ToArray()) != "SHTXDS")
                    {
                        continue;
                    }

                    for (int i = 0x06; i < 0x14; i += 2)
                    {
                        ushort value = BitConverter.ToUInt16(file.Data.Skip(i).Take(2).ToArray());
                        if (statsDictionaries[i].ContainsKey(value))
                        {
                            statsDictionaries[i][value]++;
                        }
                        else
                        {
                            statsDictionaries[i].Add(value, 1);
                        }
                    }
                }

                foreach (int offset in statsDictionaries.Keys)
                {
                    graphicsStatsStackPanel.Children.Add(new TextBlock { Text = $"0x{offset:X2}", FontSize = 16 });

                    foreach (ushort value in statsDictionaries[offset].Keys)
                    {
                        StackPanel statStackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                        statStackPanel.Children.Add(new TextBlock { Text = string.Concat(BitConverter.GetBytes(value).Select(b => $"{b:X2} ")) });
                        statStackPanel.Children.Add(new TextBlock { Text = $"{statsDictionaries[offset][value]}" });
                        graphicsStatsStackPanel.Children.Add(statStackPanel);
                    }

                    graphicsStatsStackPanel.Children.Add(new Separator());
                }
            }
        }

        private void SaveGraphicsFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "GRP file|grp*.bin"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllBytes(saveFileDialog.FileName, _grpFile.GetBytes());
                MessageBox.Show("Save completed!");
            }
        }

        private void ExportGraphicsFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "BIN file|*.bin"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllBytes(saveFileDialog.FileName, ((GraphicsFile)graphicsListBox.SelectedItem).GetBytes());
            }
        }

        private void ImportGraphicsFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "BIN file|*.bin"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                GraphicsFile newGraphicsFile = new();
                newGraphicsFile.Initialize(File.ReadAllBytes(openFileDialog.FileName), _grpFile.Files[graphicsListBox.SelectedIndex].Offset);
                newGraphicsFile.Index = _grpFile.Files[graphicsListBox.SelectedIndex].Index;
                _grpFile.Files[graphicsListBox.SelectedIndex] = newGraphicsFile;
                graphicsListBox.Items.Refresh();
            }
        }

        private void ExportGraphicsImageFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (graphicsListBox.SelectedIndex >= 0)
            {
                SaveFileDialog saveFileDialog = new()
                {
                    Filter = "PNG file|*.png"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    GraphicsFile selectedFile = (GraphicsFile)graphicsListBox.SelectedItem;
                    System.Drawing.Bitmap bitmap = selectedFile.GetImage(_currentImageWidth);
                    bitmap.Save(saveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }

        private void ImportGraphicsImageFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (graphicsListBox.SelectedIndex >= 0 && (((GraphicsFile)graphicsListBox.SelectedItem).PixelData?.Count ?? 0) > 0)
            {
                OpenFileDialog openFileDialog = new()
                {
                    Filter = "PNG file|*.png"
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    GraphicsFile selectedFile = (GraphicsFile)graphicsListBox.SelectedItem;
                    int width = selectedFile.SetImage(openFileDialog.FileName);
                    tilesEditStackPanel.Children.RemoveAt(tilesEditStackPanel.Children.Count - 1);
                    tilesEditStackPanel.Children.Add(new Image { Source = Helpers.GetBitmapImageFromBitmap(selectedFile.GetImage(width)), MaxWidth = 256 });
                    _currentImageWidth = width;
                }
            }
        }

        private void GraphicsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tilesEditStackPanel.Children.Clear();
            if (graphicsListBox.SelectedIndex >= 0)
            {
                GraphicsFile selectedFile = (GraphicsFile)graphicsListBox.SelectedItem;
                tilesEditStackPanel.Children.Add(new TextBlock { Text = $"{selectedFile.Data?.Count ?? 0} bytes" });
                tilesEditStackPanel.Children.Add(new TextBlock { Text = $"{_grpFile.SecondHeaderNumbers[graphicsListBox.SelectedIndex]:X8}" });
                if (selectedFile.PixelData is not null)
                {
                    ShtxdsWidthBox graphicsWidthBox = new ShtxdsWidthBox { Shtxds = selectedFile, Text = "256" };
                    graphicsWidthBox.TextChanged += GraphicsWidthBox_TextChanged;
                    tilesEditStackPanel.Children.Add(graphicsWidthBox);
                    tilesEditStackPanel.Children.Add(new Image { Source = Helpers.GetBitmapImageFromBitmap(selectedFile.GetImage()), MaxWidth = 256 });
                    _currentImageWidth = 256;
                }
            }
        }

        private void GraphicsWidthBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            tilesEditStackPanel.Children.RemoveAt(tilesEditStackPanel.Children.Count - 1);
            ShtxdsWidthBox widthBox = (ShtxdsWidthBox)sender;
            bool successfulParse = int.TryParse(widthBox.Text, out int width);
            if (!successfulParse)
            {
                width = 256;
            }
            tilesEditStackPanel.Children.Add(new Image { Source = Helpers.GetBitmapImageFromBitmap(widthBox.Shtxds.GetImage(width)), MaxWidth = 256 });
            _currentImageWidth = width;
        }

        private void OpenDataFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "DAT file|dat*.bin"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                _datFile = FileSystemFile<DataFile>.FromFile(openFileDialog.FileName);
                dataListBox.ItemsSource = _datFile.Files;
                dataListBox.Items.Refresh();
            }
        }

        private void SaveDataFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "DAT file|dat*.bin"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllBytes(saveFileDialog.FileName, _datFile.GetBytes());
                MessageBox.Show("Save completed!");
            }
        }

        private void ExportDataFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "BIN file|*.bin"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllBytes(saveFileDialog.FileName, ((DataFile)dataListBox.SelectedItem).GetBytes());
            }
        }

        private void ImportDataFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "BIN file|*.bin"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                DataFile newDataFile = new();
                newDataFile.Initialize(File.ReadAllBytes(openFileDialog.FileName), _datFile.Files[dataListBox.SelectedIndex].Offset);
                newDataFile.Index = _datFile.Files[dataListBox.SelectedIndex].Index;
                _datFile.Files[dataListBox.SelectedIndex] = newDataFile;
                graphicsListBox.Items.Refresh();
            }
        }

        private void DataListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dataEditStackPanel.Children.Clear();
            if (dataListBox.SelectedIndex >= 0)
            {
                DataFile selectedFile = (DataFile)dataListBox.SelectedItem;
                dataEditStackPanel.Children.Add(new TextBlock { Text = $"{selectedFile.Data.Count} bytes" });
            }
        }
    }
}
