using System;
using System.Threading.Tasks;
using ScoreboardTest.Models;
using Windows.UI;
using Caliburn.Micro;
using FadeCandy;

namespace ScoreboardTest.ViewModels
{
  internal class SafeStripController : PropertyChangedBase, ISafeStripController
  {
    private IStripController _controller;

    public bool IsInitialised => GuardedCall(() => _controller.IsInitialised);

    public Color OnColour { get => GuardedCall(() => _controller.OnColour); set => GuardedCall(() => _controller.OnColour = value); }

    public SafeStripController(IStripController controller)
    {
      _controller = controller;
      _controller.PropertyChanged += (sender, e) => NotifyOfPropertyChange(e.PropertyName);
    }
    public void Dec()
    {
      Dec(false);
    }

    public void Dec(bool throwException)
    {
      GuardedCall(() => _controller.Dec(), throwException);
    }

    public Task ExecuteNumberTestAsync(bool start)
    {
      return GuardedCall(() => _controller.ExecuteNumberTestAsync(start));
    }

    public Task ExecuteTestAsync(bool start)
    {
      return GuardedCall(() => _controller.ExecuteTestAsync(start));
    }

    public string StringValue
    {
      get => GuardedCall(() => _controller.StringValue);
      set => GuardedCall(() => _controller.StringValue = value);
    }

    public void Inc()
    {
      Inc(false);
    }

    public void Inc(bool throwException)
    {
      GuardedCall(() => _controller.Inc(), throwException);
    }

    public void Reset()
    {
      GuardedCall(() => _controller.Reset());
    }

    private T2 GuardedCall<T2>(Func<T2> fn, bool throwException = true)
    {
      try
      {
        return fn();
      }
      catch (Exception ex)
      {
        if (ex is FadeCandyException)
          _controller.Reset();

        // LogManager.GetLog(GetType()).Error(ex);
        if (throwException) throw;

        return default(T2);
      }
    }

    private void GuardedCall(System.Action fn, bool throwException = true)
    {
      try
      {
        fn();
      }
      catch (Exception ex)
      {
        if (ex is FadeCandyException)
          _controller.Reset();

        // LogManager.GetLog(GetType()).Error(ex);
        if (throwException) throw;
      }
    }
  }
}