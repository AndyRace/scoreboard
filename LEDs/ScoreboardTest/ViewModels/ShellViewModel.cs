using Caliburn.Micro;
using ScoreboardTest.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ScoreboardTest.ViewModels
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
  public class ShellViewModel : Screen
  {
    IStripController _controller = new StripController();

    public ObservableCollection<string> DebugInfo { get; private set; }

    public ShellViewModel()
    {
      DebugInfo = new ObservableCollection<string>();
      _controller.PropertyChanged += _controller_PropertyChanged;

      //_controller.Initialise();
    }

    private void _controller_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      // todo: use e to determine which property changed
      NotifyOfPropertyChange(() => CanInitialise);
      NotifyOfPropertyChange(() => CanRunTest);
      NotifyOfPropertyChange(() => CanRunNumberTest);
      NotifyOfPropertyChange(() => CanInc);
      NotifyOfPropertyChange(() => CanDec);
      NotifyOfPropertyChange(() => IsValueEnabled);
      NotifyOfPropertyChange(() => Value);
    }

    public void Initialise()
    {
      _controller.Initialise();
    }

    public bool CanInitialise => true;//!_controller.IsInitialised;


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
        Task.Run(async () => await _controller.ExecuteTestAsync(value));
        NotifyOfPropertyChange(() => RunTest);
      }
    }

    public bool CanRunTest => _controller.IsInitialised;

    private bool _runNumberTest;
    public bool RunNumberTest
    {
      get
      {
        return _runNumberTest;
      }
      set
      {
        _runNumberTest = value;
        Task.Run(async () => await _controller.ExecuteNumberTestAsync(value));
        NotifyOfPropertyChange(() => RunNumberTest);
      }
    }

    public bool CanRunNumberTest => CanRunTest;


    public string Info => "Test";

    public string Value
    {
      get => _controller.GetStringValue();
      set
      {
        try
        {
          _controller.SetStringValue(value);
        }
        catch (Exception ex)
        {
          Debug.WriteLine($"Error: {ex.Message}");
        }
      }
    }

    public bool IsValueEnabled => _controller.IsInitialised;

    public void Inc()
    {
      _controller.Inc();
    }
    public bool CanInc => _controller.IsInitialised;

    public void Dec()
    {
      _controller.Dec();
    }
    public bool CanDec => _controller.IsInitialised;
  }
}
