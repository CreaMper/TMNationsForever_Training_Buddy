using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace TMFN_Training_Buddy.Handlers
{
    public class LogHandler
    {
        private TextBlock _tb;
        private ScrollViewer _sv;
        public LogHandler(TextBlock texBox, ScrollViewer scrollViewer)
        {
            _tb = texBox;
            _tb.Text = "";
            _sv = scrollViewer;
            _sv.ScrollToEnd();
        }

        public void AddLog(string log)
        {
            _tb.Text = _tb.Text + $"{log} \r";
        }
    }
}
