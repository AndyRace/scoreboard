using FadeCandy;
using System;
using System.Collections.Generic;

namespace ScoreboardFadeCandy
{
  public class LedDigit
  {
    private static RGBColour DefaultOnColour = RGBColour.FromHSV(0.8, 1, 1); //new RGBColour(1, 1, 1);

    //     B 2
    //      --
    // A 1 |  | C 3
    //      --  D 4
    // E 5 |  | G 7
    //      --
    //      F 6

    /*const*/
    private static byte[] _segmentBitmaps = new byte[] {
                // 0
                0b1110111,
                // 1
                0b0010001,
                // 2
                0b0111110,
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

    private byte? _currentValue;

    public int Offset { get; private set; }

    public int LedsPerSegment { get; private set; }

    public int NumPixels { get { return LedsPerSegment * 7; } }

    private RGBColour _onColour;
    public RGBColour OnColour
    {
      get { return _onColour; }
      internal set
      {
        _onColour = value;
        SetValue(_currentValue);
      }
    }

    private readonly RGBColour _offColour;

    public LedDigit(
      Controller fadeCandyController,
      ref int offset,
      byte ledsPerSegment,
      RGBColour? onColour = null,
      RGBColour? offColour = null)
    {
      _controller = fadeCandyController;
      Offset = offset;
      offset += 7 * ledsPerSegment;
      LedsPerSegment = ledsPerSegment;
      _onColour = onColour ?? DefaultOnColour;
      _offColour = offColour ?? new RGBColour();
    }

    public void SetValue(byte? value)
    {
      if (value >= _segmentBitmaps.Length) throw new ArgumentOutOfRangeException("Value", value, $"Invalid value ({value}). Must be <= {_segmentBitmaps.Length}.");

      var bmp = value.HasValue ? _segmentBitmaps[value.Value] : 0;

      var offset = Offset;

      // each segment
      for (int i = 0; i < 7; i++)
      {
        for (var led = 0; led < LedsPerSegment; led++)
        {
          _controller.Pixels[offset++] = (bmp & 0b1000000) != 0 ? OnColour : _offColour;
        }

        bmp <<= 1;
      }

      _controller.FlushRange(Offset, 7 * LedsPerSegment);

      _currentValue = value;
    }

    public byte? GetValue()
    {
      return _currentValue;
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
      _maxValue = (int)Math.Pow(10, digits.Count) - 1;
    }

    //digits are stored least-significant first
    public List<LedDigit> Digits { get => _digits; }

    public void SetValue(uint? value)
    {
      if (value > _maxValue) throw new ArgumentOutOfRangeException("Value", value, $"Invalid value ({value}). Must be <= {_maxValue}.");

      _currentValue = value;

      for (var iDigit = 0; iDigit < _digits.Count; iDigit++)
      {
        byte? digitValue = value.HasValue ? (byte?)(value.Value % 10) : null;

        _digits[iDigit].SetValue(digitValue);

        if (value.HasValue) value = value / 10;

        if (value == 0) value = null;
      }
    }

    public uint? GetValue()
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

    public Dictionary<string, LedNumber> GroupNumbers { get => _numbers; }

    public void SetStringValue(string key, string strValue)
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

      SetValue(key, value);
    }

    public void SetValue(string key, uint? value)
    {
      if (!_numbers.ContainsKey(key))
        throw new ArgumentOutOfRangeException("numberIndex", $"Unknown number group ({key})");

      _numbers[key].SetValue(value);
    }

    public string GetStringValue(string key)
    {
      if (!_numbers.ContainsKey(key))
        throw new ArgumentOutOfRangeException("numberIndex", $"Unknown number group ({key})");

      var value = _numbers[key].GetValue();
      if (value.HasValue) return value.Value.ToString();
      return null;
    }

    public uint? GetValue(string key)
    {
      if (!_numbers.ContainsKey(key))
        throw new ArgumentOutOfRangeException("numberIndex", $"Unknown number group ({key})");

      return _numbers[key].GetValue();
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
      {
        digits.Add(new LedDigit(_fadeCandy, ref ledOffset, ledsPerSegment, onColour, offColour));
      }

      _numbers.Add(key, new LedNumber(_fadeCandy, digits));
    }

    /// <summary>
    /// Return all 'LedDigit's in all numbers
    /// </summary>
    /// <returns>An LedDigit</returns>
    public IEnumerable<LedDigit> Digits
    {
      get
      {
        foreach (var number in _numbers)
        {
          foreach (var digit in number.Value.Digits)
          {
            yield return digit;
          }
        }
      }
    }

    public void Reset()
    {
      foreach(var digit in Digits)
      {
        digit.SetValue(digit.GetValue());
      }
    }
  }
}