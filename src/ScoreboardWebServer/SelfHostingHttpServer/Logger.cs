using System;
using Windows.Foundation.Diagnostics;

namespace SelfHostingHttpServer
{
  public class Logger
  {
    // Log to the existing Microsoft-Windows-Diagnostics-LoggingChannel for simplicity
    static public readonly LoggingChannel LoggingChannel = new LoggingChannel(typeof(Logger).FullName, null, new Guid("4bd2826e-54a1-4ba9-bf63-92b73ea1ac4a"));

    public static void LogException(string info, Exception ex, LoggingLevel level = LoggingLevel.Error)
    {
      LoggingFields fields = new LoggingFields();
      fields.AddString("Exception.Message", ex.Message);
      fields.AddString("Exception.StackTrace", ex.StackTrace);
      LoggingChannel.LogEvent(info, fields, level);
    }
  }
}
