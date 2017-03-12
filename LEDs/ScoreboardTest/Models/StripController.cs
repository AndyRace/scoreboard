using Caliburn.Micro;
using FadeCandy;
using ScoreboardFadeCandy;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ScoreboardTest.Models
{
  class StripController : PropertyChangedBase, IStripController, IDisposable
  {
    private ScoreboardFadeCandyController _fadeCandy;
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

    private const string _groupName = "test";

    public async Task InitialiseAsync()
    {
      _fadeCandy = new ScoreboardFadeCandyController();

      await _fadeCandy.InitialiseAsync();

      var numbers = _fadeCandy.Numbers;
      var offset = 128;
      numbers.AddLedNumber(_groupName, ref offset, 1, 3);

      NotifyOfPropertyChange(() => IsInitialised);
    }

    public async Task ExecuteTestAsync(bool start)
    {
      if (_fadeCandy == null) return;

      IsExecutingTest = true;
      try
      {
        // TODO: Configure the offset!
        await _fadeCandy.ExecuteTestAsync(start);
      }
      finally
      {
        IsExecutingTest = false;
      }
    }

    public async Task SetValueAsync(string value)
    {
      await _fadeCandy.Numbers.SetValueAsync(_groupName, value);
    }

    public async Task Inc()
    {
      uint? value = await _fadeCandy.Numbers.GetValueAsync(_groupName);
      if (value == null)
      {
        value = 1;
      }
      else
      {
        value++;
      }
      await SetValueAsync(value.ToString());
    }

    public async Task Dec()
    {
      uint? value = await _fadeCandy.Numbers.GetValueAsync(_groupName);
      if (value == 0)
      {
        value = null;
      }
      else if (value != null)
      {
        value++;
      }

      await SetValueAsync(value.ToString());
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          if (_fadeCandy != null)
          {
            _fadeCandy.Dispose();
            _fadeCandy = null;
          }
        }

        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
        // TODO: set large fields to null.

        disposedValue = true;
      }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~StripController() {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
      // TODO: uncomment the following line if the finalizer is overridden above.
      // GC.SuppressFinalize(this);
    }
    #endregion
  }
}
