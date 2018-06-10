using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections;
using System.Globalization;
using Optimization;

namespace NewtonLTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Felhasznált project 
    /// https://www.codeproject.com/articles/33776/draw-closed-smooth-curve-with-bezier-spline
    /// </summary>
    public enum CurveNames
    {
        Default,
        Default3D,
    }
    public partial class MainWindow : Window
    {
        /// <summary>
        /// False = 2D, True = 3D
        /// </summary>
        private bool Dimension;
        private int PointCount2;
        private const double markerSize = 5;
        #region PointCount
        /// <summary>
        /// Identifies the PointCount dependency property.
        /// </summary>
        public static readonly DependencyProperty PointCountProperty
            = DependencyProperty.Register("PointCount", typeof(int), typeof(MainWindow)
                , new FrameworkPropertyMetadata(15
                    , FrameworkPropertyMetadataOptions.AffectsMeasure
                        | FrameworkPropertyMetadataOptions.AffectsRender)
                , validatePointCount);
        /// <summary>
        /// Gets or sets the PointCount property.
        /// </summary>
        /// <value>integer > 1</value>
        public int PointCount
        {
            get { return (int)GetValue(PointCountProperty); }
            set { SetValue(PointCountProperty, value); }
        }
        /// <summary>
        /// Validates the proposed PointCount property value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static bool validatePointCount(object value)
        {
            int cnt = (int)value;
            return (cnt > 1 ? true : false);
        }
        #endregion PointCount
        #region ScaleX
        /// <summary>
        /// Identifies the ScaleX dependency property.
        /// </summary>
        public static readonly DependencyProperty ScaleXProperty
            = DependencyProperty.Register("ScaleX", typeof(double), typeof(MainWindow)
                , new FrameworkPropertyMetadata(10.0
                    , FrameworkPropertyMetadataOptions.AffectsMeasure
                        | FrameworkPropertyMetadataOptions.AffectsRender)
                , validateScaleX);
        /// <summary>
        /// Gets or sets the ScaleX property.
        /// </summary>
        /// <value>double >= 1</value>
        public double ScaleX
        {
            get { return (double)GetValue(ScaleXProperty); }
            set { SetValue(ScaleXProperty, value); }
        }
        /// <summary>
        /// Validates the proposed ScaleX property value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static bool validateScaleX(object value)
        {
            double scale = (double)value;
            return (scale >= 1.0 ? true : false);
        }
        #endregion ScaleX
        #region ScaleY
        /// <summary>
        /// Identifies the ScaleY dependency property.
        /// </summary>
        public static readonly DependencyProperty ScaleYProperty
            = DependencyProperty.Register("ScaleY", typeof(double), typeof(MainWindow)
                , new FrameworkPropertyMetadata(10.0
                    , FrameworkPropertyMetadataOptions.AffectsMeasure
                        | FrameworkPropertyMetadataOptions.AffectsRender)
                , validateScaleY);
        /// <summary>
        /// Gets or sets the ScaleY property.
        /// </summary>
        /// <value>double >= 1</value>
        public double ScaleY
        {
            get { return (double)GetValue(ScaleYProperty); }
            set { SetValue(ScaleYProperty, value); }
        }
        /// <summary>
        /// Validates the proposed ScaleY property value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static bool validateScaleY(object value)
        {
            double scale = (double)value;
            return (scale >= 1.0 ? true : false);
        }
        #endregion ScaleY
        #region Curve
        /// <summary>
        /// Identifies the Curve dependency property.
        /// </summary>
        public static readonly DependencyProperty CurveProperty
            = DependencyProperty.Register("Curve", typeof(CurveNames), typeof(MainWindow)
                , new FrameworkPropertyMetadata(CurveNames.Default
                    , FrameworkPropertyMetadataOptions.AffectsMeasure
                        | FrameworkPropertyMetadataOptions.AffectsRender)
                , validateCurve);
        /// <summary>
        /// Gets or sets the Curve property.
        /// </summary>
        /// <value>CurveNames value</value>
        public CurveNames Curve
        {
            get { return (CurveNames)GetValue(CurveProperty); }
            set { SetValue(CurveProperty, value); }
        }
        /// <summary>
        /// Validates the proposed Curve property value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static bool validateCurve(object value)
        {
            CurveNames cName = (CurveNames)value;
            foreach (CurveNames item in Enum.GetValues(typeof(CurveNames)))
            {
                if (item == cName)
                    return true;
            }
            return false;
        }
        #endregion Curve
        /// <summary>
        /// A pontok listája
        /// (eböl készül el a SortItems valamint a vizuális megjelenítés)
        /// </summary>
        public ArrayList Newtonpoints;
        public Point[] points;
        public MainWindow()
        {
            InitializeComponent();
        }
        /// <summary>
		/// When overridden in a derived class, participates in rendering operations that are directed by the layout system. The rendering instructions for this element are not used directly when this method is invoked, and are instead preserved for later asynchronous use by layout and drawing.
		/// </summary>
		/// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Dimension && PointCount != 0)
            {
                DrawCurve(Default3D);
            }
            else
            {
                DrawCurve(Default);
            }
            base.OnRender(drawingContext);
        }
        /// <summary>
		/// Function ArrayList provider
		/// </summary>
        delegate ArrayList Function();
        /// <summary>
		/// Draw the Curve.
		/// </summary>
		/// <param name="curve">The curve to draw.</param>
        void DrawCurve(Function curve)
        {
            canvas.Children.Clear();
            lastKoorlb.Content = "";
            Newtonpoints = curve();
            if (Newtonpoints.Count < 2)
                return;
            if (!Dimension)
            {
                foreach (NewtonPoint item in Newtonpoints)
                {
                    Rectangle rect = new Rectangle()
                    {
                        Stroke = Brushes.Black,
                        Fill = Brushes.Black,
                        Height = markerSize,
                        Width = markerSize
                    };
                    Canvas.SetLeft(rect, item.Points[0] - markerSize / 2);
                    Canvas.SetTop(rect, ScaleY - item.Points[1] - markerSize / 2);
                    canvas.Children.Add(rect);
                }
                Point[] cp1, cp2;
                points = new Point[Newtonpoints.Count];
                for (int i = 0; i < points.Length; i++)
                {
                    points[i] = new Point(((NewtonPoint)Newtonpoints[i]).Points[0], ScaleY - ((NewtonPoint)Newtonpoints[i]).Points[1]);
                }
                BezierSpline.GetCurveControlPoints(points, out cp1, out cp2);
                PathSegmentCollection lines = new PathSegmentCollection();
                for (int i = 0; i < cp1.Length; ++i)
                {
                    lines.Add(new BezierSegment(cp1[i], cp2[i], points[i + 1], true));
                }
                PathFigure f = new PathFigure(points[0], lines, false);
                PathGeometry g = new PathGeometry(new PathFigure[] { f });
                Path path = new Path() { Stroke = Brushes.Red, StrokeThickness = 1, Data = g };
                canvas.Children.Add(path);
            }
            else
            {
                foreach (NewtonPoint item in Newtonpoints)
                {
                    byte red = (byte)Math.Round((item.Points[2] - 0) / PointCount2 * 255);
                    Rectangle rect = new Rectangle()
                    {
                        Stroke = new SolidColorBrush(Color.FromArgb(255, red, (byte)(255 - red), 0)),
                        Fill = new SolidColorBrush(Color.FromArgb(255, red, (byte)(255 - red), 0)),
                        Height = markerSize,
                        Width = markerSize
                    };
                    Canvas.SetLeft(rect, item.Points[0] - markerSize / 2);
                    Canvas.SetTop(rect, item.Points[1] - markerSize / 2);
                    canvas.Children.Add(rect);
                }
            }
        }
        #region Curves
        ArrayList Default()
        {
            ArrayList points = new ArrayList();
            double step = 2.0 / (PointCount - 1);
            for (int i = 0; i < PointCount; ++i)
            {
                double x = -1 + i * step;
                points.Add(new NewtonPoint(new double[] { ScaleX * (x + 1), ScaleY * (1 - 1 / (1 + 25 * x * x)) }));
            }
            return points;
        }
        ArrayList Default3D()
        {
            ArrayList points = new ArrayList();
            PointCount2 = int.Parse(Math.Sqrt(PointCount).ToString());
            double step = ScaleX / (Math.Sqrt(PointCount));
            for (int i = 0; i < PointCount2; ++i)
            {
                double x = -1 + i * step;
                for (int j = 0; j < PointCount2; j++)
                {
                    double y = -1 + j * step;
                    points.Add(new NewtonPoint(new double[] { ScaleX * (x + 1), ScaleX * (y + 1), Math.Sqrt(j * 10) }));
                }
            }
            return points;
        }
        #endregion Curves

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Uid == "2")
            {
                Dimension = true;
                SwitchBt.Uid = "3";
                SwitchBt.Content = "3D";
                sldrPointCount.IsSnapToTickEnabled = true;
                if (sldrPointCount.Value < 9)
                    sldrPointCount.Value = 9;
                else
                    if (sldrPointCount.Value < 16)
                    sldrPointCount.Value = 16;
                else
                    if (sldrPointCount.Value < 25)
                    sldrPointCount.Value = 25;
                else
                    if (sldrPointCount.Value < 36)
                    sldrPointCount.Value = 36;
                else
                    if (sldrPointCount.Value < 49)
                    sldrPointCount.Value = 49;
                else
                    if (sldrPointCount.Value < 64)
                    sldrPointCount.Value = 64;
                else
                    if (sldrPointCount.Value < 81)
                    sldrPointCount.Value = 81;
                else
                    if (sldrPointCount.Value < 100)
                    sldrPointCount.Value = 100;
                sldrPointCount.Minimum = 9;
                sldrPointCount.Maximum = 400;
                sldrPointCount.Ticks = new DoubleCollection() { 9.0, 16.0, 25.0, 36.0, 49.0, 64.0, 81.0, 100.0, 121.0, 144.0, 169.0, 196.0, 225.0, 256.0, 289.0, 324.0, 361.0, 400.0 };
                txtYScale.Visibility = Visibility.Hidden;
                YSizeLb.Visibility = Visibility.Hidden;
                XSizeLb.Content = "X, Y tartomány";
            }
            else
            {
                Dimension = false;
                SwitchBt.Uid = "2";
                SwitchBt.Content = "2D";
                sldrPointCount.Minimum = 2;
                sldrPointCount.Maximum = 100;
                sldrPointCount.IsSnapToTickEnabled = false;
                txtYScale.Visibility = Visibility.Visible;
                YSizeLb.Visibility = Visibility.Visible;
                XSizeLb.Content = "X tartomány";
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            NewtonL newt;
            if (Dimension)
            {
                newt = new NewtonL(Newtonpoints, new NewtonPoint(new double[] { ((NewtonPoint)Newtonpoints[PointCount - 1]).Points[0], ((NewtonPoint)Newtonpoints[PointCount - 1]).Points[1], ((NewtonPoint)Newtonpoints[PointCount - 1]).Points[2] }), 3);
                NewtonPoint pointFinal = newt.MinSearch();
                Ellipse marker = new Ellipse()
                {
                    Stroke = Brushes.Green,
                    Fill = Brushes.Green,
                    Height = markerSize * 2,
                    Width = markerSize * 2
                };
                Canvas.SetLeft(marker, pointFinal.Points[0] - markerSize);
                Canvas.SetTop(marker, pointFinal.Points[1] - markerSize);
                canvas.Children.Add(marker);
                lastKoorlb.Content = Math.Round(pointFinal.Points[0], 4) + " - " + Math.Round(pointFinal.Points[1], 4) + " - " + Math.Round(pointFinal.Points[2], 4);
            }
            else
            {
                newt = new NewtonL(Newtonpoints, new NewtonPoint(new double[] { ((NewtonPoint)Newtonpoints[0]).Points[0], ((NewtonPoint)Newtonpoints[0]).Points[1] }), 2);
                NewtonPoint pointFinal = newt.MinSearch();
                Ellipse marker = new Ellipse()
                {
                    Stroke = Brushes.Green,
                    Fill = Brushes.Green,
                    Height = markerSize * 2,
                    Width = markerSize * 2
                };
                Canvas.SetLeft(marker, pointFinal.Points[0] - markerSize);
                Canvas.SetTop(marker, ScaleY - pointFinal.Points[1] - markerSize);
                canvas.Children.Add(marker);
                lastKoorlb.Content = Math.Round(pointFinal.Points[0], 4) + " - " + Math.Round(pointFinal.Points[1], 4);
            }
        }
    }
    /// <summary>
	/// ValidationRule class to validate that a value is a number >= 1.
	/// </summary>
    public class ScaleRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string stringValue = value as string;
            if (!string.IsNullOrEmpty(stringValue))
            {
                double doubleValue;
                if (double.TryParse(stringValue, out doubleValue))
                {
                    if (doubleValue >= 1)
                        return new ValidationResult(true, null);
                }
            }
            return new ValidationResult(false, "Must be a number greater or equal to 1");
        }
    }
}
