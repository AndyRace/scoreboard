using Catnap.Server;
using ScoreboardFadeCandy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace ScoreboardWebServer
{
  [RoutePrefix("/")]
  class ScoreboardWebController : Controller, IDisposable
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

    private ScoreboardFadeCandyController _scoreboard;

    private async Task<ScoreboardFadeCandyController> GetScoreboardAsync()
    {
      if (_scoreboard == null)
      {
        _scoreboard = new ScoreboardFadeCandyController();
        await _scoreboard.InitialiseAsync();
      }

      return _scoreboard;
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
    public async Task<HttpResponse> RunTest(string tof)
    {
      var info = $"Test: {tof}";
      Debug.WriteLine(info);

      if (!bool.TryParse(tof, out bool value)) throw new ArgumentException("tof", $"Test runing argument invalid ({tof}). Expecting 'true' or 'false'");

      await _scoreboard.ExecuteTestAsync(value);

      return new HttpResponse(HttpStatusCode.Ok, info);
    }

    [HttpPut, Route("api/value\\?group={group}&value={value}")]
    public async Task<HttpResponse> SetValueAsync(string group, string value)
    {
      var info = $"SetValue: {group}={value}";
      Debug.WriteLine(info);

      await _scoreboard.Numbers.SetValueAsync(group, value);
      return new HttpResponse(HttpStatusCode.Ok);
    }

    [HttpGet, Route("api/value\\?group={group}")]
    public async Task<HttpResponse> GetValueAsync(string group)
    {
      var info = $"GetValue: {group}";
      Debug.WriteLine(info);

      uint? result = await _scoreboard.Numbers.GetValueAsync(group);

      return new JsonResponse(new { Value = result, Group = group });
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          if (_scoreboard != null)
          {
            _scoreboard.Dispose();
            _scoreboard = null;
          }
        }

        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
        // TODO: set large fields to null.

        disposedValue = true;
      }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~ScoreboardWebController() {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
      // TODO: uncomment the following line if the finalizer is overridden above.
      // GC.SuppressFinalize(this);
    }
    #endregion
  }
}