using FadeCandy;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScoreboardFadeCandy
{
  public class LedDigit
  {
    /*const*/
    private static byte[] _segmentBitmaps = new byte[] {
                // 0
                0b1110111,
                // 1
                0b0010001,
                // 2
                0b0101010,
                // 3
                0b0111011,
                // 4
                0b1011001,
                // 5
                0b1101011,
                // 6
                0b1101111,
                // 7
                0b0110001,
                // 8
                0b1111111,
                // 9
                0b1111001
                };

    private readonly Controller _controller;
    private readonly int _offset;
    private readonly int _ledsPerSegment;
    private readonly RGBColour _onColour, _offColour;

    public LedDigit(
      Controller fadeCandyController,
      int offset,
      byte ledsPerSegment,
      RGBColour? onColour = null,
      RGBColour? offColour = null)
    {
      _controller = fadeCandyController;
      _offset = offset;
      _ledsPerSegment = ledsPerSegment;
      _onColour = onColour ?? new RGBColour(1, 1, 1);
      _offColour = offColour ?? new RGBColour();
    }

    public async Task SetValue(byte? value)
    {
      if (value >= _segmentBitmaps.Length) throw new ArgumentOutOfRangeException("Value", value, $"Invalid value ({value}). Must be < {_segmentBitmaps.Length}.");

      var bmp = value.HasValue ? _segmentBitmaps[value.Value] : 0;

      // each segment
      for (int i = 7; i >= 0; i--)
      {
        for (var led = _offset; led < _ledsPerSegment; led++)
        {
          _controller.Pixels[led] = (bmp & 1) == 1 ? _onColour : _offColour;
        }
      }

      await _controller.FlushRangeAsync(_offset, 7 * _ledsPerSegment);
    }
  }

  public class LedNumber
  {
    private readonly Controller _controller;
    private readonly List<LedDigit> _digits;
    private readonly int _maxValue;
    private uint? _currentValue;

    public LedNumber(
      Controller fadeCandyController,
      List<LedDigit> digits)
    {
      _controller = fadeCandyController;
      _digits = digits;
      _maxValue = (int)Math.Pow(digits.Count, 10) - 1;
    }

    public async Task SetValueAsync(uint? value)
    {
      if (value > _maxValue) throw new ArgumentOutOfRangeException("Value", value, $"Invalid value ({value}). Must be < {_maxValue}.");

      for (var iDigit = _digits.Count; iDigit >= 0; iDigit--)
      {
        byte? digitValue = value.HasValue ? (byte?)((value.Value % 10) & 0xFF) : null;
        await _digits[iDigit].SetValue(digitValue);
        _currentValue = value;
      }
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task<uint?> GetValueAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
      return _currentValue;
    }
  }

  public class LedNumbers
  {
    private Dictionary<string, LedNumber> _numbers = new Dictionary<string, LedNumber>(StringComparer.CurrentCultureIgnoreCase);

    private Controller _fadeCandy;

    public LedNumbers(Controller fadeCandy)
    {
      _fadeCandy = fadeCandy ?? throw new ArgumentNullException("fadeCandy");
    }

    public async Task SetValueAsync(string key, string strValue)
    {
      uint? value = null;
      if (!string.IsNullOrWhiteSpace(strValue))
      {
        if (!uint.TryParse(strValue, out uint uValue))
        {
          throw new ArgumentOutOfRangeException("value", $"Invalid value ({strValue}) for number group {key}");
        }
        value = uValue;
      }

      await SetValueAsync(key, value);
    }

    public async Task SetValueAsync(string key, uint? value)
    {
      if (!_numbers.ContainsKey(key))
        throw new ArgumentOutOfRangeException("numberIndex", $"Unknown number group ({key})");

      await _numbers[key].SetValueAsync(value);
    }

    public async Task<uint?> GetValueAsync(string key)
    {
      if (!_numbers.ContainsKey(key))
        throw new ArgumentOutOfRangeException("numberIndex", $"Unknown number group ({key})");

      return await _numbers[key].GetValueAsync();
    }

    public void AddLedNumber(string key,
      ref int ledOffset,
      int nDigits = 0,
      byte ledsPerSegment = 0,
      RGBColour? onColour = null,
      RGBColour? offColour = null)
    {
      var digits = new List<LedDigit>();

      for (int i = 0; i < nDigits; i++)
        digits.Add(new LedDigit(_fadeCandy, ledOffset += 7 * ledsPerSegment, ledsPerSegment, onColour, offColour));

      _numbers.Add(key, new LedNumber(_fadeCandy, digits));
    }
  }
}