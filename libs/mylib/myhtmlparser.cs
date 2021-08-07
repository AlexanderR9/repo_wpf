using System;
using System.IO;
using AngleSharp.Html.Parser;
using AngleSharp.Html.Dom;
using AngleSharp.Dom;
using System.Diagnostics;

using mylib.fileworker;

namespace mylib.htmlparser
{
    //обобщенный парсер, который из кода html делает Dom
    public class MyHtmlParser
    {
        public MyHtmlParser() 
        {
            reset();
        }

        private string last_err;
        private string html_text;
        private IHtmlDocument m_dom;

        private void reset() {last_err = ""; html_text = ""; m_dom = null;}
        public bool hasErr() {return (last_err != "");}
        public string errValue() => last_err;
        public IHtmlDocument Dom {get {return m_dom;} private set {m_dom = value;}}

        public void setHtmlData(string s) 
        {
            reset(); 
            html_text = s.Trim(); 
            tryLoadDom();
        }
        public void setHtmlFile(string path)
        {
            reset();

            MyFileWorker fw = new MyFileWorker(path);
            fw.tryRead();
            if (fw.hasErr()) last_err = fw.errValue();
            else setHtmlData(fw.data());
        }

        private void tryLoadDom() //загрузить html_text в m_dom
        {
            if (html_text == "")
            {
                last_err = "HTML data is empty.";
                return;
            }

            HtmlParser prs = new HtmlParser();
            try {Dom = prs.ParseDocument(html_text);}
            catch {last_err = String.Format("invalid load DOM struct");}
            finally 
            {
                //Console.WriteLine("DOM loaded.");
                if (!m_dom.HasChildNodes)
                    last_err = String.Format("DOM struct is empty");
            }
        }
    }

}

