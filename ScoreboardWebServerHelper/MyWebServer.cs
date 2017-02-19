using SelfHostedHttpServer;
using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net;

namespace ScoreboardWebServerHelper
{
    public class MyWebServer : ServerBase
    {
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
                        // return await GetResponse(method, $"TODO: Load Web resource: {arg}");
                        return new HtmlResponseFromFile(arg);

                    default:
                        throw new InvalidApiMethodException($"Unsupported method ({method}). Expecting GET only for web resources");
                }
            }

            throw new InvalidRequestException($"Invalid request ({pathAndQuery})");
        }
    }
}