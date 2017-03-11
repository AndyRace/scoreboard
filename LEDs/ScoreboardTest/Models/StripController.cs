using Caliburn.Micro;
using FadeCandy;
using System.Threading;
using System.Threading.Tasks;

namespace ScoreboardTest.Models
{
  class StripController : PropertyChangedBase, IStripController
  {
    private Controller _fadeCandy;
    private bool _running;

    public bool IsInitialised => _fadeCandy != null;

    public bool IsExecutingTest
    {
      get
      {
        return _running;
      }
      set
      {
        _running = value;
        NotifyOfPropertyChange(() => IsExecutingTest);
      }
    }

    public StripController()
    {
    }

    public async Task InitialiseAsync()
    {
      _fadeCandy = await Controller.CreateController();
      await _fadeCandy.InitialiseAsync();
      NotifyOfPropertyChange(() => IsInitialised);
    }

    public async Task ExecuteTestAsync(CancellationToken ct)
    {
      if (_fadeCandy == null) return;

      IsExecutingTest = true;
      try
      {
        await _fadeCandy.ExecuteTestAsync(ct);
      }
      finally
      {
        IsExecutingTest = false;
      }
    }
  }
}
