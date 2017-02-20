using System;
using System.IO;
using System.Net;
using System.Text;

namespace SelfHostedHttpServer
{
    public abstract class MyWebResponse : WebResponse
    {
        public override WebHeaderCollection Headers { get; }

        public MyWebResponse()
        {
            Headers = new WebHeaderCollection();
        }
    }

    public class HtmlResponse : MyWebResponse
    {
        private string _html;

        public HtmlResponse(string html)
        {
            _html = html;
        }

        public override long ContentLength => _html.Length;

        // see: https://en.wikipedia.org/wiki/Media_type
        public override string ContentType => "text/html";

        public override Uri ResponseUri => null; // TBD throw new NotImplementedException();

        public override Stream GetResponseStream()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(_html.ToString()));
        }
    }

    public class HtmlResponseFromFile : MyWebResponse
    {
        private string _filename;
        private Stream _stream;

        public override long ContentLength => GetResponseStream().Length;

        // see: https://en.wikipedia.org/wiki/Media_type
        // todo: Need to map extension to conent type
        public override string ContentType => "text/html";

        public override Uri ResponseUri => null; // throw new NotImplementedException();

        public override Stream GetResponseStream()
        {
            if (_stream == null)
            {
                _stream = new FileStream($"web/{_filename}", FileMode.Open, FileAccess.Read);
            }
            return _stream;
        }

        public HtmlResponseFromFile(string filename)
        {
            _filename = filename;
        }
    }
}