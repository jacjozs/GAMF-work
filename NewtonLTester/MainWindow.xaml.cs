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
        Sinus,
        Default,
    }
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Az algoritmusnak átadandó pontok listája
        /// </summary>
        private ArrayList SortItems;
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
                , new FrameworkPropertyMetadata(500.0
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
                , new FrameworkPropertyMetadata(500.0
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
            switch (Curve)
            {
                case CurveNames.Sinus:
                    DrawCurve(Sinus);
                    break;
                case CurveNames.Default:
                    DrawCurve(Default);
                    break;
            }
            base.OnRender(drawingContext);
        }
        /// <summary>
		/// Function points provider
		/// </summary>
        delegate Point[] Function();
        /// <summary>
		/// Draw the Curve.
		/// </summary>
		/// <param name="curve">The curve to draw.</param>
        void DrawCurve(Function curve)
        {
            canvas.Children.Clear();
            points = curve();
            SortItems = new ArrayList();
            if (points.Length < 2)
                return;
            foreach (var item in points)
            {
                //azért kell a minus ScaleY hogy a kimutatásnál helyes pozicióba kerüljön
                SortItems.Add(new NewtonPoint(new double[] { item.X, ScaleY - item.Y }));
            }
            const double markerSize = 5;
            for (int i = 0; i < points.Length; ++i)
            {
                Rectangle rect = new Rectangle()
                {
                    Stroke = Brushes.Black,
                    Fill = Brushes.Black,
                    Height = markerSize,
                    Width = markerSize
                };
                Canvas.SetLeft(rect, points[i].X - markerSize / 2);
                Canvas.SetTop(rect, points[i].Y - markerSize / 2);
                canvas.Children.Add(rect);
            }
            Point[] cp1, cp2;
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

            NewtonL newt = new NewtonL(SortItems, new double[] { ((NewtonPoint)SortItems[0]).Points[0], ((NewtonPoint)SortItems[0]).Points[1] }, 2);
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
        }
        #region Curves

        /// <summary>
        /// Runge function points in [-1,1].
        /// </summary>
        /// <returns></returns>
        Point[] Default()
        {
            // Fill point array with scaled in X,Y Runge (1 / (1 + 25 * x * x)) values in [-1, 1].
            Point[] points = new Point[PointCount];
            double step = 2.0 / (PointCount - 1);
            for (int i = 0; i < PointCount; ++i)
            {
                double x = -1 + i * step;
                points[i] = new Point(ScaleX * (x + 1), ScaleY - (ScaleY * (1 - 1 / (1 + 25 * x * x))));
            }
            return points;
        }
        /// <summary>
		/// Sinus points in [0,2PI].
		/// </summary>
		/// <returns></returns>
        Point[] Sinus()
        {
            // Fill point array with scaled in X,Y Sin values in [0, PI].
            Point[] points = new Point[PointCount];
            double step = 2 * Math.PI / (PointCount - 1);
            for (int i = 0; i < PointCount; ++i)
            {
                points[i] = new Point(ScaleX * i * step, (ScaleY * (1 - Math.Sin(i * step))));
            }
            return points;
        }
        #endregion Curves
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
