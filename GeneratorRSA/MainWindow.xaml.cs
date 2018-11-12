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
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

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
        BackgroundWorker _bgworker;
        int _workerState;
        bool goOn = true;
        String time;
        int random;
        Byte[] bytesFromInput;
        String binaryText;
        String result;

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
            //opisTextBlock.Text = "Program \"RSA generator\" służy do generowania ciągu pseudolosowych bitów. Najpierw należy podać klucz i długość ciągu, który chcemy otrzymać. Zarówno pola klucza i długości są zabezpieczone przed wprowadzeniem nieprawidłowych danych. Przycisk \"Generate\" aktywuję się, gdy wszystkie pola są wypełnione. Postęp pracy programu poakzuje nam progress bar. W każdej chwili za pomocą przycisku \"Reset\" można przerwać pracę programu i wprowadzić nowe dane wejściowe. Otrzymy ciąg znaków wyświetlany jest na ekranie. Przy pomocy przycisku \"Save to file\" możemy zapisać wynik w pliku tekstowym. W pierwszym wersie pliku tekstowego zapisany jest klucz i długość generowanego ciągu.\n\nZasada działania generatora RSA:\nDla klucza [e,n] liczba x0 < n jest losowo wybierana. Pseudolosowy bit si jest to LSB liczby xi, gdzie xi+1 = xi^e mod n.";

            generateButton.IsEnabled = false;

        }

        public String Generate(int e, int n, int lenght)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Random randomGen = new Random();
            random = randomGen.Next(1, n);

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

            sw.Stop();
            time = sw.Elapsed.ToString();
            return result;
        }

        public String Generate(int e, int n, int lenght, int rand)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            xarray = new int[lenght + 1];

            xarray[0] = rand;

            String result = "";

            for (int i = 0; i < lenght; i++)
            {
                if (!goOn)
                {
                    return null;
                }

                BigInteger bigX = new BigInteger();
                bigX = BigInteger.ModPow(new BigInteger(xarray[i]), new BigInteger(e), new BigInteger(n));

                if (bigX % 2 == 0)
                {
                    result += "0";
                }
                else
                {
                    result += "1";
                }

                xarray[i + 1] = (int)bigX;
                WorkerState = i;
            }

            sw.Stop();
            time = sw.Elapsed.ToString();
            return result;
        }


        private void generateButton_Click(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(100);
            timeTextBlock.Text = "";

            bytesFromInput = EncodeToBytes(textToEncryptTextBox.Text);
            Console.WriteLine("rozmiar tablicy" + bytesFromInput.Length);
            for(int i = 0; i< bytesFromInput.Length; i++)
            Console.WriteLine("pierwszy bajt" + bytesFromInput[i]);

            string[] tablicastringow = bytesFromInput.Select(x => Convert.ToString(x, 2).PadLeft(8, '0')).ToArray();
            binaryText = "";
            for (int i = 0; i < tablicastringow.Length; i++)
            {
                //if(i%2==0)
                binaryText += tablicastringow[i];
            }

            //binaryText = DecodeToString(bytesFromInput);
            Console.WriteLine("teskst bin: " + binaryText);
            Console.WriteLine("rozmiar: " + binaryText.Length);

            lenght = bytesFromInput.Length * 8;

            generateButton.IsEnabled = false;
            generateProgressBar.Minimum = 0;
            generateProgressBar.Maximum = lenght - 1;
            

            int dlgRandom = randomTextBox.Text.Length;
            

            DataContext = this;
            goOn = true;
            _bgworker = new BackgroundWorker();
            _bgworker.DoWork += (s, f) =>
            {
                if (dlgRandom == 0)
                {
                    Console.WriteLine("tutaj 1");
                    result = Generate(keyE, keyN, lenght);
                }
                else
                {
                    Console.WriteLine("tutaj 2");
                    result = Generate(keyE, keyN, lenght, random);
                }
                if(result != null)
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                    {
                        stringRSA.Text = result;
                        ranodmTextBlock.Text = "Random: " + random;
                        timeTextBlock.Text = time;
                        generateButton.IsEnabled = true;
                    }));
                } else
                {
                    Console.WriteLine("Reset!");
                }
                
            };
            _bgworker.RunWorkerAsync();
            
           
        }

        /*private void saveToFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".txt";
            sfd.Filter = "Text documents (.txt)|*.txt";

            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, "keyE=" + keyE + " keyN=" + keyN + " lenght=" + lenght + " random=" + random + "\n" + stringRSA.Text);
            }
        }*/

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

       /* private void lenghtTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
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
        }*/

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);

        }

        private void shouldGenButtonBeEnabled()
        {
            if (keyETextBox.Text.Length > 0 && keyNTextBox.Text.Length > 0 && textToEncryptTextBox.Text.Length > 0)
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
            stringRSA.Clear();
            timeTextBlock.Text = "";
            shouldGenButtonBeEnabled();
            WorkerState = 0;
        }

        private void loadFromFileButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            String temp = EncryptInputWithKey(binaryText, result);
            Byte[] tab = GetBytesFromBinaryString(temp);
            OutputTextBlock.Text = DecodeToString(tab);
        }

        private void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("jestem w decrypt");
            Byte[] temp = EncodeToBytes(textToEncryptTextBox.Text);
            
            string[] tablicastringow = temp.Select(x => Convert.ToString(x, 2).PadLeft(8, '0')).ToArray();
            String output = "";
            for (int i = 0; i < tablicastringow.Length; i++)
            {
                output += tablicastringow[i];
            }

            String temp2 = decodeInput(output, stringRSA.Text);

            Byte[] tab = GetBytesFromBinaryString(temp2);
            
            for(int i =0; i< tab.Length; i++)
            {
                Console.Write(tab[0]);
            }

            OutputTextBlock.Text = DecodeToString(tab);
        }

        private void SaveToFileButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void randomTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = (TextBox)sender;
            int key;
            if (int.TryParse(tb.Text, out key))
            {
                random = key;
            }
            else
            {
                tb.Clear();
            }

            shouldGenButtonBeEnabled();
        }

        private void loadKeyFromFileButton_Click(object sender, RoutedEventArgs e)
        {

        }

        static byte[] EncodeToBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string DecodeToString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        private void textToEncryptTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            shouldGenButtonBeEnabled();
        }

        String EncryptInputWithKey(String input, String Key)
        {
            String encryptionResult = "";
            for(int i = 0; i < input.Length; i++)
            {
                if(input[i] == Key[i])
                {
                    encryptionResult += "0";
                }
                else
                {
                    encryptionResult += "1";
                }
            }

            return encryptionResult;
        }

        public Byte[] GetBytesFromBinaryString(String binary)
        {
            var list = new List<Byte>();

            for (int i = 0; i < binary.Length; i += 8)
            {
                String t = binary.Substring(i, 8);

                list.Add(Convert.ToByte(t, 2));
            }

            return list.ToArray();
        }

        String decodeInput(String encoded, String generator)
        {
            String decoded = "";
            for (int i = 0; i < encoded.Length; i++)
            {
                if (encoded[i] == '1' && generator[i] == '1') decoded += "0";
                if (encoded[i] == '0' && generator[i] == '0') decoded += "0";
                if (encoded[i] == '1' && generator[i] == '0') decoded += "1";
                if (encoded[i] == '0' && generator[i] == '1') decoded += "1";
            }

            return decoded;
        }


    }
}
