using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections;
using System.Collections.Generic;
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
        Default2D,
        Default3D,
        Default4D,
        Default5D
    }
    public enum Dimensions
    {
        Dimension_1,
        Dimension_2,
        Dimension_3,
        Dimension_4,
        Dimension_5
    }
    public partial class MainWindow : Window
    {
        private int PointCount2;
        private NewtonL newt;
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
        #region Dimension
        public static readonly DependencyProperty DimensionProperty
            = DependencyProperty.Register("Dimension", typeof(Dimensions), typeof(MainWindow)
                , new FrameworkPropertyMetadata(Dimensions.Dimension_2
                    , FrameworkPropertyMetadataOptions.AffectsMeasure
                      | FrameworkPropertyMetadataOptions.AffectsRender)
                , validateDimension);
        public Dimensions Dimension
        {
            get { return (Dimensions)GetValue(DimensionProperty); }
            set { SetValue(DimensionProperty, value); }
        }
        private static bool validateDimension(object value)
        {
            Dimensions dimension = (Dimensions)value;
            foreach (Dimensions item in Enum.GetValues(typeof(Dimensions)))
            {
                if (item == dimension)
                    return true;
            }
            return false;
        }

        #endregion
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
                , new FrameworkPropertyMetadata(CurveNames.Default2D
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
            Modify(Dimension);
            switch (Dimension)
            {
                case Dimensions.Dimension_1:
                    break;
                case Dimensions.Dimension_2:
                    switch (Curve)
                    {
                        case CurveNames.Default2D:
                            DrawCurve(Default2D);
                            break;
                    }
                    break;
                case Dimensions.Dimension_3:
                    switch (Curve)
                    {
                        case CurveNames.Default3D:
                            DrawCurve(Default3D);
                            break;
                    }
                    break;
                case Dimensions.Dimension_4:
                    switch (Curve)
                    {
                        case CurveNames.Default4D:
                            DrawCurve(Default4D);
                            break;
                    }
                    break;
                case Dimensions.Dimension_5:
                    switch (Curve)
                    {
                        case CurveNames.Default5D:
                            DrawCurve(Default5D);
                            break;
                    }
                    break;
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
            switch (Dimension)
            {
                case Dimensions.Dimension_1:
                    break;
                case Dimensions.Dimension_2:
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
                    break;
                case Dimensions.Dimension_3:
                    double min = Math.Abs(((NewtonPoint)Newtonpoints[0])[0]);
                    foreach (NewtonPoint item in Newtonpoints)
                    {
                        byte red = (byte)Math.Round(item.Points[2] / PointCount2 * 255);
                        Rectangle rect = new Rectangle()
                        {
                            Stroke = new SolidColorBrush(Color.FromArgb(255, red, (byte)(255 - red), 0)),
                            Fill = new SolidColorBrush(Color.FromArgb(255, red, (byte)(255 - red), 0)),
                            Height = markerSize,
                            Width = markerSize
                        };
                        Canvas.SetLeft(rect, (item.Points[0] - markerSize / 2) + min);
                        Canvas.SetTop(rect, (item.Points[1] - markerSize / 2) + min);
                        canvas.Children.Add(rect);
                    }
                    break;
                case Dimensions.Dimension_4:
                    break;
                case Dimensions.Dimension_5:
                    break;
            }
        }
        #region Curves
        ArrayList Default2D()
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
            PointCount2 = int.Parse((Math.Sqrt(PointCount)).ToString());
            double step = ScaleX / (Math.Sqrt(PointCount));
            for (int i = 0; i < PointCount2; ++i)
            {
                double x = -1 + i * step;
                for (int j = 0; j < PointCount2; j++)
                {
                    double y = -1 + j * step;
                    points.Add(new NewtonPoint(new double[] { ScaleX * x, ScaleX * y, ScaleY * (1 + 25 * x * y * x * y) }));
                }
            }
            return points;
        }
        ArrayList Default4D()
        {
            ArrayList points = new ArrayList();
            PointCount2 = int.Parse((Math.Sqrt(PointCount)).ToString());
            double step = ScaleX / (Math.Sqrt(PointCount));
            for (int i = 0; i < PointCount2; ++i)
            {
                double x = -1 + i * step;
                for (int j = 0; j < PointCount2; j++)
                {
                    double y = -1 + j * step;
                    points.Add(new NewtonPoint(new double[] { ScaleX * x, ScaleX * y, x + y }));
                }
            }
            return points;
        }
        ArrayList Default5D()
        {
            ArrayList points = new ArrayList();
            PointCount2 = int.Parse((Math.Sqrt(PointCount)).ToString());
            double step = ScaleX / (Math.Sqrt(PointCount));
            for (int i = 0; i < PointCount2; ++i)
            {
                double x = -1 + i * step;
                for (int j = 0; j < PointCount2; j++)
                {
                    double y = -1 + j * step;
                    points.Add(new NewtonPoint(new double[] { ScaleX * x, ScaleX * y, x + y }));
                }
            }
            return points;
        }
        #endregion Curves

        private void Modify(Dimensions Enum)
        {
            switch (Enum)
            {
                case Dimensions.Dimension_1:
                    break;
                case Dimensions.Dimension_2:
                    sldrPointCount.Minimum = 2;
                    sldrPointCount.Maximum = 100;
                    sldrPointCount.IsSnapToTickEnabled = false;
                    txtYScale.Visibility = Visibility.Visible;
                    YSizeLb.Visibility = Visibility.Visible;
                    XSizeLb.Content = "X tartomány";
                    firstKoorlb.Content = "";
                    lastKoorlb.Content = "";
                    break;
                case Dimensions.Dimension_3:
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
                    sldrPointCount.Maximum = 900;
                    sldrPointCount.Ticks = new DoubleCollection() { 9.0, 16.0, 25.0, 36.0, 49.0, 64.0, 81.0, 100.0, 121.0, 144.0, 169.0, 196.0, 225.0, 256.0, 289.0, 324.0, 361.0, 400.0, 441.0, 484.0, 529.0, 576, 625.0, 676.0, 729.0, 784.0, 841.0, 900.0  };
                    txtYScale.Visibility = Visibility.Hidden;
                    YSizeLb.Visibility = Visibility.Hidden;
                    XSizeLb.Content = "X, Y tartomány";
                    firstKoorlb.Content = "";
                    lastKoorlb.Content = "";
                    break;
                case Dimensions.Dimension_4:
                    break;
                case Dimensions.Dimension_5:
                    break;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            NewtonPoint startPoint;
            NewtonPoint pointFinal;
            Ellipse marker;
            Ellipse marker1;
            switch (Dimension)
            {
                case Dimensions.Dimension_1:
                    break;
                case Dimensions.Dimension_2:
                    startPoint = new NewtonPoint(new double[] { ((NewtonPoint)Newtonpoints[0])[0], ((NewtonPoint)Newtonpoints[0])[1] });
                    marker = new Ellipse()
                    {
                        Stroke = Brushes.DarkBlue,
                        Fill = Brushes.DarkBlue,
                        Height = markerSize * 2,
                        Width = markerSize * 2
                    };
                    Canvas.SetLeft(marker, startPoint.Points[0] - markerSize);
                    Canvas.SetTop(marker, ScaleY - startPoint.Points[1] - markerSize);
                    canvas.Children.Add(marker);
                    newt = new NewtonL(Newtonpoints, startPoint, 2);
                    pointFinal = newt.MinSearch();
                    marker1 = new Ellipse()
                    {
                        Stroke = Brushes.Green,
                        Fill = Brushes.Green,
                        Height = markerSize * 2,
                        Width = markerSize * 2
                    };
                    Canvas.SetLeft(marker1, pointFinal[0] - markerSize);
                    Canvas.SetTop(marker1, ScaleY - pointFinal[1] - markerSize);
                    canvas.Children.Add(marker1);
                    firstKoorlb.Content = "Kezdet: |" + Math.Round(startPoint[0], 4) + " | " + Math.Round(startPoint[1], 4) + " |";
                    lastKoorlb.Content = "Vége: |" + Math.Round(pointFinal[0], 4) + " | " + Math.Round(pointFinal[1], 4) + " |";
                    break;
                case Dimensions.Dimension_3:
                    double min = Math.Abs(((NewtonPoint)Newtonpoints[0])[0]);
                    startPoint = new NewtonPoint(new double[] { ((NewtonPoint)Newtonpoints[(PointCount2 * PointCount2) - 1])[0], ((NewtonPoint)Newtonpoints[(PointCount2 * PointCount2) - 1])[1], ((NewtonPoint)Newtonpoints[(PointCount2 * PointCount2) - 1])[2] });
                    marker = new Ellipse()
                    {
                        Stroke = Brushes.DarkBlue,
                        Fill = Brushes.DarkBlue,
                        Height = markerSize * 2,
                        Width = markerSize * 2
                    };
                    Canvas.SetLeft(marker, (startPoint.Points[0] - markerSize) + min);
                    Canvas.SetTop(marker, (startPoint.Points[1] - markerSize) + min);
                    canvas.Children.Add(marker);
                    newt = new NewtonL(Newtonpoints, startPoint, 3);
                    pointFinal = newt.MinSearch();
                    marker1 = new Ellipse()
                    {
                        Stroke = Brushes.Green,
                        Fill = Brushes.Green,
                        Height = markerSize * 2,
                        Width = markerSize * 2
                    };
                    Canvas.SetLeft(marker1, (pointFinal[0] - markerSize) + min);
                    Canvas.SetTop(marker1, (pointFinal[1] - markerSize) + min);
                    canvas.Children.Add(marker1);
                    firstKoorlb.Content = "Kezdete: | " + Math.Round(startPoint.Points[0], 4) + " | " + Math.Round(startPoint[1], 4) + " | " + Math.Round(startPoint[2], 4) + " |";
                    lastKoorlb.Content = "Vége: | " + Math.Round(pointFinal.Points[0], 4) + " | " + Math.Round(pointFinal[1], 4) + " | " + Math.Round(pointFinal[2], 4) + " |";
                    break;
                case Dimensions.Dimension_4:
                    break;
                case Dimensions.Dimension_5:
                    break;
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
