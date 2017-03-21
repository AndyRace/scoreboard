using Caliburn.Micro;
using ScoreboardFadeCandy;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ScoreboardTest.Models
{
  class StripController : PropertyChangedBase, IStripController, IDisposable
  {
    private ScoreboardFadeCandyController _fadeCandy = new ScoreboardFadeCandyController();

    // This is the group used for 'strings' and inc/dec
    private string _groupName;

    public StripController()
    {
      _groupName = _fadeCandy.LedNumbers.GroupNumbers.First().Key;

      _fadeCandy.PropertyChanged += _fadeCandy_PropertyChanged;
    }

    public bool IsInitialised => _fadeCandy.IsInitialised;

    private void _fadeCandy_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "IsInitialised")
        NotifyOfPropertyChange("IsInitialised");
    }

    public async Task ExecuteTestAsync(bool execute)
    {
      await _fadeCandy.ExecuteTestAsync(execute);
    }

    public async Task ExecuteNumberTestAsync(bool execute)
    {
      await _fadeCandy.ExecuteNumberTestAsync(execute);
    }

    public void SetStringValue(string value)
    {
      _fadeCandy.LedNumbers.SetStringValue(_groupName, value);
      NotifyOfPropertyChange("Value");
    }

    public string GetStringValue()
    {
      return _fadeCandy.LedNumbers.GetStringValue(_groupName);
    }

    public void Inc()
    {
      uint? value = _fadeCandy.LedNumbers.GetValue(_groupName);
      if (value == null)
      {
        value = 1;
      }
      else
      {
        value++;
      }

      SetStringValue(value.ToString());
    }

    public void Dec()
    {
      uint? value = _fadeCandy.LedNumbers.GetValue(_groupName);
      if (value == 0)
      {
        value = null;
      }
      else if (value != null)
      {
        value--;
      }

      SetStringValue(value.ToString());
    }

    public Windows.UI.Color OnColour { get { return _fadeCandy.OnColour; } set { _fadeCandy.OnColour = value; } }

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
