using Caliburn.Micro;
using FadeCandy;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;

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

      const int nTrips = 1;
      int offset = 128;
      try
      {
        var fwd = true;
        double h = 0.0, s = 1, v = 1;
        while (true)
        {
          Debug.WriteLine($"Offset: {offset}");

          int min = 0 + offset;
          int max = 21 + offset;

          for (var trips = 0; trips < nTrips; trips++)
          {
            for (var pixel = fwd ? min : max; pixel != (fwd ? max : min); pixel += (fwd ? 1 : -1))
            {
              if (ct.IsCancellationRequested) return;

              // https://en.wikipedia.org/wiki/HSL_and_HSV
              _fadeCandy.Pixels[pixel] = RGBColour.FromHSV(h, s, v);
              await _fadeCandy.FlushAllAsync();

              await Task.Delay(TimeSpan.FromMilliseconds(30));

              _fadeCandy.Pixels[pixel] = new RGBColour();
              h = (h + 0.1) % 1.00;
            }

            fwd = !fwd;
          }


          for (var trips = 0; trips < nTrips; trips++)
          {
            fwd = true;

            h = 0;
            while (h >= 0 && h < 1.0)
            {
              if (ct.IsCancellationRequested) return;

              for (var pixel = min; pixel <= max; pixel++)
              {
                // https://en.wikipedia.org/wiki/HSL_and_HSV
                _fadeCandy.Pixels[pixel] = RGBColour.FromHSV(h, s, v);
              }

              await _fadeCandy.FlushAllAsync();

              await Task.Delay(TimeSpan.FromMilliseconds(30));

              h += fwd ? 0.02 : -0.02;
            }

            fwd = !fwd;
          }

          {
            var colours = new[] { new RGBColour(1, 0, 0), new RGBColour(0, 1, 0), new RGBColour(0, 0, 1) };
            foreach (var rgb in colours)
            {
              if (ct.IsCancellationRequested) return;

              for (var pixel = min; pixel <= max; pixel++)
              {
                _fadeCandy.Pixels[pixel] = rgb;
              }

              await _fadeCandy.FlushAllAsync();
              await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }
          }

          //offset = (offset + 64) % 512;
        }
      }
      finally
      {
        await _fadeCandy.ClearAsync();

        IsExecutingTest = false;
      }
    }
  }
}
