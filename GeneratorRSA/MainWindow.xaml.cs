using System;
using System.Windows;
using System.Numerics;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;

namespace GeneratorRSA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window , INotifyPropertyChanged
    {

        int[] xarray;
        int keyE;
        int keyN;
        int lenght;
        BackgroundWorker _bgworker = new BackgroundWorker();
        int _workerState;
        bool goOn = true;


        #region INotifyPropertyChanged Member
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public int WorkerState
        {
            get { return _workerState; }
            set
            {
                _workerState = value;
                if(PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("WorkerState"));
                        
                }
            }
        }


        

        public MainWindow()
        {
            InitializeComponent();
           

            generateButton.IsEnabled = false;

        }

        public String Generate(int e, int n, int lenght)
        {

            Random randomGen = new Random();
            int random = randomGen.Next(1, n);

            xarray = new int[lenght + 1];

            xarray[0] = random;

            String result = "";

            for(int i = 0; i < lenght; i++)
            {
                if(!goOn)
                {
                    return null;
                }

                BigInteger bigX = new BigInteger();
                bigX = BigInteger.ModPow(new BigInteger(xarray[i]), new BigInteger(e), new BigInteger(n));

                if(bigX % 2 == 0)
                {
                    result += "0";
                } else
                {
                    result += "1";
                }

                xarray[i + 1] = (int)bigX;
                WorkerState = i;
            }

            return result;
        }

        private void generateButton_Click(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(100);
            generateButton.IsEnabled = false;
            generateProgressBar.Minimum = 0;
            generateProgressBar.Maximum = lenght - 1;

            DataContext = this;
            goOn = true;
            _bgworker.DoWork += (s, f) =>
            {
                String result = Generate(keyE, keyN, lenght);
                if(result != null)
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                    {
                        stringRSA.Text = result;
                        generateButton.IsEnabled = true;
                    }));
                } else
                {
                    Console.WriteLine("Reset!");
                }
                
            };
            _bgworker.RunWorkerAsync();
            
           
        }

        private void saveToFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".txt";
            sfd.Filter = "Text documents (.txt)|*.txt";

            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, stringRSA.Text);
            }
        }

        private void keyETextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var tb = (TextBox)sender;
            int key;
            if(int.TryParse(tb.Text, out key))
            {
                keyE = key;
            } else
            {
                tb.Clear();
            }

            shouldGenButtonBeEnabled();

        }

        private void keyNTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var tb = (TextBox)sender;
            int key;
            if (int.TryParse(tb.Text, out key))
            {
                keyN = key;
            }
            else
            {
                tb.Clear();
            }

            shouldGenButtonBeEnabled();
        }

        private void lenghtTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var tb = (TextBox)sender;
            int key;
            if (int.TryParse(tb.Text, out key))
            {
                lenght = key;
            }
            else
            {
                tb.Clear();
            }

            shouldGenButtonBeEnabled();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);

        }

        private void shouldGenButtonBeEnabled()
        {
            if (keyETextBox.Text.Length > 0 && keyNTextBox.Text.Length > 0 && lenghtTextBox.Text.Length > 0)
            {
                generateButton.IsEnabled = true;
            }
            else
            {
                generateButton.IsEnabled = false;
            }
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            goOn = false;
            generateButton.IsEnabled = true;
            keyETextBox.Clear();
            keyNTextBox.Clear();
            lenghtTextBox.Clear();
            stringRSA.Clear();
            shouldGenButtonBeEnabled();
            WorkerState = 0;
        }
    }
}
