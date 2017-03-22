using SelfHostedHttpServer;
using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using System.Threading;
using Windows.System.Threading;

namespace ScoreboardWebServerHelper
{
  public class ScoreboardWebServer : ServerBase, IDisposable
  {
    private const int BufferSize = 8192;

    private ScoreboardController controller = new ScoreboardController();

    private CancellationTokenSource _cts = new CancellationTokenSource();

    protected override async Task<WebResponse> CreateResponse(WebRequest request)
    {
      var pathAndQuery = request.RequestUri.PathAndQuery;
      var method = request.Method;

      var m = Regex.Match(pathAndQuery, @"^/api/([^?]*)\??(.*)?$",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled,
                          TimeSpan.FromSeconds(1));

      if (m.Success)
      {
        var fn = m.Groups[1].Value;

        var body = "";
        {
          var sbRequest = new StringBuilder();
          var input = await request.GetRequestStreamAsync();
          var buffer = new byte[BufferSize];
          int dataRead;
          do
          {
            dataRead = await input.ReadAsync(buffer, 0, BufferSize);
            sbRequest.Append(Encoding.UTF8.GetString(buffer, 0, dataRead));
          } while (dataRead == BufferSize);

          body = sbRequest.ToString();
        }

        Dictionary<string, string> parameters;
        {
          if (m.Groups.Count > 1)
          {
            string[] t;
            t = m.Groups[2].Value.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            parameters = t.ToDictionary(s => WebUtility.HtmlDecode(s.Split('=')[0]), s => WebUtility.HtmlDecode(s.Split('=')[1]));
          }
          else
          {
            parameters = null;
          }
        }

        // test
        //      POST, test=true|false
        //          Start/Stop the test
        //        test
        // text
        //      POST, {text: <text to display>}
        // value
        //      GET/PUT, {key: <score group>, value: <score>}
        switch (fn)
        {
          case "test":
            {
              if (method == "POST")
              {
                var tof = (parameters != null && parameters.ContainsKey("test")) ? parameters["test"] : null;

                if (tof != null && tof.ToLower() == "true")
                {
                  _cts.Cancel();

                  _cts = new CancellationTokenSource();
                  //_cts.CancelAfter(10000);
                  await ThreadPool.RunAsync(async workItem =>
                  {
                    await controller.Test(_cts.Token);
                  });
                }
                else
                {
                  _cts.Cancel();
                }

                // return await GetHtmlResponse(method, $"TODO: Execute test");
                return CreateApiResponse();
              }
              break;
            }

          case "text":
            {
              if (method == "POST")
              {
                controller.AddText(body);

                // return await CreateHtmlResponse(method, $"TODO: Update text. Text='{body}'");
                return CreateApiResponse();
              }

              break;
            }

          case "value":
            {
              var group = (parameters != null && parameters.ContainsKey("group")) ? parameters["group"] : null;

              switch (method)
              {
                case "GET":
                  {
                    var result = controller.GetValue(group);

                    JsonObject jsonObject = new JsonObject();
                    if (result < 0)
                    {
                      jsonObject["Value"] = JsonValue.CreateStringValue("NaN");
                    }
                    else
                    {
                      jsonObject["Value"] = JsonValue.CreateNumberValue(result);
                    }
                    jsonObject["Group"] = JsonValue.CreateStringValue(group);

                    string jsonString = jsonObject.Stringify();

                    // return await CreateHtmlResponse(method, $"TODO: Get value. Which: '{group}'");
                    return CreateApiResponse(jsonString);
                  }

                case "PUT":
                  {
                    if (parameters == null || !parameters.ContainsKey("value"))
                      throw new InvalidApiMethodException($"Setting value doesn't include a value");

                    var valueStr = parameters["value"];
                    int value;
                    if (valueStr == "NaN")
                    {
                      value = -1;
                    }
                    else
                    {
                      if (!int.TryParse(valueStr, out value))
                      {
                        throw new InvalidApiMethodException($"Invalid integer value ({valueStr}) for group ({group})");
                      }
                    }

                    controller.SetValue(group, value);

                    // return await CreateHtmlResponse(method, $"TODO: Set value. Which: '{group}', value: '{value}'");
                    return CreateApiResponse();
                  }
              }
              break;
            }

          default:
            throw new InvalidApiMethodException($"Unsupported web api request ({fn})");
        }

        throw new InvalidApiMethodException($"Unsupported method ({method}) for function '{fn}'");
      }

      m = Regex.Match(pathAndQuery, @"^/(.*)$",
                      RegexOptions.IgnoreCase | RegexOptions.Compiled,
                      TimeSpan.FromSeconds(1));
      if (m.Success)
      {
        var arg = m.Groups[1].Value;

        switch (method)
        {
          case "GET":
            // todo: don't allow relative paths
            if (arg.StartsWith("."))
              throw new InvalidFilenameException($"Invalid filename requested ({arg})");

            return new HtmlResponseFromFile($"{arg}");

          default:
            throw new InvalidApiMethodException($"Unsupported method ({method}). Expecting GET only for web resources");
        }
      }

      throw new InvalidRequestException($"Invalid request ({pathAndQuery})");
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected override void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          if (_cts != null)
          {
            _cts.Dispose();
            _cts = null;
          }
          // TODO: dispose managed state (managed objects).
        }

        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
        // TODO: set large fields to null.

        disposedValue = true;
      }

      base.Dispose(disposing);
    }
    #endregion
  }
}