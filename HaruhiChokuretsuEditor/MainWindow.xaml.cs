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
        private EventFile _event;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenEventsFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "EVT file|*.evt"
            };
            if (openFileDialog.ShowDialog() == true)
            {

            }
        }

        private void SaveEventsFileButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void InsertEv000Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "EVT file|*.bin"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                byte[] binFileBytes = File.ReadAllBytes(openFileDialog.FileName);
                OpenFileDialog evtFileDialog = new OpenFileDialog
                {
                    Filter = "EVT file|*.bin"
                };
                if (evtFileDialog.ShowDialog() == true)
                {
                    byte[] evtFileBytes = Helpers.CompressData(File.ReadAllBytes(evtFileDialog.FileName));
                    for (int i = 0; i < 0xFF0; i++)
                    {
                        if (i < evtFileBytes.Length)
                        {
                            binFileBytes[i + 0x12F800] = evtFileBytes[i];
                        }
                        else
                        {
                            binFileBytes[i + 0x12F800] = 0x00;
                        }
                    }
                    File.WriteAllBytes(openFileDialog.FileName, binFileBytes);
                }
            }
        }
    }
}
