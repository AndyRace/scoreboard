using FadeCandy;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
    private Windows.UI.Color _onColour = Windows.UI.Colors.Yellow;

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
        var digits = new List<LedDigit>();
        // var onColor = RGBColour.FromBytes(0xFF, 0xFF, 0x00); // Yellow

        var onColor = RGBColour.FromColor(_onColour);

        // add digits least-significant first
        int offset = 0;
        digits.Add(new LedDigit(this, ref offset, 1, onColor));
        offset = 128;
        digits.Add(new LedDigit(this, ref offset, 2, onColor));

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

    private async Task ExecuteTestForAllDigits(CancellationToken ct, Func<CancellationToken, LedDigit, Task> executeTest)
    {
      var tasks = new List<Task>();

      foreach (var digit in _numbers.Digits)
      {
        tasks.Add(Task.Run(() => executeTest(ct, digit)));
      }

      await Task.WhenAll(tasks.ToArray());
    }

    private async Task ExecuteTestAsync(CancellationToken ct)
    {
      try
      {
        while (!ct.IsCancellationRequested)
        {
          await ExecuteTestForAllDigits(ct, async (cancellationToken, digit) =>
            {
              {
                var figureOf8 = new int[] { 0, 1, 2, 3, 4, 5, 6, 3 };

                double h = 0.0, s = 1, v = 1;

                for (int iteration = 0; iteration < 2; iteration++)
                {
                  foreach (var segment in figureOf8)
                  {
                    var firstPixel = digit.Offset + segment * digit.LedsPerSegment;
                    var lastPixel = digit.Offset + (segment + 1) * digit.LedsPerSegment;

                    for (var pixel = firstPixel; pixel != lastPixel; pixel++)
                    {
                      if (cancellationToken.IsCancellationRequested) return;

                      // https://en.wikipedia.org/wiki/HSL_and_HSV
                      var rgb = RGBColour.FromHSV(h, s, v);

                      Pixels[pixel] = rgb;

                      FlushAll();

                      // Debug.WriteLine($"Pixels[{pixel}]=r:{rgb.RByte.ToString("x2")} g:{rgb.GByte.ToString("x2")} b:{rgb.BByte.ToString("x2")} ({info})");

                      await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);

                      Pixels[pixel].Clear();

                      h = (h + 0.1) % 1.00;
                    }
                  }
                }
              }

              var min = digit.Offset;
              var max = digit.Offset + 7 * digit.LedsPerSegment - 1;

              {
                var fwd = true;
                double h = 0.0, s = 1, v = 1;

                for (int iteration = 0; iteration < 2; iteration++)
                {
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
                }
              }

              {
                var colours = new[] { new RGBColour(1, 0, 0), new RGBColour(0, 1, 0), new RGBColour(0, 0, 1) };
                for (int iteration = 0; iteration < 2; iteration++)
                {
                  foreach (var rgb in colours)
                  {
                    if (ct.IsCancellationRequested) return;

                    for (var pixel = min; pixel <= max; pixel++)
                    {
                      Pixels[pixel] = rgb;
                    }

                    FlushRange(min, max - min + 1);
                    await Task.Delay(TimeSpan.FromMilliseconds(1000), ct);
                  }
                }
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

    public Windows.UI.Color OnColour
    {
      get { return _onColour; }
      set
      {
        _onColour = value;
        foreach (var digit in LedNumbers.Digits)
        {
          digit.OnColour = RGBColour.FromColor(_onColour);
        }
      }
    }

    public override void Reset()
    {
      base.Reset();

      LedNumbers.Reset();
    }
  }
}
