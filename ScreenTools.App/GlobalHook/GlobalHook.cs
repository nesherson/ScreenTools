using SharpHook.Native;
using SharpHook;
using System.Diagnostics;
using System.Windows;

namespace ScreenTools.App;

public class GlobalHook : IDisposable
{
    private TaskPoolGlobalHook _hook;

    public GlobalHook()
    {
        _hook = new TaskPoolGlobalHook();

        _hook.KeyTyped += Hook_KeyTyped;
    }

    public async Task RunAsync()
    {
        await _hook.RunAsync();
    }

    private void Hook_KeyTyped(object? sender, KeyboardHookEventArgs e)
    {
        //KeyCode->VcSlash
        //RawEvent.Mask->LeftShift, LeftCtrl
        Debug.WriteLine($"KeyCode -> {e.Data.KeyCode}");
        Debug.WriteLine($"RawEvent.Mask -> {e.RawEvent.Mask}");

        if (e.RawEvent.Mask == (ModifierMask.LeftShift | ModifierMask.LeftCtrl) &&
            e.Data.KeyCode == KeyCode.VcSlash)
        {
            MessageBox.Show("Success");
        }
    }

    public void Dispose()
    {
        _hook.Dispose();
    }
}
