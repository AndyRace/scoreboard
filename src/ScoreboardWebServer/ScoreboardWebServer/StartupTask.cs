using System;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;
using System.Diagnostics;
using System.Threading;
using Catnap.Server;
using System.Threading.Tasks;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace ScoreboardWebServer
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Microsoft.Design",
    "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable",
    Justification = "CancellationTokenSource: Disposed of by other means")]
  public sealed class StartupTask : IBackgroundTask
  {
    private static BackgroundTaskDeferral _Deferral = null;
    private CancellationTokenSource _cts;
    private ScoreboardWebController _webController;

    public async void Run(IBackgroundTaskInstance taskInstance)
    {

      _Deferral = taskInstance.GetDeferral();

      _cts = new CancellationTokenSource();

      taskInstance.Canceled += TaskInstance_Canceled;

      var httpServer = new HttpServer(80);
      _webController = new ScoreboardWebController();
      httpServer.RestHandler.RegisterController(_webController);
      await ThreadPool.RunAsync(async workItem =>
      {
        await httpServer.StartServerAsync();
      });
    }

    private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
    {
      Debug.WriteLine("TaskInstance_Canceled");

      if (_cts != null)
      {
        _cts.Cancel();
        _cts.Dispose();
        _cts = null;
      }

      if(_webController != null)
      {
        _webController.Dispose();
        _webController = null;
      }
    }
  }
}
