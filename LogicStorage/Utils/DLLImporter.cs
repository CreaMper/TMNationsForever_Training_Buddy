using System;
using System.Runtime.InteropServices;

namespace LogicStorage.Utils
{
    public class DLLImporter
    {
        [DllImport("user32.dll")]
        private static extern int SetWindowText(IntPtr hWnd, string text);

        public void UseSetWindowText(IntPtr hWnd, string text)
        {
            SetWindowText(hWnd, text);
        }

        [DllImport("user32.dll")]
        internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        public void UseSetForegroundWindow(IntPtr hWnd)
        {
            SetForegroundWindow(hWnd);
        }
    }
}
