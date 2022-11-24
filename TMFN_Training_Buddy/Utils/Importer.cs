using System;
using System.Runtime.InteropServices;

namespace TMFN_Training_Buddy.Utils
{
    public class Importer
    {
        [DllImport("user32.dll")]
        static extern int SetWindowText(IntPtr hWnd, string text);

        public void UseSetWindowText(IntPtr hWnd, string text)
        {
            SetWindowText(hWnd, text);
        }
    }
}
