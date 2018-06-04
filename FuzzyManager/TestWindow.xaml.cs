using Fuzzy;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;

namespace FuzzyManager
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        public TestWindow()
        {
            InitializeComponent();
        }

        private void FuzzyButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();

            openFileDialog.DefaultExt = ".txt";
            openFileDialog.Filter = "Text file (*.txt, *.fis, *.fisx)|*.txt;*.fis;*.fisx";
            openFileDialog.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

            Nullable<bool> result = openFileDialog.ShowDialog();

            if (!result.HasValue || !result.Value)
                return;

            FuzzySystemText.Text = openFileDialog.FileName;
        }

        private void InputValuesButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();

            openFileDialog.DefaultExt = ".txt";
            openFileDialog.Filter = "Text file (*.txt)|*.txt";
            openFileDialog.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

            Nullable<bool> result = openFileDialog.ShowDialog();

            if (!result.HasValue || !result.Value)
                return;

            InputValuesText.Text = openFileDialog.FileName;
        }

        private void OutputValuesButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();

            openFileDialog.DefaultExt = ".txt";
            openFileDialog.Filter = "Text file (*.txt)|*.txt";
            openFileDialog.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

            Nullable<bool> result = openFileDialog.ShowDialog();

            if (!result.HasValue || !result.Value)
                return;

            OutputValuesText.Text = openFileDialog.FileName;
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FuzzySystem fuzzy = FuzzyReader.GetFuzzySystemFromFile(FuzzySystemText.Text);
                fuzzy.LoadInputsAndSaveOutputs(InputValuesText.Text, "FuzzySystemTemporaryTestOutputFile.temp.del");
                float MSE = FuzzySystem.GetOutputFilesMSE(OutputValuesText.Text, "FuzzySystemTemporaryTestOutputFile.temp.del");
                if (File.Exists("FuzzySystemTemporaryTestOutputFile.temp.del"))
                    File.Delete("FuzzySystemTemporaryTestOutputFile.temp.del");

                MSEText.Text = MSE.ToString(CultureInfo.InvariantCulture);
                RMSEText.Text = Math.Sqrt(MSE).ToString(CultureInfo.InvariantCulture);
                RMSEPText.Text = (Math.Sqrt(MSE) / (fuzzy.Outputs[0].Range[1] - fuzzy.Outputs[0].Range[0]) * 100.0f).ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                if (File.Exists("FuzzySystemTemporaryTestOutputFile.temp.del"))
                    File.Delete("FuzzySystemTemporaryTestOutputFile.temp.del");
                return;
            }
        }
    }
}
