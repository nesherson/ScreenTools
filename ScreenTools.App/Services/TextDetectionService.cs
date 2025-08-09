using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Tesseract;
using ImageFormat = Tesseract.ImageFormat;

namespace ScreenTools.App;

public class TextDetectionService
{
    private const string TesseractLanguage = "eng";
    private const string TesseractDataPath = "./tessdata";
    
    private string ProcessImage(byte[] imageBytes)
    {
        using var engine = new TesseractEngine(TesseractDataPath, TesseractLanguage, EngineMode.Default);
        using var img = Pix.LoadFromMemory(imageBytes);
        using var page = engine.Process(img);
        var text = page.GetText();
        
        return text;
    }

    public string DetectText(double x, double y, double width, double height)
    {
        if (width == 0 || height == 0)
            throw new ArgumentException("TextDetectError: Width and Height cannot be 0");

        var bmp = new Bitmap(Convert.ToInt32(width),
            Convert.ToInt32(height),
            PixelFormat.Format32bppArgb);

        using (var g = Graphics.FromImage(bmp))
            g.CopyFromScreen(Convert.ToInt32(x),
                Convert.ToInt32(y),
                0,
                0,
                bmp.Size,
                CopyPixelOperation.SourceCopy);

        var ms = new MemoryStream();

        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

        return ProcessImage(ms.ToArray())
            .Trim();
    }
}