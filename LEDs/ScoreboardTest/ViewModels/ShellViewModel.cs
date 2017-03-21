using Caliburn.Micro;
using ScoreboardTest.Models;
using ScoreboardTest.Utils;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace ScoreboardTest.ViewModels
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
  public sealed class ShellViewModel : Screen, ILog
  {
    ILog _logger = LogManager.GetLog(typeof(ShellViewModel));

    IStripController _controller = new StripController();
    private Func<Type, ILog> _createLogger;

    public ObservableCollection<DebugItemViewModel> DebugInfo { get; private set; }

    public ShellViewModel()
    {
      DebugInfo = new ObservableCollection<DebugItemViewModel>();
      _controller.PropertyChanged += _controller_PropertyChanged;

      _createLogger = AggregateLogger.AddLogger((type) => this);

      if (Execute.InDesignMode)
        LoadDesignData();

      //_controller.Initialise();
    }

    private void LoadDesignData()
    {
      // NOT Working :(
      AddLogMessageAsync("Test", $"Some test data");
    }

    protected override void OnDeactivate(bool close)
    {
      if (close) AggregateLogger.RemoveLogger(_createLogger);

      base.OnDeactivate(close);
    }

    private void _controller_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      // todo: use e to determine which property changed
      NotifyOfPropertyChange(() => CanRunTest);
      NotifyOfPropertyChange(() => CanRunNumberTest);
      NotifyOfPropertyChange(() => CanInc);
      NotifyOfPropertyChange(() => CanDec);
      NotifyOfPropertyChange(() => IsValueEnabled);
      NotifyOfPropertyChange(() => Value);
    }

    private bool _runTest;
    public bool RunTest
    {
      get
      {
        return _runTest;
      }
      set
      {
        _runTest = value;
        Task.Run(async () =>
        {
          try
          {
            await _controller.ExecuteTestAsync(value);
          }
          catch
          {
            _runTest = false;
            NotifyOfPropertyChange(() => RunTest);
          }
        });

        NotifyOfPropertyChange(() => RunTest);
      }
    }

    public bool CanRunTest => true;// _controller.IsInitialised;

    private bool _runNumberTest;

    public bool RunNumberTest
    {
      get
      {
        return _runNumberTest;
      }
      set
      {
        Task.Run(async () =>
        {
          try
          {
            _runNumberTest = value;
            NotifyOfPropertyChange(() => RunNumberTest);
            await _controller.ExecuteNumberTestAsync(value);
          }
          catch
          {
            _runNumberTest = false;
            NotifyOfPropertyChange(() => RunNumberTest);
          }
        });
      }
    }

    public bool CanRunNumberTest => CanRunTest;

    public string Info => "Test";

    public string Value
    {
      get => _controller.GetStringValue();
      set
      {
        _controller.SetStringValue(value);
      }
    }

    public bool IsValueEnabled => true;//_controller.IsInitialised;

    public void Inc()
    {
      _controller.Inc();
    }
    public bool CanInc => true;//_controller.IsInitialised;

    public void Dec()
    {
      _controller.Dec();
    }
    public bool CanDec => true;//_controller.IsInitialised;

    public void ColourChanged(Windows.UI.Color color)
    {
      _controller.OnColour = color;
    }

    private void AddLogMessageAsync(string category, string format, params object[] args)
    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
      CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        () => DebugInfo.Insert(0, new DebugItemViewModel(DateTime.Now, category, string.Format(format, args))));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    void ILog.Info(string format, params object[] args)
    {
      AddLogMessageAsync("INFO", format, args);
    }

    void ILog.Warn(string format, params object[] args)
    {
      AddLogMessageAsync("WARN", format, args);
    }

    void ILog.Error(Exception exception)
    {
      AddLogMessageAsync("ERROR", exception.ToString());
    }
  }
}
