using System;
using System.Collections.Generic;
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

namespace FuzzyManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void GeneratorButton_Click(object sender, RoutedEventArgs e)
        {
            GeneratorWindow generatorWindow = new GeneratorWindow();
            generatorWindow.Show();
        }

        private void OptimizerButton_Click(object sender, RoutedEventArgs e)
        {
            OptimizerWindow optimizerWindow = new OptimizerWindow();
            optimizerWindow.Show();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            TestWindow testWindow = new TestWindow();
            testWindow.Show();
        }
    }
}
