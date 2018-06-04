using Fuzzy;
using System;
using System.Collections.Generic;
using System.Drawing;
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
    /// Interaction logic for MFsPreviewWindow.xaml
    /// </summary>
    public partial class MFsPreviewWindow : Window
    {
        const int padding = 20;

        private InputOutput Ioput;

        public MFsPreviewWindow(InputOutput ioput)
        {
            InitializeComponent();

            Ioput = ioput;
            this.Title = "Membership functions of " + ioput.Name;

            PreviewImage.Source = GetPreviewBitmapImage(Convert.ToInt32(Width), Convert.ToInt32(Height)-50, Ioput);
        }

        public void ShowNewIoput(InputOutput ioput)
        {
            Ioput = ioput;
            Dispatcher.Invoke(() =>
            {
                this.Title = "Membership functions of " + ioput.Name;

                PreviewImage.Source = GetPreviewBitmapImage(Convert.ToInt32(Width), Convert.ToInt32(Height) - 50, Ioput);
            });
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            int x, y;

            try
            {
                x = int.Parse(HorizontalResBox.Text);
                y = int.Parse(VerticalResBox.Text);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "ERROR", MessageBoxButton.OK);
                return;
            }

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();

            saveFileDialog.DefaultExt = ".bmp";
            saveFileDialog.Filter = "Bitmap image (*.bmp)|*.bmp";
            saveFileDialog.FileName = Ioput.Name + ".bmp";
            saveFileDialog.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

            Nullable<bool> result = saveFileDialog.ShowDialog();

            if (!result.HasValue || !result.Value)
                return;

            GetPreviewBitmap(x, y, Ioput).Save(saveFileDialog.FileName);

            /*Rect rect = new Rect(PreviewImage.RenderSize);
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Right,
              (int)rect.Bottom, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(PreviewImage);
            //endcode as PNG
            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            //save to memory stream
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            pngEncoder.Save(ms);
            ms.Close();
            System.IO.File.WriteAllBytes("preview.png", ms.ToArray());*/
            //Console.WriteLine("Done");
        }

        protected float GetXcoordinate(float rangemin, float rangemax, int pad, int canvasX, float value)
        {
            //X = (param - rangemin) / (rangemax - rangemin) * (x - 2 * padding) + padding
            return (value - rangemin) / (rangemax - rangemin) * (canvasX - 2 * pad) + pad;
        }

        protected float GetYcoordinate(int pad, int canvasY, float value)
        {
            // Y = y - padding - value*(y - 2 * padding)
            return canvasY - pad - value*(canvasY - 2 * pad);
        }

        public System.Drawing.Bitmap GetPreviewBitmap(int x, int y, InputOutput ioput)
        {
            // Initialize variables
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(x, y);
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bmp);

            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            //graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;  // This messes up the lines, maybe improves text, though?

            // Set a white background
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            graphics.FillRectangle(System.Drawing.Brushes.White, 0, 0, x, y);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw the MFs
            foreach (MembershipFunction mf in ioput.MFs)
            {
                // If trapeze or triangle
                if (mf.Type == MFtype.trapmf || mf.Type == MFtype.trimf)
                {
                    // For each consequent param/value (ignore the last one)
                    for (int paramId = 0; paramId < mf.Params.Length-1; paramId++)
                    {
                        // Draw a line between them
                        graphics.DrawLine(
                            System.Drawing.Pens.Red,
                            GetXcoordinate(ioput.Range[0], ioput.Range[1], padding, x, mf.Params[paramId]),
                            GetYcoordinate(padding, y, mf.Values[paramId]),
                            GetXcoordinate(ioput.Range[0], ioput.Range[1], padding, x, mf.Params[paramId + 1]),
                            GetYcoordinate(padding, y, mf.Values[paramId + 1])
                        );
                    }
                }
                // If constant
                if (mf.Type == MFtype.constant)
                {
                    graphics.DrawLine(
                        Pens.Red,
                        GetXcoordinate(ioput.Range[0], ioput.Range[1], padding, x, mf.Params[0]),
                        GetYcoordinate(padding, y, 0),
                        GetXcoordinate(ioput.Range[0], ioput.Range[1], padding, x, mf.Params[0]),
                        GetYcoordinate(padding, y, mf.Values[0])
                    );
                }
            }

            // Cover the padding
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            graphics.FillRectangle(System.Drawing.Brushes.White, 0, 0, x, padding);
            graphics.FillRectangle(System.Drawing.Brushes.White, 0, y - padding, x, padding);
            graphics.FillRectangle(System.Drawing.Brushes.White, 0, 0, padding, y);
            graphics.FillRectangle(System.Drawing.Brushes.White, x - padding, 0, padding, y);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw the axes
            int zeroX = (int)Math.Round(GetXcoordinate(ioput.Range[0], ioput.Range[1], padding, x, 0.0f));
            if (zeroX < padding || zeroX > x - padding)
                zeroX = padding;
            graphics.DrawLine(System.Drawing.Pens.Black, zeroX, 0, zeroX, y - padding + 5); //y
            graphics.DrawLine(System.Drawing.Pens.Black, padding - 5, y - padding, x, y - padding); //x

            // Draw the arrowheads
            graphics.DrawLine(System.Drawing.Pens.Black, zeroX, 0, zeroX - 5, 10);  //y
            graphics.DrawLine(System.Drawing.Pens.Black, zeroX, 0, zeroX + 5, 10);  //y
            graphics.DrawLine(System.Drawing.Pens.Black, x, y - padding, x - 10, y - padding - 5);  //x
            graphics.DrawLine(System.Drawing.Pens.Black, x, y - padding, x - 10, y - padding + 5);  //x

            // Draw dashes
            graphics.DrawLine(System.Drawing.Pens.Black, padding, y - padding - 5, padding, y - padding + 5);
            graphics.DrawLine(System.Drawing.Pens.Black, x - padding, y - padding - 5, x - padding, y - padding + 5);
            graphics.DrawLine(System.Drawing.Pens.Black, zeroX - 5, padding, zeroX + 5, padding);

            //Add text
            int fontSize = 16;
            Font font = new Font(new System.Drawing.FontFamily("Arial"), fontSize, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel);
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Far;
            stringFormat.LineAlignment = StringAlignment.Near;
            graphics.DrawString("μ", font, System.Drawing.Brushes.Black, (float)(zeroX - 20), 0.0f);
            graphics.DrawString("1", font, System.Drawing.Brushes.Black, (float)(zeroX - 20), (float)padding);
            graphics.DrawString("0", font, System.Drawing.Brushes.Black, (float)(zeroX - 20), (float)(y - padding - fontSize - 2));
            graphics.DrawString(Ioput.Range[0].ToString(), font, System.Drawing.Brushes.Black, padding, (float)(y - fontSize));
            graphics.DrawString(Ioput.Range[1].ToString(), font, System.Drawing.Brushes.Black, new RectangleF(0, y - fontSize, x-padding, fontSize), stringFormat);

            return bmp;
        }

        public BitmapImage GetPreviewBitmapImage(int x, int y, InputOutput ioput)
        {
            System.Drawing.Bitmap bmp = GetPreviewBitmap(x, y, ioput);

            // Return as a BitmapImage
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            return bi;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ActualHeight > 50.0)
                PreviewImage.Source = GetPreviewBitmapImage(Convert.ToInt32(ActualWidth), Convert.ToInt32(ActualHeight) - 50, Ioput);
        }
    }
}
