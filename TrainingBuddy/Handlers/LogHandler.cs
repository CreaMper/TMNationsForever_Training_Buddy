using LogicStorage.Utils;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using TrainingBuddy.Utils;

namespace TrainingBuddy.Handlers
{
    public class LogHandler
    {
        public RichTextBox _rtb;
        public Dispatcher _dispather;

        public LogHandler(RichTextBox richTextBox, Dispatcher dispather)
        {
            _rtb = richTextBox;
            _dispather = dispather;
        }

        public void AddLog(string log)
        {
            _rtb.AppendText($"{log} \r");
        }

        public void AddLog(string log, LogTypeEnum type)
        {
            _dispather.Invoke(() => {
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
            }, DispatcherPriority.SystemIdle);
        }
    }
}
