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

      // const int nLedsPerSegment = 2;
      // int offset = 128;
      // _numbers.AddLedNumber("total", ref offset, 1, nLedsPerSegment);
      // (new List<string> { "overs", "wickets", "firstInnings" }).ForEach((group) => _numbers.AddLedNumber(group, ref offset));


      // todo: Configuration!

      // while we're testing we have a 6 LED (2 pixel) and a 3 LED (1 pixel) number
      {
        int ledOffset = 128;

        var digits = new List<LedDigit>();

        int offset = ledOffset;
        digits.Add(new LedDigit(this, ref offset, 2));
        //digits.Add(new LedDigit(this, ref offset, 1));

        LedNumbers.GroupNumbers.Add("test", new LedNumber(this, digits));
      }
    }

    public LedNumbers LedNumbers
    {
      get
      {
        return _numbers;
      }
    }

    private class TestExecutor
    {
      private object ctsLock = new object();
      private CancellationTokenSource _cts;

      public async Task DoExecuteTestAsync(
        bool execute,
        Func<CancellationToken, Task> doExecuteTestAsync)
      {
        CancellationTokenSource cts;
        lock (ctsLock)
        {
          if (_cts != null)
            _cts.Cancel();

          if (execute)
            _cts = new CancellationTokenSource();

          cts = _cts;
        }

        if (execute)
          await doExecuteTestAsync(cts.Token);
      }
    }

    private TestExecutor _executeNumberTest = new TestExecutor();

    public async Task ExecuteNumberTestAsync(bool execute)
    {
      await _executeNumberTest.DoExecuteTestAsync(execute, async (ct) => await ExecuteNumberTestAsync(ct));
    }

    private async Task ExecuteNumberTestAsync(CancellationToken ct)
    {
      try
      {
        while (true)
        {
          for (byte? digitValue = null;
            (digitValue == null) || (digitValue < 10);
            digitValue = digitValue.HasValue ? (byte?)(digitValue + 1) : 0)
          {
            if (ct.IsCancellationRequested) break;

            foreach (var digit in LedNumbers.Digits)
              digit.SetValue(digitValue);

            await Task.Delay(TimeSpan.FromMilliseconds(500));
          }
        }
      }
      finally
      {
        foreach (var digit in LedNumbers.Digits)
        {
          digit.SetValue(null);
        }
      }
    }

    private TestExecutor _executeTest = new TestExecutor();

    public async Task ExecuteTestAsync(bool execute)
    {
      await _executeTest.DoExecuteTestAsync(execute, async (ct) => await ExecuteTestAsync(ct));
    }

    private async Task ExecuteTestForAllDigits(CancellationToken ct, int iterations, Func<CancellationToken, int, int, int, Task> executeTest)
    {
      var tasks = new List<Task>();

      for (var n = 0; n < iterations; n++)
      {
        foreach (var digit in _numbers.Digits)
        {
          var offset = digit.Offset;

          Debug.WriteLine($"Offset: {offset}");

          // tasks.Add(Task.Run(() => executeTest(ct, offset, offset + digit.NumPixels - 1, n)));
          await executeTest(ct, offset, offset + digit.NumPixels - 1, n);
        }

        // await Task.WhenAll(tasks.ToArray());
      }
    }

    private async Task ExecuteTestAsync(CancellationToken ct)
    {
      try
      {
        var fwd = true;
        double h = 0.0, s = 1, v = 1;

        while (!ct.IsCancellationRequested)
        {
          await ExecuteTestForAllDigits(ct, 10, async (cancellationToken, min, max, iteration) =>
            {
              for (var pixel = fwd ? min : max; pixel != (fwd ? max : min); pixel += (fwd ? 1 : -1))
              {
                if (cancellationToken.IsCancellationRequested) return;

                // https://en.wikipedia.org/wiki/HSL_and_HSV
                Pixels[pixel] = RGBColour.FromHSV(h, s, v);
                FlushRange(min, max - min + 1);

                await Task.Delay(TimeSpan.FromMilliseconds(30), cancellationToken);

                Pixels[pixel] = new RGBColour();
                h = (h + 0.1) % 1.00;
              }

              fwd = !fwd;
            });

          await ExecuteTestForAllDigits(ct, 10, async (cancellationToken, min, max, iteration) =>
          {
            fwd = true;

            h = 0;
            while (h >= 0 && h < 1.0)
            {
              if (ct.IsCancellationRequested) return;

              for (var pixel = min; pixel <= max; pixel++)
              {
                // https://en.wikipedia.org/wiki/HSL_and_HSV
                Pixels[pixel] = RGBColour.FromHSV(h, s, v);
              }

              FlushRange(min, max - min + 1);
              await Task.Delay(TimeSpan.FromMilliseconds(30), ct);

              h += fwd ? 0.02 : -0.02;
            }

            fwd = !fwd;
          });

          await ExecuteTestForAllDigits(ct, 10, async (cancellationToken, min, max, iteration) =>
          {
            var colours = new[] { new RGBColour(1, 0, 0), new RGBColour(0, 1, 0), new RGBColour(0, 0, 1) };
            foreach (var rgb in colours)
            {
              if (ct.IsCancellationRequested) return;

              for (var pixel = min; pixel <= max; pixel++)
              {
                Pixels[pixel] = rgb;
              }

              FlushRange(min,  max - min + 1);
              await Task.Delay(TimeSpan.FromMilliseconds(1000), ct);
            }
          });
        }
      }
      catch (OperationCanceledException)
      {
        // ignore this
      }
      finally
      {
        Clear();
      }
    }
  }
}
