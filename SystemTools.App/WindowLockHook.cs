using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;

namespace SystemTools.App;

public class WindowLockHook
{
    public static void Hook(Window window)
    {
        Win32Properties.AddWndProcHookCallback(window, WndProc);
    }
    private static IntPtr WndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_WINDOWPOSCHANGING)
        {
            var wp = Marshal.PtrToStructure<WINDOWPOS>(lParam);
            wp.flags |= SWP_NOMOVE | SWP_NOSIZE;
            Marshal.StructureToPtr(wp, lParam, false);
        }

        return IntPtr.Zero;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    private struct WINDOWPOS
    {
        public IntPtr hwnd;
        public IntPtr hwndInsertAfter;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public int flags;
    }

    private const int
        SWP_NOMOVE = 0x0002,
        SWP_NOSIZE = 0x0001;

    private const int
        WM_WINDOWPOSCHANGING = 0x0046;
}