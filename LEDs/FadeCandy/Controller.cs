using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Usb;
using Windows.Storage.Streams;

namespace FadeCandy
{
  // https://msdn.microsoft.com/en-us/library/windows/hardware/dn312121(v=vs.85).aspx
  public class Controller : IDisposable
  {
    public RGBColour[] Pixels = new RGBColour[512];
    private UsbDevice _usbDevice;

    protected static async Task<UsbDevice> GetDevice()
    {
      // DeviceInterfaceGUID: {62fd4123-87e3-47fe-946a-e044f36f2fb3}
      // HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\USB\VID_1D50&PID_607A&MI_00\7&34f36986&0&0000\Device Parameters
      // VendorId: 0x1D50
      // ProductId: 0x607A

      UInt32 vid = 0x1D50;
      UInt32 pid = 0x607A;

      string aqs = UsbDevice.GetDeviceSelector(vid, pid);

      var myDevices = await DeviceInformation.FindAllAsync(aqs, null);

      // we expect 2 'devices' but we're only interested in the first
      if (myDevices.Count != 2)
      {
        throw new Exception("Unable to find FadeCandy device!");
        //return null;
      }

      try
      {
        Debug.WriteLine($"Found FadeCandy device");

        return await UsbDevice.FromIdAsync(myDevices[0].Id);
      }
      catch (Exception)
      {
        // Debug.WriteLine(ex.Message, "Error");

        throw;
      }
    }

    public Controller()
    {
    }

    public async Task ClearAsync()
    {
      var unset = new RGBColour(0, 0, 0);

      for (var pixel = 0; pixel < Pixels.Length; pixel++)
      {
        Pixels[pixel] = unset;
      }

      await FlushAllAsync();
    }

    private async Task WriteDataAsync(byte[] data)
    {
      try
      {
        // todo: Is this the right approach
        // allow callers to call this with await even though we're no longer really async
        await Task.FromResult(0);

        lock (_usbDevice)
        {
          UInt32 bytesWritten = 0;

          UsbBulkOutPipe writePipe = _usbDevice.DefaultInterface.BulkOutPipes[0];
          writePipe.WriteOptions |= UsbWriteOptions.ShortPacketTerminate;

          var stream = writePipe.OutputStream;

          DataWriter writer = new DataWriter(stream);

          writer.WriteBytes(data);

          // bytesWritten = await writer.StoreAsync();
          bytesWritten = /*await*/ writer.StoreAsync().GetResults();
        }
      }
      catch (Exception exception)
      {
        Debug.WriteLine(exception.Message.ToString(), "Exception");
      }
      finally
      {
        //ShowStatus("Data written: " + bytesWritten + " bytes.");
      }
    }

    public void Dispose()
    {
      if (_usbDevice != null)
      {
        _usbDevice.Dispose();
        _usbDevice = null;
      }

      GC.SuppressFinalize(this);
    }

    public async Task FlushAllAsync()
    {
      await FlushRangeAsync(0, Pixels.Length);
    }

    public async Task FlushRangeAsync(int start, int count)
    {
      if (start < 0 || start > 511) throw new ArgumentException("start");
      if (count < 0 || (start + count) > 512) throw new ArgumentException("count");
      const int pixelsPerChunk = 21;

      int firstChunk = (start / pixelsPerChunk);
      int lastChunk = ((start + count - 1) / pixelsPerChunk);

      byte[] data = new byte[64];
      for (int chunk = firstChunk; chunk <= lastChunk; chunk++)
      {
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

        await WriteDataAsync(data);
      }
    }

    public async Task InitialiseAsync()
    {
      if (_usbDevice == null)
      {
        _usbDevice = await GetDevice();
      }

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
      byte[] data = new byte[64];

      int blockIndex = 0;
      int lutIndex = 0;
      while (lutIndex < lutTotalEntries)
      {
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

        await WriteDataAsync(data);

        blockIndex++;
        lutIndex = nextIndex;
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

    public async Task SendConfigurationAsync(bool enableDithering = true, bool enableKeyframeInterpolation = true, bool manualLedControl = false, bool ledValue = false, bool reservedMode = false)
    {
      byte[] data = new byte[64];

      data[0] = ControlByte(2);

      if (!enableDithering)
        data[1] |= 0x01;

      if (enableKeyframeInterpolation)
        data[1] |= 0x02;

      if (manualLedControl)
        data[1] |= 0x04;

      if (ledValue)
        data[1] |= 0x08;

      if (reservedMode)
        data[1] |= 0x10;


      await WriteDataAsync(data);
    }
  }
}
