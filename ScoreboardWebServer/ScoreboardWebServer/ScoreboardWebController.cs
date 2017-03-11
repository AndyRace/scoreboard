using Catnap.Server;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Windows.Web.Http;

namespace ScoreboardWebServer
{
  [RoutePrefix("/")]
  class ScoreboardWebController : Controller
  {
    // test
    //      POST, test=true|false
    //          Start/Stop the test
    //        test
    // text
    //      POST, {text: <text to display>}
    // value
    //      GET/PUT, {key: <score group>, value: <score>}
    [HttpGet, Route("/")]
    public HttpResponseBase GetRoot()
    {
      Debug.WriteLine("Get: index.html");

      return GetWeb("index.html");
    }

    [HttpGet, Route("/favicon.ico")]
    public HttpResponseBase GetFavicon()
    {
      Debug.WriteLine("GetFavicon");

      return GetWeb("favicon.ico");
    }

    [HttpGet, Route("/web/{path}")]
    public HttpResponseBase GetWeb(string path)
    {
      Debug.WriteLine($"GetWeb: {path}");

      return new FileResponse($"web/{path}");
    }

    [HttpGet, DefaultRoute]
    public HttpResponseBase GetDefaultWeb(string path)
    {
      Debug.WriteLine($"GetWeb: {path}");

      return new FileResponse($"web/{path}");
    }

    //    // test
    //    //      POST, test=true|false
    //    //          Start/Stop the test
    //    //        test
    //    // text
    //    //      POST, {text: <text to display>}
    //    // value
    //    //      GET/PUT, {key: <score group>, value: <score>}
    [HttpPut, Route("api/test\\?running={tof}")]
    public HttpResponse RunTest(string tof)
    {
      var info = $"Test: {tof}";
      Debug.WriteLine(info);
      return new HttpResponse(HttpStatusCode.Ok, info);
    }

    private Dictionary<string, int> Values = new Dictionary<string, int> { { "total", 0 }, { "wickets", 0 }, { "overs", 0 }, { "firstInnings", 0 } };

    [HttpPut, Route("api/value\\?group={group}&value={value}")]
    public HttpResponse SetValue(string group, string value)
    {
      var info = $"SetValue: {group}={value}";
      Debug.WriteLine(info);

      if (Values.ContainsKey(group))
      {
        if (int.TryParse(value, out int result))
        {
          Values[group] = result;
          return new HttpResponse(HttpStatusCode.Ok);
        }
        else
          return new HttpResponse(HttpStatusCode.NotImplemented, $"Invalid value '{value}' for group '{group}'");
      }
      else
        return new HttpResponse(HttpStatusCode.NotImplemented, $"Unknown group '{group}'");
    }

    [HttpGet, Route("api/value\\?group={group}")]
    public HttpResponse GetValue(string group)
    {
      var info = $"GetValue: {group}";
      Debug.WriteLine(info);

      if (Values.TryGetValue(group, out int result))
        return new JsonResponse(new { Value = result, Group = group });
      else
        return new HttpResponse(HttpStatusCode.NotImplemented, $"Unknown group '{group}'");
    }
  }

  //protected override async Task<WebResponse> CreateResponse(WebRequest request)
  //{
  //  var pathAndQuery = request.RequestUri.PathAndQuery;
  //  var method = request.Method;

  //  var m = Regex.Match(pathAndQuery, @"^/api/([^?]*)\??(.*)?$",
  //                      RegexOptions.IgnoreCase | RegexOptions.Compiled,
  //                      TimeSpan.FromSeconds(1));

  //  if (m.Success)
  //  {
  //    var fn = m.Groups[1].Value;

  //    var body = "";
  //    {
  //      var sbRequest = new StringBuilder();
  //      var input = await request.GetRequestStreamAsync();
  //      var buffer = new byte[BufferSize];
  //      int dataRead;
  //      do
  //      {
  //        dataRead = await input.ReadAsync(buffer, 0, BufferSize);
  //        sbRequest.Append(Encoding.UTF8.GetString(buffer, 0, dataRead));
  //      } while (dataRead == BufferSize);

  //      body = sbRequest.ToString();
  //    }

  //    Dictionary<string, string> parameters;
  //    {
  //      if (m.Groups.Count > 1)
  //      {
  //        string[] t;
  //        t = m.Groups[2].Value.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
  //        parameters = t.ToDictionary(s => WebUtility.HtmlDecode(s.Split('=')[0]), s => WebUtility.HtmlDecode(s.Split('=')[1]));
  //      }
  //      else
  //      {
  //        parameters = null;
  //      }
  //    }

  //    // test
  //    //      POST, test=true|false
  //    //          Start/Stop the test
  //    //        test
  //    // text
  //    //      POST, {text: <text to display>}
  //    // value
  //    //      GET/PUT, {key: <score group>, value: <score>}
  //    switch (fn)
  //    {
  //      case "test":
  //        {
  //          if (method == "POST")
  //          {
  //            var tof = (parameters != null && parameters.ContainsKey("test")) ? parameters["test"] : null;

  //            if (tof != null && tof.ToLower() == "true")
  //            {
  //              _cts.Cancel();

  //              _cts = new CancellationTokenSource();
  //              //_cts.CancelAfter(10000);
  //              await ThreadPool.RunAsync(async workItem =>
  //              {
  //                await controller.Test(_cts.Token);
  //              });
  //            }
  //            else
  //            {
  //              _cts.Cancel();
  //            }

  //            // return await GetHtmlResponse(method, $"TODO: Execute test");
  //            return CreateApiResponse();
  //          }
  //          break;
  //        }

  //      case "text":
  //        {
  //          if (method == "POST")
  //          {
  //            controller.AddText(body);

  //            // return await CreateHtmlResponse(method, $"TODO: Update text. Text='{body}'");
  //            return CreateApiResponse();
  //          }

  //          break;
  //        }

  //      case "value":
  //        {
  //          var group = (parameters != null && parameters.ContainsKey("group")) ? parameters["group"] : null;

  //          switch (method)
  //          {
  //            case "GET":
  //              {
  //                var result = controller.GetValue(group);

  //                JsonObject jsonObject = new JsonObject();
  //                if (result < 0)
  //                {
  //                  jsonObject["Value"] = JsonValue.CreateStringValue("NaN");
  //                }
  //                else
  //                {
  //                  jsonObject["Value"] = JsonValue.CreateNumberValue(result);
  //                }
  //                jsonObject["Group"] = JsonValue.CreateStringValue(group);

  //                string jsonString = jsonObject.Stringify();

  //                // return await CreateHtmlResponse(method, $"TODO: Get value. Which: '{group}'");
  //                return CreateApiResponse(jsonString);
  //              }

  //            case "PUT":
  //              {
  //                if (parameters == null || !parameters.ContainsKey("value"))
  //                  throw new InvalidApiMethodException($"Setting value doesn't include a value");

  //                var valueStr = parameters["value"];
  //                int value;
  //                if (valueStr == "NaN")
  //                {
  //                  value = -1;
  //                }
  //                else
  //                {
  //                  if (!int.TryParse(valueStr, out value))
  //                  {
  //                    throw new InvalidApiMethodException($"Invalid integer value ({valueStr}) for group ({group})");
  //                  }
  //                }

  //                controller.SetValue(group, value);

  //                // return await CreateHtmlResponse(method, $"TODO: Set value. Which: '{group}', value: '{value}'");
  //                return CreateApiResponse();
  //              }
  //          }
  //          break;
  //        }

  //      default:
  //        throw new InvalidApiMethodException($"Unsupported web api request ({fn})");
  //    }

  //    throw new InvalidApiMethodException($"Unsupported method ({method}) for function '{fn}'");
  //  }

  //  m = Regex.Match(pathAndQuery, @"^/(.*)$",
  //                  RegexOptions.IgnoreCase | RegexOptions.Compiled,
  //                  TimeSpan.FromSeconds(1));
  //  if (m.Success)
  //  {
  //    var arg = m.Groups[1].Value;

  //    switch (method)
  //    {
  //      case "GET":
  //        // todo: don't allow relative paths
  //        if (arg.StartsWith("."))
  //          throw new InvalidFilenameException($"Invalid filename requested ({arg})");

  //        return new HtmlResponseFromFile($"{arg}");

  //      default:
  //        throw new InvalidApiMethodException($"Unsupported method ({method}). Expecting GET only for web resources");
  //    }
  //  }

  //  throw new InvalidRequestException($"Invalid request ({pathAndQuery})");
  //}
}
