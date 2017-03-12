using Caliburn.Micro;
using ScoreboardTest.Models;
using ScoreboardTest.Utils;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Threading;
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
    }

    private void _controller_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      // todo: use e to determine which property changed
      NotifyOfPropertyChange(() => CanInitialise);
      NotifyOfPropertyChange(() => CanRun);
      NotifyOfPropertyChange(() => CanStop);
    }

    public async Task Initialise()
    {
      await _controller.InitialiseAsync();
    }

    public bool CanInitialise => !_controller.IsInitialised;

    public async Task Run()
    {
      await _controller.ExecuteTestAsync(true);
    }

    public bool CanRun => _controller.IsInitialised && !_controller.IsExecutingTest;

    public async Task StopAsync()
    {
      await _controller.ExecuteTestAsync(false);
    }

    public bool CanStop => _controller.IsExecutingTest;

    public string Info => "Test";

    public async Task ValueAsync(string value)
    {
      await _controller.SetValueAsync(value);
    }
    public bool CanValue => _controller.IsInitialised;

    public async Task Inc()
    {
      await _controller.Inc();
    }
    public bool CanInc => _controller.IsInitialised;

    public async Task Dec()
    {
      await _controller.Dec();
    }
    public bool CanDec => _controller.IsInitialised;
  }
}
