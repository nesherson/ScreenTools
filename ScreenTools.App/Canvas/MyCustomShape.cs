using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Platform;

public class MyCustomShape : Shape
{
    public static readonly StyledProperty<IList<Point>> PointsProperty =
        AvaloniaProperty.Register<MyCustomShape, IList<Point>>("Points");

    static MyCustomShape()
    {
        StrokeThicknessProperty.OverrideDefaultValue<Polyline>(1);
        AffectsGeometry<Polyline>(PointsProperty);
    }

    public MyCustomShape()
    {
        SetValue(PointsProperty, new Points(), BindingPriority.Template);
    }

    public IList<Point> Points
    {
        get => GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    protected override Geometry CreateDefiningGeometry()
    {
        return new MyCustomShapeGeometry { Points = Points, IsFilled = false };
    }
}

  public class MyCustomShapeGeometry : StreamGeometry
    {
        /// <summary>
        /// Defines the <see cref="Points"/> property.
        /// </summary>
        public static readonly DirectProperty<PolylineGeometry, IList<Point>> PointsProperty =
            AvaloniaProperty.RegisterDirect<PolylineGeometry, IList<Point>>(nameof(Points), g => g.Points, (g, f) => g.Points = f);

        /// <summary>
        /// Defines the <see cref="IsFilled"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsFilledProperty =
            AvaloniaProperty.Register<PolylineGeometry, bool>(nameof(IsFilled));

        private IList<Point> _points;
        private IDisposable? _pointsObserver;

        static MyCustomShapeGeometry()
        {
            AffectsGeometry(IsFilledProperty);
            PointsProperty.Changed.AddClassHandler<MyCustomShapeGeometry>((s, e) => s.OnPointsChanged(e.NewValue as IList<Point>));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolylineGeometry"/> class.
        /// </summary>
        public MyCustomShapeGeometry()
        {
            _points = new Points();
            
            Debug.WriteLine("MyCustomShapeGeometry Ctor");

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolylineGeometry"/> class.
        /// </summary>
        public MyCustomShapeGeometry(IEnumerable<Point> points, bool isFilled)
        {
            _points = new Points(points);
            IsFilled = isFilled;
            
            Debug.WriteLine("MyCustomShapeGeometry Ctor");
        }

        /// <summary>
        /// Gets or sets the figures.
        /// </summary>
        /// <value>
        /// The points.
        /// </value>
        [Content]
        public IList<Point> Points
        {
            get => _points;
            set => SetAndRaise(PointsProperty, ref _points, value);
        }

        public bool IsFilled
        {
            get => GetValue(IsFilledProperty);
            set => SetValue(IsFilledProperty, value);
        }

        /// <inheritdoc/>
        public override Geometry Clone()
        {
            return new MyCustomShapeGeometry(Points, IsFilled);
        }
        
        private void RedefineGeometry()
        {
            Debug.WriteLine("MyCustomShapeGeometry RedefineGeometry");

            using (var context = Open())
            {
                if (Points.Count > 0)
                {
                    context.BeginFigure(Points[0], IsFilled);
                    for (var i = 1; i < Points.Count; i++)
                    {
                        context.LineTo(Points[i]);
                    }
                    context.EndFigure(IsFilled); 
                }
            }
        }

        private void OnPointsChanged(IList<Point>? newValue)
        {
            RedefineGeometry();
            Debug.WriteLine("MyCustomShapeGeometry OnPointsChanged");
            _pointsObserver?.Dispose();
            _pointsObserver = (newValue as INotifyCollectionChanged)
                ?.GetWeakCollectionChangedObservable()
                .Subscribe(_ => InvalidateGeometry());
        }
    }