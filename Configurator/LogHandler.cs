using System.Windows.Controls;

namespace Configurator
{
    public class LogHandler
    {
        private readonly TextBlock _tb;
        private readonly ScrollViewer _sv;

        public LogHandler(TextBlock texBox, ScrollViewer scrollViewer)
        {
            _tb = texBox;
            _tb.Text = "";
            _sv = scrollViewer;
            _sv.ScrollToEnd();
        }

        public void AddLog(string log)
        {
            _tb.Text += $"{log} \r";
        }



        public void Clean()
        {
            _tb.Text = "";
        }
    }
}
