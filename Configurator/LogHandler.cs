using LogicStorage.Utils;
using System.Windows.Controls;
using System.Windows.Media;
using Configurator.Utils;

namespace Configurator
{
    public class LogHandler
    {
        private RichTextBox _rtb;

        public LogHandler(RichTextBox richTextBox)
        {
            _rtb = richTextBox;
        }

        public void AddLog(string log)
        {
            _rtb.AppendText($"{log} \r");
        }

        public void AddLog(string log, LogTypeEnum type)
        {
            var prefix = "";
            if (type.Equals(LogTypeEnum.Info)) 
            {
                prefix = "[Info]";
                _rtb.SelectionTextBrush = Brushes.Blue;
                _rtb.AppendText($"{prefix} ", "lightblue");
            }
            else if (type.Equals(LogTypeEnum.Success)) 
            {
                prefix = "[Ok]";
                _rtb.SelectionTextBrush = Brushes.Green;
                _rtb.AppendText($"{prefix} ", "green");
            }
            else if (type.Equals(LogTypeEnum.Error)) 
            {
                prefix = "[Error]";
                _rtb.SelectionTextBrush = Brushes.DarkRed;
                _rtb.AppendText($"{prefix} ", "red");
            }
            else if (type.Equals(LogTypeEnum.CRITICAL)) 
            {
                prefix = "[CRITICAL]";
                _rtb.SelectionTextBrush = Brushes.Red;
                _rtb.AppendText($"{prefix} ", "red");
            }

            _rtb.AppendText($"{log} \r", "white");
            _rtb.ScrollToEnd();
        }
    }
}
