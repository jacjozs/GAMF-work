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
        Dimension_2,
        Dimension_3,
        Dimension_4,
        Dimension_5
    }
    public partial class MainWindow : Window
    {
        private int PointCount2;
        private NewtonL newt;
        private bool History = false;
        private bool IsSelected = false;
        private NewtonPoint startPoint;
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
                , new FrameworkPropertyMetadata(400.0
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
                , new FrameworkPropertyMetadata(400.0
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
        #region ScaleZ
        /// <summary>
        /// Identifies the ScaleZ dependency property.
        /// </summary>
        public static readonly DependencyProperty ScaleZProperty
            = DependencyProperty.Register("ScaleZ", typeof(double), typeof(MainWindow)
                , new FrameworkPropertyMetadata(10.0
                    , FrameworkPropertyMetadataOptions.AffectsMeasure
                      | FrameworkPropertyMetadataOptions.AffectsRender)
                , validateScaleZ);
        /// <summary>
        /// Gets or sets the ScaleZ property.
        /// </summary>
        /// <value>double >= 1</value>
        public double ScaleZ
        {
            get { return (double)GetValue(ScaleZProperty); }
            set { SetValue(ScaleZProperty, value); }
        }
        /// <summary>
        /// Validates the proposed ScaleZ property value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static bool validateScaleZ(object value)
        {
            double scale = (double)value;
            return (scale >= 1.0 ? true : false);
        }
        #endregion ScaleZ
        #region ScaleW
        /// <summary>
        /// Identifies the ScaleW dependency property.
        /// </summary>
        public static readonly DependencyProperty ScaleWProperty
            = DependencyProperty.Register("ScaleW", typeof(double), typeof(MainWindow)
                , new FrameworkPropertyMetadata(10.0
                    , FrameworkPropertyMetadataOptions.AffectsMeasure
                      | FrameworkPropertyMetadataOptions.AffectsRender)
                , validateScaleW);
        /// <summary>
        /// Gets or sets the ScaleW property.
        /// </summary>
        /// <value>double >= 1</value>
        public double ScaleW
        {
            get { return (double)GetValue(ScaleWProperty); }
            set { SetValue(ScaleWProperty, value); }
        }
        /// <summary>
        /// Validates the proposed ScaleW property value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static bool validateScaleW(object value)
        {
            double scale = (double)value;
            return (scale >= 1.0 ? true : false);
        }
        #endregion ScaleW
        #region ScaleN
        /// <summary>
        /// Identifies the ScaleN dependency property.
        /// </summary>
        public static readonly DependencyProperty ScaleNProperty
            = DependencyProperty.Register("ScaleN", typeof(double), typeof(MainWindow)
                , new FrameworkPropertyMetadata(10.0
                    , FrameworkPropertyMetadataOptions.AffectsMeasure
                      | FrameworkPropertyMetadataOptions.AffectsRender)
                , validateScaleN);
        /// <summary>
        /// Gets or sets the ScaleN property.
        /// </summary>
        /// <value>double >= 1</value>
        public double ScaleN
        {
            get { return (double)GetValue(ScaleNProperty); }
            set { SetValue(ScaleNProperty, value); }
        }
        /// <summary>
        /// Validates the proposed ScaleN property value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static bool validateScaleN(object value)
        {
            double scale = (double)value;
            return (scale >= 1.0 ? true : false);
        }
        #endregion ScaleW
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
            newt = new NewtonL();
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
                case Dimensions.Dimension_2:
                    Curve = CurveNames.Default2D;
                    switch (Curve)
                    {
                        case CurveNames.Default2D:
                            DrawCurve(Default2D);
                            break;
                    }
                    break;
                case Dimensions.Dimension_3:
                    Curve = CurveNames.Default3D;
                    switch (Curve)
                    {
                        case CurveNames.Default3D:
                            DrawCurve(Default3D);
                            break;
                    }
                    break;
                case Dimensions.Dimension_4:
                    Curve = CurveNames.Default4D;
                    switch (Curve)
                    {
                        case CurveNames.Default4D:
                            DrawCurve(Default4D);
                            break;
                    }
                    break;
                case Dimensions.Dimension_5:
                    Curve = CurveNames.Default5D;
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
            Ellipse marker;
            ListUpdate();
            IsSelected = false;
            switch (Dimension)
            {
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
                    startPoint = new NewtonPoint(new double[] { ((NewtonPoint)Newtonpoints[0])[0], ((NewtonPoint)Newtonpoints[0])[1] });
                    marker = new Ellipse()
                    {
                        Stroke = Brushes.DarkBlue,
                        Fill = Brushes.DarkBlue,
                        Height = markerSize * 2,
                        Width = markerSize * 2
                    };
                    Canvas.SetLeft(marker, startPoint[0] - markerSize);
                    Canvas.SetTop(marker, ScaleY - startPoint[1] - markerSize);
                    canvas.Children.Add(marker);
                    firstKoorlb.Content = "Kezdet: |" + Math.Round(startPoint[0], 4) + " | " + Math.Round(startPoint[1], 4) + " |";
                    break;
                case Dimensions.Dimension_3:
                    double min = Math.Abs(((NewtonPoint)Newtonpoints[0])[0]);
                    foreach (NewtonPoint item in Newtonpoints)
                    {
                        byte red = (byte)Math.Round(item[2] / ScaleZ * 255);
                        Rectangle rect = new Rectangle()
                        {
                            Stroke = new SolidColorBrush(Color.FromArgb(255, red, (byte)(255 - red), 0)),
                            Fill = new SolidColorBrush(Color.FromArgb(255, red, (byte)(255 - red), 0)),
                            Height = markerSize,
                            Width = markerSize
                        };
                        Canvas.SetLeft(rect, (item[0] - markerSize / 2) + min + 1);
                        Canvas.SetTop(rect, (item[1] - markerSize / 2) + min + 1);
                        canvas.Children.Add(rect);
                    }
                    startPoint = new NewtonPoint(new double[] { ((NewtonPoint)Newtonpoints[(PointCount2 * 2) - 1])[0], ((NewtonPoint)Newtonpoints[(PointCount2 * 2) - 1])[1], ((NewtonPoint)Newtonpoints[(PointCount2 * 2) - 1])[2] });
                    marker = new Ellipse()
                    {
                        Stroke = Brushes.DarkBlue,
                        Fill = Brushes.DarkBlue,
                        Height = markerSize * 2,
                        Width = markerSize * 2
                    };
                    Canvas.SetLeft(marker, (startPoint[0] - markerSize) + min + 1);
                    Canvas.SetTop(marker, (startPoint[1] - markerSize) + min + 1);
                    canvas.Children.Add(marker);
                    firstKoorlb.Content = "Kezdete: | " + Math.Round(startPoint.Points[0], 4) + " | " + Math.Round(startPoint[1], 4) + " | " + Math.Round(startPoint[2], 4) + " |";
                    break;
                case Dimensions.Dimension_4:
                    startPoint = new NewtonPoint(new double[] { ((NewtonPoint)Newtonpoints[(PointCount2 * 3) - 1])[0], ((NewtonPoint)Newtonpoints[(PointCount2 * 3) - 1])[1], ((NewtonPoint)Newtonpoints[(PointCount2 * 3) - 1])[2], ((NewtonPoint)Newtonpoints[(PointCount2 * 3) - 1])[3] });
                    firstKoorlb.Content = "Kezdete: | " + Math.Round(startPoint.Points[0], 4) + " | " +
                      Math.Round(startPoint[1], 4) + " | " + Math.Round(startPoint[2], 4) + " | " +
                      Math.Round(startPoint[3], 4) + " |";
                    break;
                case Dimensions.Dimension_5:
                    startPoint = new NewtonPoint(new double[] { ((NewtonPoint)Newtonpoints[(PointCount2 * 4) - 1])[0], ((NewtonPoint)Newtonpoints[(PointCount2 * 4) - 1])[1], ((NewtonPoint)Newtonpoints[(PointCount2 * 4) - 1])[2], ((NewtonPoint)Newtonpoints[(PointCount2 * 4) - 1])[3], ((NewtonPoint)Newtonpoints[(PointCount2 * 4) - 1])[4] });
                    firstKoorlb.Content = "Kezdete: | " + Math.Round(startPoint.Points[0], 4) + " | " +
                      Math.Round(startPoint[1], 4) + " | " + Math.Round(startPoint[2], 4) + " | " +
                      Math.Round(startPoint[3], 4) + " | " + Math.Round(startPoint[4], 4) + " |";
                    break;
            }
        }
        #region Curves
        ArrayList Default2D()
        {
            ArrayList points = new ArrayList();
            double step = 2.0 / (PointCount - 1);
            return newt.PointCreate("((sqrt(x)*√(49))^0,5)+1-1", PointCount, new double[] { ScaleX }, step);
        }
        ArrayList Default3D()
        {
            PointCount2 = (int)Math.Sqrt(PointCount);
            double stepX = ScaleX / PointCount2;
            return newt.PointCreate("2*x+3*sqrt(y)", PointCount, new double[] { ScaleX, ScaleY }, stepX);
        }
        ArrayList Default4D()
        {
            PointCount2 = (int)Math.Sqrt(PointCount);
            double stepX = ScaleX / PointCount2;
            return newt.PointCreate("2*x+3*sqrt(y)*z", PointCount, new double[] { ScaleX, ScaleY, ScaleZ }, stepX);
        }
        ArrayList Default5D()
        {
            PointCount2 = (int)Math.Sqrt(PointCount);
            double stepX = ScaleX / PointCount2;
            return newt.PointCreate("2*x+3*sqrt(y)*z+w", PointCount, new double[] { ScaleX, ScaleY, ScaleZ, ScaleW }, stepX);
        }
        #endregion Curves

        private void Modify(Dimensions Enum)
        {
            switch (Enum)
            {
                case Dimensions.Dimension_2:
                    sldrPointCount.Minimum = 2;
                    sldrPointCount.Maximum = 100;
                    txtYScale.Visibility = Visibility.Visible;
                    YSizeLb.Visibility = Visibility.Visible;
                    txtZScale.Visibility = Visibility.Hidden;
                    ZSizeLb.Visibility = Visibility.Hidden;
                    txtWScale.Visibility = Visibility.Hidden;
                    WSizeLb.Visibility = Visibility.Hidden;
                    txtNScale.Visibility = Visibility.Hidden;
                    NSizeLb.Visibility = Visibility.Hidden;
                    firstKoorlb.Content = "";
                    lastKoorlb.Content = "";
                    break;
                case Dimensions.Dimension_3:
                    sldrPointCount.Maximum = 1000;
                    txtYScale.Visibility = Visibility.Visible;
                    YSizeLb.Visibility = Visibility.Visible;
                    txtZScale.Visibility = Visibility.Visible;
                    ZSizeLb.Visibility = Visibility.Visible;
                    txtWScale.Visibility = Visibility.Hidden;
                    WSizeLb.Visibility = Visibility.Hidden;
                    txtNScale.Visibility = Visibility.Hidden;
                    NSizeLb.Visibility = Visibility.Hidden;
                    firstKoorlb.Content = "";
                    lastKoorlb.Content = "";
                    break;
                case Dimensions.Dimension_4:
                    sldrPointCount.Maximum = 10000;
                    txtYScale.Visibility = Visibility.Visible;
                    YSizeLb.Visibility = Visibility.Visible;
                    txtZScale.Visibility = Visibility.Visible;
                    ZSizeLb.Visibility = Visibility.Visible;
                    txtWScale.Visibility = Visibility.Visible;
                    WSizeLb.Visibility = Visibility.Visible;
                    txtNScale.Visibility = Visibility.Hidden;
                    NSizeLb.Visibility = Visibility.Hidden;
                    firstKoorlb.Content = "";
                    lastKoorlb.Content = "";
                    break;
                case Dimensions.Dimension_5:
                    sldrPointCount.Maximum = 100000;
                    txtYScale.Visibility = Visibility.Visible;
                    YSizeLb.Visibility = Visibility.Visible;
                    txtZScale.Visibility = Visibility.Visible;
                    ZSizeLb.Visibility = Visibility.Visible;
                    txtWScale.Visibility = Visibility.Visible;
                    WSizeLb.Visibility = Visibility.Visible;
                    txtNScale.Visibility = Visibility.Visible;
                    NSizeLb.Visibility = Visibility.Visible;
                    firstKoorlb.Content = "";
                    lastKoorlb.Content = "";
                    break;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            NewtonPoint pointFinal;
            Ellipse marker1;
            switch (Dimension)
            {
                case Dimensions.Dimension_2:
                    newt.InitialParameters(Newtonpoints, startPoint, 2);
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
                    lastKoorlb.Content = "Vége: |" + Math.Round(pointFinal[0], 4) + " | " + Math.Round(pointFinal[1], 4) + " |";
                    break;
                case Dimensions.Dimension_3:
                    double min = Math.Abs(((NewtonPoint)Newtonpoints[0])[0]);
                    newt.InitialParameters(Newtonpoints, startPoint, 3);
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
                    lastKoorlb.Content = "Vége: | " + Math.Round(pointFinal.Points[0], 4) + " | " + Math.Round(pointFinal[1], 4) + " | " + Math.Round(pointFinal[2], 4) + " |";
                    break;
                case Dimensions.Dimension_4:
                    newt.InitialParameters(Newtonpoints, startPoint, 4);
                    pointFinal = newt.MinSearch();
                    lastKoorlb.Content = "Vége: | " + Math.Round(startPoint.Points[0], 4) + " | " +
                                         Math.Round(pointFinal[1], 4) + " | " + Math.Round(pointFinal[2], 4) + " | " +
                                         Math.Round(pointFinal[3], 4) + " |";
                    break;
                case Dimensions.Dimension_5:
                    newt.InitialParameters(Newtonpoints, startPoint, 5);
                    pointFinal = newt.MinSearch();
                    lastKoorlb.Content = "Vége: | " + Math.Round(pointFinal.Points[0], 4) + " | " +
                                         Math.Round(pointFinal[1], 4) + " | " + Math.Round(pointFinal[2], 4) + " | " +
                                         Math.Round(pointFinal[3], 4) + " | " + Math.Round(pointFinal[4], 4) + " |";
                    break;
            }
            ListUpdate();
        }
        private void ListUpdate()
        {
            if (!History)
            {
                lbList.Items.Clear();
                foreach (NewtonPoint item in Newtonpoints)
                {
                    Label lb = new Label();
                    lb.Content = item.ToString();
                    lbList.Items.Add(lb);
                }
            }
            else if (newt != null && newt.HistoryPositions != null)
            {
                lbList.Items.Clear();
                foreach (NewtonPoint item in newt.HistoryPositions)
                {
                    Label lb = new Label();
                    lb.Content = item.ToString();
                    lbList.Items.Add(lb);
                }
            }
        }

        private void checkBox_Click(object sender, RoutedEventArgs e)
        {
            var ertek = (CheckBox)sender;
            History = ertek.IsChecked.Value;
            ListUpdate();
        }

        private void lbList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Dimension == Dimensions.Dimension_2 || Dimension == Dimensions.Dimension_3)
            {
                string[] items = ((Label)((ListBox)sender).SelectedItem).Content.ToString().Split('|', ' ');
                double min = 0.0;
                double[] koordinatak = new double[2];
                if (Dimension == Dimensions.Dimension_3)
                {
                    min = Math.Abs(((NewtonPoint)Newtonpoints[0])[0]);
                }
                int i = 0;
                foreach (var item in items)
                {
                    if (item != "")
                    {
                        koordinatak[i] = double.Parse(item);
                        i++;
                    }
                    if (i == 2) break;
                }
                if (IsSelected)
                {
                    canvas.Children.RemoveAt(canvas.Children.Count - 1);
                }
                Ellipse marker = new Ellipse()
                {
                    Stroke = Brushes.DarkBlue,
                    Fill = Brushes.DarkBlue,
                    Height = markerSize * 2,
                    Width = markerSize * 2
                };
                if (Dimension == Dimensions.Dimension_2)
                {
                    Canvas.SetLeft(marker, koordinatak[0] - markerSize);
                    Canvas.SetTop(marker, ScaleY - koordinatak[1] - markerSize);
                }
                else
                {
                    Canvas.SetLeft(marker, (koordinatak[0] - markerSize) + min + 1);
                    Canvas.SetTop(marker, (koordinatak[1] - markerSize) + min + 1);
                }
                canvas.Children.Add(marker);
                IsSelected = true;
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
