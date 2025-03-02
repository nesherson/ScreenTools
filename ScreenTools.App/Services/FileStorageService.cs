using System.IO;
using System.Threading.Tasks;

namespace ScreenTools.App;

public class FileStorageService : IStorageService<string>
{
    private readonly string _filePath;

    public FileStorageService(string filePath)
    {
        _filePath = filePath;
    }
    
    public async Task SaveData(string data)
    {
        await using var outputFile = new StreamWriter(_filePath);
        await outputFile.WriteAsync(data);
    }

    public async Task<string> LoadData()
    {
        StreamReader streamReader = new(_filePath);
        return await streamReader.ReadToEndAsync();
    }
}