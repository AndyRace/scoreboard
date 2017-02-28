using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using System.Net;
using System.Globalization;
using SelfHostingHttpServer;
using System.Threading;

namespace SelfHostedHttpServer
{

  public class ServerBase
  {
    const int DefaultReponseStreamBufferSize = 8120;
    const int CancellationTimeoutMs = 1000;

    //public class RequestEventArgs : EventArgs
    //{
    //}

    //public delegate void RequestEventHandler(object sender, RequestEventArgs args);

    //public event RequestEventHandler Request;

    //protected virtual void OnRequest(RequestEventArgs e)
    //{
    //    Request?.Invoke(this, e);
    //}

    public async void Start(int port = 80)
    {
      var listener = new StreamSocketListener();

      await listener.BindServiceNameAsync(port.ToString());

      listener.ConnectionReceived += async (sender, args) =>
      {
        try
        {
          // Logger.WriteLn($"Connection reveived: Start {args.Socket.Information.RemoteAddress}");

          var myWebRequest = new SelfHostedWebRequest(args.Socket);
          //using (var output = args.Socket.OutputStream)
          var output = args.Socket.OutputStream;
          // using (var responseStream = output.AsStreamForWrite())
          var responseStream = output.AsStreamForWrite();
          {
            try
            {
              await myWebRequest.Initialise();

              var response = await CreateResponse(myWebRequest);

              await WriteResponse(responseStream, response);
            }
            catch (Exception ex)
            {
              string error;
              var exType = ex.GetType();
              if (exType == typeof(ResponseException))
              {
                var responseEx = (ResponseException)ex;
                error = $"{responseEx.ErrorCode} {responseEx.ErrorCodeShortDescription}";
              }
              else if (exType == typeof(DirectoryNotFoundException)
                                || exType == typeof(FileNotFoundException))
              {
                error = "404 Not Found";
              }
              else
              {
                error = "400 Bad Request";
              }

              // todo: Map exceptions to status codes
              // See: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes
              var info = WebUtility.HtmlEncode(ex.Message).Replace("\r\n", "<br/>");
              info += "<hr/>";
              // TODO: DEBUG only
              info += WebUtility.HtmlEncode(ex.StackTrace).Replace("\r\n", "<br/>");

              var html = $"<html>" +
                                    $"<head>" +
                                        $"<title>Error</title>" +
                                    $"</head>" +
                                    $"<body>" +
                                        $"<h1>{error}</h1>" +
                                        $"{info}" +
                                    $"</body>" +
                                $"</html>";
              await WriteResponse(responseStream, html, error);
            }
          }
        }
        catch (Exception ex)
        {
          // todo: LOG THIS FATAL ERROR
          Logger.LogEvent("Exception in listener.ConnectionReceived", ex);
        }
        finally
        {
          // Logger.WriteLn("Connection reveived: End");
        }
      };
    }

    protected WebResponse CreateApiResponse(string body = "")
    {
      return new HtmlResponse(body);
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
    protected virtual async Task<WebResponse> CreateResponse(WebRequest request)
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

      return await CreateHtmlResponse(request.Method, body.ToString());
    }

    protected async virtual Task<WebResponse> CreateHtmlResponse(string h1, string body)
    {
      return await Task.Run(() => new HtmlResponse($"<html><head>" +
          $"<title>Background Message</title>" +
          $"</head>" +
          $"<h1>{WebUtility.HtmlEncode(h1)}</h1>" +
          $"<body>{body}</body>" +
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
          $"Date: {DateTime.Now.ToString("R", DateTimeFormatInfo.InvariantInfo)}\r\n" +
          $"Connection: close\r\n");

      foreach (var header in response.Headers.AllKeys)
      {
        AppendTo(responseStream, $"{header}: {response.Headers[header]}\r\n");
      }
      AppendTo(responseStream, "\r\n");

      await response.GetResponseStream().CopyToAsync(
        responseStream,
        DefaultReponseStreamBufferSize,
        new CancellationTokenSource(CancellationTimeoutMs).Token);

      await responseStream.FlushAsync(new CancellationTokenSource(CancellationTimeoutMs).Token);
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