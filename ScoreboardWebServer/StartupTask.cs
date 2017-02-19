using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using SelfHostedHttpServer;
using Windows.System.Threading;
using ScoreboardWebServerHelper;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace ScoreboardWebServer
{
    
    public sealed class StartupTask : IBackgroundTask
    {
        private static BackgroundTaskDeferral _Deferral = null;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _Deferral = taskInstance.GetDeferral();

            var webserver = new MyWebServer();

            await ThreadPool.RunAsync(workItem =>
            {
                webserver.Start();
            });
        }
    }
}
