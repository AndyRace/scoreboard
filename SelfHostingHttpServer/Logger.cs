using System;
using Windows.Foundation.Diagnostics;

namespace SelfHostingHttpServer
{
  public class Logger
  {
    // Log to the existing Microsoft-Windows-Diagnostics-LoggingChannel for simplicity
    static readonly LoggingChannel _lc = new LoggingChannel(typeof(Logger).FullName, null, new Guid("4bd2826e-54a1-4ba9-bf63-92b73ea1ac4a"));

    public static void WriteLn(string info, LoggingLevel level = LoggingLevel.Information)
    {
      _lc.LogMessage(info, level);
    }

    public static void LogEvent(string info, Exception ex)
    {
      LoggingFields fields = new LoggingFields();
      fields.AddString("Exception.Message", ex.Message);
      fields.AddString("Exception.StackTrace", ex.StackTrace);
      _lc.LogEvent(info, fields);
    }
  }
}
