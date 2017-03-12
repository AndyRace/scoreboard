using FadeCandy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.System.Threading;

namespace ScoreboardFadeCandy
{
  public class ScoreboardFadeCandyController : Controller
  {
    //     B 2
    //      --
    // A 1 |  | C 3
    //      --  D 4
    // E 5 |  | G 7
    //      --
    //      F 6

    private LedNumbers _numbers;

    public ScoreboardFadeCandyController()
    {
      _numbers = new LedNumbers(this);

      const int nLedsPerSegment = 3;
      int offset = 128;
      _numbers.AddLedNumber("total", ref offset, 1, nLedsPerSegment);
      (new List<string> { "overs", "wickets", "firstInnings" }).ForEach((group) => _numbers.AddLedNumber(group, ref offset));
    }

    public LedNumbers Numbers
    {
      get
      {
        return _numbers;
      }
    }

    private async Task ExecuteTestAsync(CancellationToken ct, int offset, int numPixels)
    {
      const int nTrips = 1;

      try
      {
        var fwd = true;
        double h = 0.0, s = 1, v = 1;
        while (true)
        {
          Debug.WriteLine($"Offset: {offset}");

          int min = offset;
          int max = numPixels - 1 + offset;

          for (var trips = 0; trips < nTrips; trips++)
          {
            for (var pixel = fwd ? min : max; pixel != (fwd ? max : min); pixel += (fwd ? 1 : -1))
            {
              if (ct.IsCancellationRequested) return;

              // https://en.wikipedia.org/wiki/HSL_and_HSV
              this.Pixels[pixel] = RGBColour.FromHSV(h, s, v);
              await this.FlushAllAsync();

              await Task.Delay(TimeSpan.FromMilliseconds(30));

              this.Pixels[pixel] = new RGBColour();
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
                this.Pixels[pixel] = RGBColour.FromHSV(h, s, v);
              }

              await this.FlushAllAsync();

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
                this.Pixels[pixel] = rgb;
              }

              await this.FlushAllAsync();
              await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }
          }

          //offset = (offset + 64) % 512;
        }
      }
      catch (OperationCanceledException)
      {
        // ignore this
      }
      finally
      {
        await this.ClearAsync();
      }
    }

    private object _ctsLock;
    private CancellationTokenSource _cts;

    public async Task ExecuteTestAsync(bool start)
    {
      if (start)
      {
        CancellationToken token;
        lock (_ctsLock)
        {
          if (_cts != null) _cts.Cancel();
          _cts = new CancellationTokenSource();
          token = _cts.Token;
        }

        // TODO: Configure the offset!
        await ThreadPool.RunAsync(async workItem =>
            await ExecuteTestAsync(token, 128, 128));
      }
      else
      {
        lock (_ctsLock)
        {
          _cts.Cancel();
          _cts = null;
        }
      }
    }
  }
}
