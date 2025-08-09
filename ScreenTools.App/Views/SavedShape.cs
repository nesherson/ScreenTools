namespace ScreenTools.App;

public class SavedShape
{
    public ShapeType ShapeType { get; set; }
    public SavedPoint[]? Points { get; set; }
    public SavedPoint? StartPoint { get; set; }
    public SavedPoint? EndPoint { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public string? StrokeColor { get; set; }
    public string? FillColor { get; set; }
    public double? StrokeWidth { get; set; }
}

public class SavedPoint
{
    public SavedPoint(double x, double y)
    {
        X = x;
        Y = y;
    }
    public double X { get; set; }
    public double Y { get; set; }
}