using SelfHostedHttpServer;
using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net;

namespace ScoreboardWebServerHelper
{
    public class MyWebServer : ServerBase
    {
        protected override async Task<WebResponse> GetResponse(WebRequest request)
        {
            var pathAndQuery = request.RequestUri.PathAndQuery;
            var method = request.Method;
            var m = Regex.Match(pathAndQuery, @"^/api/(.*)$",
                                RegexOptions.IgnoreCase | RegexOptions.Compiled,
                                TimeSpan.FromSeconds(1));
            if (m.Success)
            {
                var arg = m.Groups[1].Value;

                switch (method)
                {
                    case "GET":
                        return await GetResponse(method, $"TODO: Execute: {arg}");

                    case "PUT":
                        return await GetResponse(method, $"TODO: Execute: {arg}");

                    case "POST":
                        return await GetResponse(method, $"TODO: Execute: {arg}");

                    default:
                        throw new InvalidApiMethodException($"Unsupported method ({method}). Expecting GET, PUT or POST for web api");
                }
            }

            m = Regex.Match(pathAndQuery, @"^/web/(.*)$",
                            RegexOptions.IgnoreCase | RegexOptions.Compiled,
                            TimeSpan.FromSeconds(1));
            if (m.Success)
            {
                var arg = m.Groups[1].Value;

                switch (method)
                {
                    case "GET":
                        return new HtmlResponseFromFile(arg);

                    default:
                        throw new InvalidApiMethodException($"Unsupported method ({method}). Expecting GET only for web resources");
                }
            }

            throw new InvalidRequestException($"Invalid request ({pathAndQuery})");
        }
    }
}