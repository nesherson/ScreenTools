using System.Diagnostics;

namespace ScreenTools.App;

public class ProcessHelpers
{
    public static void ShowFileInFileExplorer(string pathToFile)
    {
        Process.Start("explorer.exe", $"/select, \"{pathToFile}\"");
    }
}