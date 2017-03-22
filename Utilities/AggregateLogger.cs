using Caliburn.Micro;
using System;
using System.Collections.Generic;

namespace ScoreboardTest.Utils
{
  public class AggregateLogger : ILog
  {
    private Type _type;

    // private static List<Type> _loggerClasses = new List<Type>();
    private static List<Func<Type, ILog>> _loggerCreators = new List<Func<Type, ILog>>();

    private List<ILog> _loggers = new List<ILog>();

    public AggregateLogger(Type type)
    {
      _type = type;

      // _loggerClasses.ForEach((loggerType) => _loggers.Add((ILog)Activator.CreateInstance(loggerType, type)));

      // todo: Add filtering
      _loggerCreators.ForEach((fn) => _loggers.Add(fn(type)));
    }

    public static Func<Type, ILog> AddLogger(Func<Type, ILog> createLogger)
    {
      _loggerCreators.Add(createLogger);

      return createLogger;
    }

    public static void RemoveLogger(Func<Type, ILog> createLogger)
    {
      _loggerCreators.Remove(createLogger);
    }

    private string CreateLogMessage(string format, params object[] args)
    {
      return string.Format($"[{DateTime.Now.ToString("o")}] {string.Format(format, args)}");
    }

    void ILog.Error(Exception exception)
    {
      // DebugInfo.Add($"ERROR: {CreateLogMessage(exception.ToString())}");
      _loggers.ForEach((logger) => logger.Error(exception));
    }

    void ILog.Info(string format, params object[] args)
    {
      // DebugInfo.Add($"INFO: {CreateLogMessage(format, args)}");
      if (_type == null || _type.Namespace != "Caliburn.Micro")
        _loggers.ForEach((logger) => logger.Info(format, args));
    }

    void ILog.Warn(string format, params object[] args)
    {
      //DebugInfo.Add($"WARN: CreateLogMessage(format, args)");
      _loggers.ForEach((logger) => logger.Warn(format, args));
    }
  }
}
