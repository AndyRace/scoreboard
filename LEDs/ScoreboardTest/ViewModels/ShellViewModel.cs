using Caliburn.Micro;
using ScoreboardTest.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace ScoreboardTest.ViewModels
{
  public class ShellViewModel : Screen, IDisposable
  {

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

    IStripController _controller = new StripController();

    CancellationTokenSource _cts;

    public async Task Initialise()
    {
      await _controller.InitialiseAsync();
    }

    public bool CanInitialise => !_controller.IsInitialised;

    public async Task Run()
    {
      _cts = new CancellationTokenSource();
      await _controller.ExecuteTestAsync(_cts.Token);
    }

    public bool CanRun => _controller.IsInitialised && !_controller.IsExecutingTest;

    public void Stop()
    {
      if (_cts != null)
      {
        _cts.Cancel();
      }
    }

    public bool CanStop => _controller.IsExecutingTest;

    public string Info => "Test";

    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        // dispose managed resources
        if (_cts != null)
        {
          _cts.Dispose();
          _cts = null;
        }
      }
      // free native resources
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
  }
}
