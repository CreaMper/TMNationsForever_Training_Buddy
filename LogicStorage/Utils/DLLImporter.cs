using System;
using System.Runtime.InteropServices;

namespace LogicStorage.Utils
{
    public class DLLImporter
    {
        [DllImport("user32.dll")]
        static extern int SetWindowText(IntPtr hWnd, string text);

        public void UseSetWindowText(IntPtr hWnd, string text)
        {
            SetWindowText(hWnd, text);
        }
    }
}
