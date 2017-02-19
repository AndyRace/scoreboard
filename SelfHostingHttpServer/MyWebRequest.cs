using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace SelfHostedHttpServer
{
    internal class MyWebRequest : WebRequest
    {
        private const uint BufferSize = 8192;
        private StreamSocket _socket;
        private Uri _requestUri;
        private Stream _requestStream;

        public override WebHeaderCollection Headers { get; set; }
        public override string Method { get; set; }

        public override Uri RequestUri { get => _requestUri; }
        public override string ContentType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override async Task<Stream> GetRequestStreamAsync()
        {
            //_requestStream.Seek(0, SeekOrigin.Begin);
            return await Task.Run(() => _requestStream);
        }

        public MyWebRequest(StreamSocket socket) : base()
        {
            _socket = socket;
            Headers = new WebHeaderCollection();
        }

        public async Task Initialise()
        {
            var sbRequest = new StringBuilder();

            using (var input = _socket.InputStream)
            {
                var data = new byte[BufferSize];
                IBuffer buffer = data.AsBuffer();
                var dataRead = BufferSize;

                while (dataRead == BufferSize)
                {
                    await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
                    sbRequest.Append(Encoding.UTF8.GetString(data, 0, (int)buffer.Length));
                    dataRead = buffer.Length;
                }
            }

            // The request is of the form:
            //  <method> <query> HTTP/<version>
            //  [<header attribute name>: <value>]*
            //
            //  [payload]?
            //
            // e.g.
            //  GET /a/query?more HTTP/1.1
            //  Host: minwinpc: 8081
            //  Connection: keep - alive
            //  Cache - Control: max - age = 0
            //  Upgrade - Insecure - Requests: 1
            //  User - Agent: Mozilla / 5.0(Windows NT 10.0; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 57.0.2987.54 Safari / 537.36
            //  Accept: text / html,application / xhtml + xml,application / xml; q = 0.9,image / webp,*/*;q=0.8
            //  Accept-Encoding: gzip, deflate, sdch
            //  Accept-Language: en-GB,en-US;q=0.8,en;q=0.6
            //  Cookie: CSRF-Token=uNDLATg79vXfPpavU5ch7YY4atbMaGA1
            var reader = new StringReader(sbRequest.ToString());

            var line = reader.ReadLine();

            var m = Regex.Match(line, @"^(.*) (.*) HTTP/(\d+.\d+)$",
                                RegexOptions.IgnoreCase | RegexOptions.Compiled,
                                TimeSpan.FromSeconds(1));
            if (!m.Success) throw new Exception($"Unexpected request header ({line})");

            var query = WebUtility.HtmlDecode(m.Groups[2].Value);
            Method = m.Groups[1].Value;

            if (m.Groups[3].Value != "1.1")
            {
                throw new HttpVersionNotSupportedException($"Only support for HTTP/1.1 ({m.Groups[3].Value})");
            }

            while ((line = reader.ReadLine()) != "")
            {
                if (line == null) throw new Exception($"Unexpected end of header");

                var attribs = Regex.Match(line, @"^([^:]+):\s*(.*)$",
                                 RegexOptions.IgnoreCase | RegexOptions.Compiled,
                                 TimeSpan.FromSeconds(1));

                Headers[attribs.Groups[1].Value] = attribs.Groups[2].Value;
            }

            // todo: Assumes http. The socket includes security info, but what if behind a loiad balancer ... really?? ... a Pi?
            _requestUri = new Uri($"http://{Headers["Host"]}{query}");

            // write the body as if it were the request
            {
                var body = await reader.ReadToEndAsync();

                _requestStream = new MemoryStream(Encoding.UTF8.GetBytes(body), 0, body.Length);
            }
        }

        public override void Abort()
        {
            throw new NotImplementedException();
        }

        public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public override Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public override WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }
    }
}
