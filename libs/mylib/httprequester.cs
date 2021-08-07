using System;
using System.IO;
using System.Net.Http;

namespace mylib.htmlrequester
{
    public class HTTPRequester
    {
        public event Action<object> OnFinishedEvent;
        public HTTPRequester()
        {
            m_client = new HttpClient();
            m_client.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");
            m_url = String.Empty;

            resetRequest();
            ResultCode = 0;
        }

        private HttpClient m_client = null;
        private string m_url;
        private string last_err;
        
        private void resetRequest() {ResultCode = -1; last_err = ""; HTMLData = ""; Buzy = false;}
 
        public bool hasErr() {return (last_err != "");}
        public string errValue() => last_err;
        public void setUrl(string url) => m_url=url;
        public bool requestOk() {return (ResultCode == 200 && HTMLData.Length > 0);}

        public int ResultCode {get; private set;}
        public string HTMLData {get; private set;}
        public bool Buzy {get; private set;}
        public string strResult()
        {
            string s = "Result request:";
            s += String.Format("  code={0}", ResultCode);
            s += String.Format("  err=[{0}]", hasErr() ? errValue() : "NO_ERROR");
            s += String.Format("  html data size {0}", HTMLData.Length);
            return s;
        }

        public async void startRequest()
        {
            Console.WriteLine("\n Start request");
            resetRequest();
            checkUrl();
            if (hasErr()) return;

            Buzy = true;
            try
            {    
                HttpResponseMessage response = await m_client.GetAsync(m_url);
                ResultCode = (int)response.StatusCode;
                response.EnsureSuccessStatusCode();
                readResponse(response);
            }
            catch {last_err = String.Format("http request error, URL: [{0}]", m_url);}
            finally {/* to do, req OK*/}
            Buzy = false;
          	this.OnFinishedEvent?.Invoke(this);

            Console.WriteLine("request exit");
        }
        

        private void checkUrl()
        {
            m_url = m_url.Trim();
            if (m_url == "")
            {
                last_err = "URL value is empty.";
                return;
            }

            Uri  uri = new Uri(m_url);
            if (!uri.IsWellFormedOriginalString()) 
            {
                last_err = String.Format("URL value invalid: [{0}]", m_url);
                return;
            }
        }
        private async void readResponse(HttpResponseMessage response)
        {
            if (ResultCode != 200)
            {
                last_err = String.Format("invalid result code of request: [{0}]", ResultCode);
                return;
            }

            try {HTMLData = await response.Content.ReadAsStringAsync();}
            catch {last_err = String.Format("reading responce error, URL: [{0}]", m_url);}
            finally {/* to do, req OK*/}
            Console.WriteLine("readResponse exit");
        }
    }
}