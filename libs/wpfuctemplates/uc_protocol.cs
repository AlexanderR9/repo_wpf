using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;


namespace wpfuctemplates
{
    public partial class MyProtocolTeplate : UserControl
    {
        public enum MsgType {mtText = 300, mtErr, mtWarning, mtHeader, mtOk, mtTest};
        public MyProtocolTeplate()
        {
            InitializeComponent();
            with_time = true;
        }
        public void setBoxTitle(string s) => box.Header = s;
        public void setFontSize(int a) => textObject.FontSize = a;
        public void addMessage(string s, int type = (int)MsgType.mtText)
        {
            if (with_time) s = String.Format("[{0}]   {1}", DateTime.Now.ToString("HH:mm:ss"), s);
            Color text_color = (Color)ColorConverter.ConvertFromString(colorByType(type));

            Paragraph p = new Paragraph();
            p.Inlines.Add(s);
            p.Foreground = new SolidColorBrush(text_color);
            textObject.Document.Blocks.Add(p);
            textObject.ScrollToEnd();

            //Console.WriteLine("protocol blocks: {0}", textObject.Document.Blocks.Count);


            trim();
        }
        private void trim() //если число строк превысит 100, то удаляет 1-е записи
        {
            int n = textObject.Document.Blocks.Count - 100;
            if (n < 2) return;

            for (int i=0; i<n; i++)
                textObject.Document.Blocks.Remove(textObject.Document.Blocks.FirstBlock);
        }


        public bool with_time {get; set;}
        private static string colorByType(int msg_type)
        {
            switch ((MsgType)msg_type)
            {
                case MsgType.mtText : return "Black";
                case MsgType.mtErr : return "Crimson";
                case MsgType.mtWarning : return "Coral";
                case MsgType.mtHeader : return "ForestGreen";
                case MsgType.mtOk : return "Blue";
                case MsgType.mtTest : return "Sienna";
                
                default: break;
            }
            return "LightGray";
        }

    }
}