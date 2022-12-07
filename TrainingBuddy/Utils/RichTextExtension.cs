using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace TrainingBuddy.Utils
{
    public static class RichTextExtension
    {
        public static void AppendText(this RichTextBox box, string text, string color)
        {
            BrushConverter bc = new BrushConverter();
            TextRange tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
            tr.Text = text;
            try
            {
                tr.ApplyPropertyValue(TextElement.ForegroundProperty,
                    bc.ConvertFromString(color));
            }
            catch (FormatException) { }
        }

    }
}
