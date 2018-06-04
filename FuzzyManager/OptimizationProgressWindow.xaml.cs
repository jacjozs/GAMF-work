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
using System.Windows.Shapes;

namespace FuzzyManager
{
    /// <summary>
    /// Interaction logic for OptimizationProgressWindow.xaml
    /// </summary>
    public partial class OptimizationProgressWindow : Window
    {
        public OptimizerWindow OptWindow;

        public OptimizationProgressWindow()
        {
            InitializeComponent();
            Affinities = new List<double>();
            Max = -1.0;
        }

        public void UpdateText(string text)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressText.Text = text;
                /*ProgressText.Text = affinities[0].ToString();
                for (int i = 1; i < affinities.Length; i++)
                    ProgressText.Text += "\r\n" + affinities[i].ToString();*/
            });
        }

        private List<double> Affinities;
        private double Max;
        public void UpdateImage(double newAffinity)
        {
            Affinities.Add(newAffinity);

            if (Max == -1.0)
                Max = newAffinity;

            Dispatcher.Invoke(() =>
            {
                // Initialize variables
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(300, 200);
                System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bmp);

                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                //graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;  // This messes up the lines, maybe improves text, though?

                // Draw the bars
                int drawOffset = 0;
                for (int i = Math.Max(0, Affinities.Count - 100); i < Affinities.Count; i++)
                {
                    graphics.FillRectangle(
                        System.Drawing.Brushes.DarkGreen,
                        drawOffset * 3,
                        (float)(bmp.Height - bmp.Height * (Affinities[i] / Max)),
                        3.0f,
                        (float)(bmp.Height * (Affinities[i] / Max))
                    );
                    drawOffset++;
                }
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();

                ProgressImg.Source = bi;
                NewAffinityTextBox.Text = "Current affinity: " + newAffinity;
            });
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            OptWindow.StopOptimizing();
            StopButton.Content = "Stopping...";
            StopButton.IsEnabled = false;
        }
    }
}
