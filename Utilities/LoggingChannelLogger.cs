using Caliburn.Micro;
using System;
using Windows.Foundation.Diagnostics;

namespace ScoreboardTest.Utils
{
  public class LoggingChannelLogger : ILog
  {
    // Log to the existing Microsoft-Windows-Diagnostics-LoggingChannel for simplicity
    static public readonly LoggingChannel LoggingChannel = new LoggingChannel(typeof(LoggingChannelLogger).FullName, null, new Guid("4bd2826e-54a1-4ba9-bf63-92b73ea1ac4a"));

    private Type _type;

    public LoggingChannelLogger(Type type)
    {
      _type = type;
    }

    //public static void LogException(string info, Exception ex, LoggingLevel level = LoggingLevel.Error)
    //{
    //  LoggingFields fields = new LoggingFields();
    //  fields.AddString("Exception.Message", ex.Message);
    //  fields.AddString("Exception.StackTrace", ex.StackTrace);
    //  LoggingChannel.LogEvent(info, fields, level);
    //}

    public void Error(Exception exception)
    {
      var fields = new LoggingFields();
      fields.AddString("Exception.Message", exception.Message);
      fields.AddString("Exception.StackTrace", exception.StackTrace);
      fields.AddString("Type", _type.Name);
      LoggingChannel.LogEvent("Error", fields, LoggingLevel.Error);
    }

    public void Info(string format, params object[] args)
    {
      var fields = new LoggingFields();
      fields.AddString("Type", _type.Name);
      LoggingChannel.LogEvent(string.Format(format, args), fields, LoggingLevel.Information);
    }

    public void Warn(string format, params object[] args)
    {
      var fields = new LoggingFields();
      fields.AddString("Type", _type.Name);
      LoggingChannel.LogEvent(string.Format(format, args), fields, LoggingLevel.Warning);
    }
  }
}
