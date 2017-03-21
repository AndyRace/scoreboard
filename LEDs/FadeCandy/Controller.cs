using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Threading;

namespace FadeCandy
{
  // https://msdn.microsoft.com/en-us/library/windows/hardware/dn312121(v=vs.85).aspx
  public class Controller : PropertyChangedBase, IDisposable
  {
    private FadeCandyUsbDevice _fadeCandyUsbDevice = new FadeCandyUsbDevice();

    public RGBColour[] Pixels = new RGBColour[512];
    private bool _isInitialised;

    public bool IsInitialised
    {
      get => _isInitialised;
      set
      {
        _isInitialised = value;
        NotifyOfPropertyChange(() => IsInitialised);
      }
    }

    public void Clear()
    {
      foreach (var pixel in Pixels)
        pixel.Clear();

      // Yes we really do need to flush it twice
      FlushAll();
      FlushAll();
    }

    public void Dispose()
    {
      // TODO: It's a static so we can't Dispose of it
      //if (_usbDevice != null)
      //{
      //  _usbDevice.Dispose();
      //  _usbDevice = null;
      //}

      GC.SuppressFinalize(this);
    }

    public void FlushAll()
    {
      FlushRange(0, Pixels.Length);
    }

    public void FlushRange(int start, int count)
    {
      // Unfortunately the FadeCandy can't cope with flushing ranges
      // It seems to set the LEDs momentarily then set them to what was previously flushed!
      start = 0;
      count = Pixels.Length;

      if (start < 0 || start > 511) throw new ArgumentException("start");
      if (count < 0 || (start + count) > 512) throw new ArgumentException("count");
      const int pixelsPerChunk = 21;

      int firstChunk = (start / pixelsPerChunk);
      int lastChunk = ((start + count - 1) / pixelsPerChunk);

      var packet = new List<byte[]>();
      for (int chunk = firstChunk; chunk <= lastChunk; chunk++)
      {
        byte[] data = new byte[64];
        int offset = chunk * pixelsPerChunk;
        data[0] = ControlByte(0, chunk == lastChunk, chunk);
        for (int i = 0; i < pixelsPerChunk; i++)
        {
          if (i + offset > 511) continue;

          // todo: This seems odd that it's not RGB!
          data[1 + i * 3] = Pixels[i + offset].RByte;
          data[2 + i * 3] = Pixels[i + offset].BByte;
          data[3 + i * 3] = Pixels[i + offset].GByte;
        }

        packet.Add(data);
      }

      Initialise();
      _fadeCandyUsbDevice.WriteData(packet);
    }

    public void Reset()
    {
      IsInitialised = false;
      Initialise();
    }

    private int _inInitialise;

    private void Initialise()
    {
      if (IsInitialised) return;

      if (Interlocked.CompareExchange(ref _inInitialise, 1, 0) == 0)
      {
        _inInitialise = 1;
        try
        {
          _fadeCandyUsbDevice.Reset();

          SendConfiguration();

          double gammaCorrection = 1.6;
          // compute basic uniform gamma table for r/g/b

          const int lutEntries = 257;
          const int lutTotalEntries = lutEntries * 3;
          UInt16[] lutValues = new UInt16[lutTotalEntries];

          for (int i = 0; i < lutEntries; i++)
          {
            double r, g, b;
            r = g = b = Math.Pow((double)i / (lutEntries - 1), gammaCorrection) * 65535;
            lutValues[i] = (UInt16)r;
            lutValues[i + lutEntries] = (UInt16)g;
            lutValues[i + lutEntries * 2] = (UInt16)b;
          }

          // Send LUT 31 entries at a time.
          var packet = new List<byte[]>();

          int blockIndex = 0;
          int lutIndex = 0;
          while (lutIndex < lutTotalEntries)
          {
            byte[] data = new byte[64];

            bool lastChunk = false;
            int lutCount = (lutTotalEntries - lutIndex);
            if (lutCount > 31)
              lutCount = 31;

            int nextIndex = lutIndex + lutCount;
            if (nextIndex == lutTotalEntries)
              lastChunk = true;

            data[0] = ControlByte(1, lastChunk, blockIndex);

            for (int i = 0; i < lutCount; i++)
            {
              data[i * 2 + 2] = (byte)(lutValues[lutIndex + i] & 0xFF);
              data[i * 2 + 3] = (byte)((lutValues[lutIndex + i] >> 8) & 0xFF);
            }

            blockIndex++;
            lutIndex = nextIndex;

            packet.Add(data);
          }

          _fadeCandyUsbDevice.WriteData(packet);

          // clear any lit LEDs
          Clear();

          IsInitialised = true;
        }
        finally
        {
          _inInitialise = 0;
        }
      }
    }

    private static byte ControlByte(int type, bool final = false, int index = 0)
    {
      if (type < 0 || type > 3) throw new ArgumentException("type");
      if (index < 0 || index > 31) throw new ArgumentException("index");
      byte output = (byte)((type << 6) | index);
      if (final) output |= 0x20;
      return output;
    }

    private void SendConfiguration(bool enableDithering = true, bool enableKeyframeInterpolation = true, bool manualLedControl = false, bool ledValue = false, bool reservedMode = false)
    {
      byte[] data = new byte[64];

      data[0] = ControlByte(2);

      if (!enableDithering)
        data[1] |= 0x01;

      if (!enableKeyframeInterpolation)
        data[1] |= 0x02;

      if (manualLedControl)
        data[1] |= 0x04;

      if (ledValue)
        data[1] |= 0x08;

      if (reservedMode)
        data[1] |= 0x10;

      Initialise();
      _fadeCandyUsbDevice.WriteData(data);
    }
  }
}
