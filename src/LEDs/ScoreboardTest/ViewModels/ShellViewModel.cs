using Caliburn.Micro;
using ScoreboardFadeCandy;
using ScoreboardTest.Models;
using ScoreboardTest.Utils;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace ScoreboardTest.ViewModels
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
  public sealed class ShellViewModel : Screen, ILog
  {
    ISafeStripController _controller;
    private Func<Type, ILog> _createLogger;

    public ObservableCollection<DebugItemViewModel> DebugInfo { get; private set; }

    public ShellViewModel()
    {
      DebugInfo = new ObservableCollection<DebugItemViewModel>();

      _controller = new SafeStripController(new StripController());

      _controller.PropertyChanged += _controller_PropertyChanged;

      _createLogger = AggregateLogger.AddLogger((type) => this);

      // Setting this static to true ensures the Can<action> method is called before calling the action
      // regardless of the UI state
      // ActionMessage.EnforceGuardsDuringInvocation = true;
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
      NotifyOfPropertyChange(() => OnColour);
    }

    public void Reset()
    {
      _controller.Reset();
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
      get => _controller.StringValue;
      set { _controller.StringValue = value; }
    }

    public bool IsValueEnabled => true;//_controller.IsInitialised;

    public void ColourChanged(Windows.UI.Color color)
    {
      _controller.OnColour = color;
      NotifyOfPropertyChange(() => OnColour);
    }

    public string OnColour
    {
      get => _controller.OnColour.ToString();
    }

    // Repeat buttons will cotinually fire events if there's a delay during processing!
    class NoBounceRepeatButton
    {
      private const int MinRepeatButtonDelay = 1000;

      public bool CanExecute { get { return _canExecute != 0; } }
      private Stopwatch _lastError;
      private int _canExecute;

      public NoBounceRepeatButton(bool initialState)
      {
        _canExecute = initialState ? 1 : 0;
      }

      public void TryAction(System.Action action, System.Action done)
      {
        if (Interlocked.CompareExchange(ref _canExecute, 0, 1) == 1)
        {
          try
          {
            if (_lastError != null && _lastError.ElapsedMilliseconds < MinRepeatButtonDelay)
            {
              // LogManager.GetLog(GetType()).Info("Ignoring bouncing repeat button");
              return;
            }

            action();
            _lastError = null;
          }
          catch (Exception ex)
          {
            LogManager.GetLog(GetType()).Error(ex);
            _lastError = Stopwatch.StartNew();
          }
          finally
          {
            _canExecute = 1;
            done();
          }
        }
        else
        {
          // LogManager.GetLog(GetType()).Info("Ignoring re-entrant repeat button");
        }
      }
    }

    private NoBounceRepeatButton _decButtonAction = new NoBounceRepeatButton(true);

    public bool CanDec => true;// _decButtonAction.CanExecute; //_controller.IsInitialised;

    public void Dec()
    {
      _decButtonAction.TryAction(() => _controller.Dec(true), () => NotifyOfPropertyChange(nameof(CanDec)));
    }

    private NoBounceRepeatButton _incButtonAction = new NoBounceRepeatButton(true);

    public bool CanInc => true;// _incButtonAction.CanExecute; //_controller.IsInitialised;

    public void Inc()
    {
      _incButtonAction.TryAction(() => _controller.Inc(true), () => NotifyOfPropertyChange(nameof(CanInc)));
    }

    private void AddLogMessageAsync(string category, string format, params object[] args)
    {
      var debugItem = new DebugItemViewModel(DateTime.Now, category, string.Format(format, args));

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
      CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        () => DebugInfo.Insert(0, debugItem));
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
