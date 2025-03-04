using Tesseract;

namespace ScreenTools.App;

public class ImageProcessingService
{
    private const string TesseractLanguage = "eng";
    private const string TesseractDataPath = "./tessdata";
    
    public string ProcessImage(byte[] imageBytes)
    {
        using var engine = new TesseractEngine(TesseractDataPath, TesseractLanguage, EngineMode.Default);
        using var img = Pix.LoadFromMemory(imageBytes);
        using var page = engine.Process(img);
        var text = page.GetText();
        
        return text;
    }
}