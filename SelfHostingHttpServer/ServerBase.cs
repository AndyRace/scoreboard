using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Net;
using System.Text.RegularExpressions;

namespace SelfHostedHttpServer
{

    public class ServerBase
    {
        private const string Title = "Self-Hosted Server";

        //public class RequestEventArgs : EventArgs
        //{
        //}

        //public delegate void RequestEventHandler(object sender, RequestEventArgs args);

        //public event RequestEventHandler Request;

        //protected virtual void OnRequest(RequestEventArgs e)
        //{
        //    Request?.Invoke(this, e);
        //}

        public async void Start(int port = 8081)
        {
            var listener = new StreamSocketListener();

            await listener.BindServiceNameAsync(port.ToString());

            listener.ConnectionReceived += async (sender, args) =>
            {
                try
                {
                    var myWebRequest = new MyWebRequest(args.Socket);
                    using (var output = args.Socket.OutputStream)
                    using (var responseStream = output.AsStreamForWrite())
                    {
                        try
                        {
                            await myWebRequest.Initialise();

                            var response = await GetResponse(myWebRequest);

                            await WriteResponse(responseStream, response);
                        }
                        catch (ResponseException ex)
                        {
                            var info = WebUtility.HtmlEncode(ex.Message).Replace("\r\n", "<br/>");
                            var html = $"<html><head><title>{Title}</title></head><body><h1>Error</h1>{info}</body></html>";
                            await WriteResponse(responseStream, html, $"{ex.ErrorCode} {WebUtility.HtmlEncode(ex.ErrorCodeShortDescription)}");
                        }
                        catch (Exception ex)
                        {
                            // See: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes
                            var info = WebUtility.HtmlEncode(ex.Message).Replace("\r\n", "<br/>");
                            var html = $"<html><head><title>{Title}</title></head><body><h1>Error</h1>{info}</body></html>";
                            await WriteResponse(responseStream, html, "400 Bad Request");
                        }
                    }
                }
                catch
                {
                    // todo: LOG THIS FATAL ERROR
                }
            };
        }

        /// <summary>
        /// Get the response
        /// </summary>
        /// <param name="bag">
        /// header
        ///     .method:   GET, PUT, POST
        ///     .query:     The request's query
        ///     .version:   1.1
        /// 
        /// attributes[]
        ///     .Item1:     request header attribute name
        ///     .Item2:     request header attribute value
        /// 
        /// body:   The PUT/POST payload
        /// </param>
        /// <returns>
        /// The response as a Stream.
        /// </returns>
        protected virtual async Task<WebResponse> GetResponse(WebRequest request)
        {
            var body = new StringBuilder(
                    $"Command: {WebUtility.HtmlEncode(request.Method)}<br/>" +
                    $"Query: {WebUtility.HtmlEncode(request.RequestUri.Query)}<br/>" +
                    $"<br/>" +
                    $"<table style=\"text-align:left\"><tr><th>Key</th><th>Value</th></tr>");

            var headerCollection = request.Headers;
            foreach (var attrib in headerCollection.AllKeys)
            {
                body.Append($"<tr><td>{WebUtility.HtmlEncode(attrib)}</td><td>{WebUtility.HtmlEncode(headerCollection[attrib])}</td></tr>");
            }

            var requestPayload = await request.GetRequestStreamAsync();
            StreamReader reader = new StreamReader(requestPayload);
            string text = reader.ReadToEnd();

            body.Append($"<tr><td>Body</td><td>{WebUtility.HtmlEncode(text)}</td></tr>" +
                    "</table>");

            return await GetResponse(request.Method, body.ToString());
        }

        protected async virtual Task<WebResponse> GetResponse(string header, string htmlBody)
        {
            return await Task.Run(() => new HtmlResponse($"<html><head>" +
                $"<title>Background Message</title>" +
                $"</head>" +
                $"<h1>{WebUtility.HtmlEncode(header)}</h1>" +
                $"<body>{htmlBody}</body>" +
                $"</html>"));
        }

        private static void AppendTo(Stream stream, string info)
        {
            var headerArray = Encoding.UTF8.GetBytes(info);
            stream.WriteAsync(headerArray, 0, headerArray.Length);
        }

        protected async static Task WriteResponse(Stream responseStream, WebResponse response, string responseCode = "200 OK")
        {
            //responseStream.Seek(0, SeekOrigin.Begin);

            AppendTo(responseStream, $"HTTP/1.1 {responseCode}\r\n" +
                $"Content-Length: {response.ContentLength}\r\n" +
                $"Content-Type: {response.ContentType}\r\n" +
                $"Connection: close\r\n");

            foreach (var header in response.Headers.AllKeys)
            {
                AppendTo(responseStream, $"{header}: {response.Headers[header]}\r\n");
            }
            AppendTo(responseStream, "\r\n");

            await response.GetResponseStream().CopyToAsync(responseStream);
            await responseStream.FlushAsync();
        }

        protected async static Task WriteResponse(Stream responseStream, string htmlString, string responseCode = "200 OK")
        {
            await WriteResponse(responseStream, new HtmlResponse(htmlString), responseCode);
        }

        // See: https://tools.ietf.org/html/rfc2616#section-9.3
        private static Uri GetQuery(StringBuilder request)
        {
            var requestLines = request.ToString().Split(new String[] { "\r\n" }, StringSplitOptions.None);
            if (requestLines.Length != 1) return null;

            var line = requestLines[1];
            var query = line.Split(' ');
            if (query.Length != 2) return null;

            return new Uri($"http://localhost/{query[0]}");
        }
    }
}