using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Usb;
using Windows.Storage.Streams;

namespace FadeCandy
{
  public class FadeCandyUsbDevice
  {
    private static UsbDevice GetUsbDevice()
    {
      // DeviceInterfaceGUID: {62fd4123-87e3-47fe-946a-e044f36f2fb3}
      // HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\USB\VID_1D50&PID_607A&MI_00\7&34f36986&0&0000\Device Parameters
      // VendorId: 0x1D50
      // ProductId: 0x607A

      var logger = LogManager.GetLog(typeof(FadeCandyUsbDevice));

      const UInt32 vid = 0x1D50;
      const UInt32 pid = 0x607A;

      var aqs = UsbDevice.GetDeviceSelector(vid, pid);

      for (var attempt = 1; attempt <= 3; attempt++)
      {
        var waitDelay = TimeSpan.FromMilliseconds(attempt * 500);

        // TODO: Document this!
        // Using AsTask() => issues re-running
        // var findAllDevicesTask = DeviceInformation.FindAllAsync(aqs).AsTask();
        var findAllDevicesTask = Task.Run(async () => await DeviceInformation.FindAllAsync(aqs));
        if (findAllDevicesTask.Wait(waitDelay))
        {
          var myDevices = findAllDevicesTask.Result;

          // we expect 2 'devices' but we're only interested in the first
          if (myDevices != null && myDevices.Count == 2)
          {
            var deviceFromIdTask = Task.Run(async () => await UsbDevice.FromIdAsync(myDevices[0].Id));
            if (deviceFromIdTask.Wait(waitDelay) && (deviceFromIdTask.Result != null))
            {
              return deviceFromIdTask.Result;
            }
          }
        }
      }

      throw new UnableToFindFadeCandyException("Unable to find FadeCandy device!");
    }

    private static UsbDevice _usbDeviceSingleton;

    private object writeLock = new object();
    private CancellationTokenSource _cts = new CancellationTokenSource();
    private ILog Logger => LogManager.GetLog(GetType());

    public void WriteData(byte[] data)
    {
      // lock writing as some sequences of data cannot be intereaved with others
      lock (writeLock)
      {
        DoWriteData(data);
      }
    }

    public void WriteData(List<byte[]> packet)
    {
      // lock writing as some sequences of data cannot be intereaved with others
      lock (writeLock)
      {
        packet.ForEach((data) => DoWriteData(data));
      }
    }

    public void Reset()
    {
      if (_usbDeviceSingleton != null)
      {
        lock (writeLock)
        {
          if (_usbDeviceSingleton != null)
          {
            _usbDeviceSingleton.Dispose();
            _usbDeviceSingleton = null;
          }
        }
      }
    }

    private void DoWriteData(byte[] data, int attempts = 0)
    {
      try
      {
        if (_usbDeviceSingleton == null)
        {
          lock (writeLock)
          {
            if (_usbDeviceSingleton == null)
              _usbDeviceSingleton = GetUsbDevice();
          }
        }

        var writePipe = _usbDeviceSingleton.DefaultInterface.BulkOutPipes[0];

        // _writePipe.WriteOptions |= UsbWriteOptions.ShortPacketTerminate;

        var stream = writePipe.OutputStream;
        using (var writer = new DataWriter(stream))
        {
          writer.WriteBytes(data);

          writer.StoreAsync().AsTask().Wait(1000);
          // Not Implemented: writer.FlushAsync().AsTask().Wait();
        }

        //Task.Run(() =>
        //{
        //DebugPacket(data);
        //});
      }
      catch (Exception exception)
      {
        Reset();

        //Logger.Error(exception);

        throw new FadeCandyException("Unable to write to FadeCandy!", exception);
      }
      finally
      {
        //ShowStatus("Data written: " + bytesWritten + " bytes.");
        // Interlocked.Decrement(ref _count);
      }
    }

    private void DebugPacket(byte[] data)
    {
      // See: https://github.com/scanlime/fadecandy/blob/master/doc/fc_protocol_usb.md

      // Bits 7..6    Bit 5         Bits 4..0
      // Type code    'Final' bit   Packet index
      //
      // The 'type' code indicates what kind of packet this is.
      // The 'final' bit, if set, causes the most recent group of packets to take effect
      // The packet index is used to sequence packets within a particular type code
      var controlByte = data[0];
      var typeCode = (controlByte >> 6) & 0b11;
      var finalBit = ((controlByte >> 5) & 0b1) != 0;
      var packetIndex = (controlByte & 0b11111);

      // The following packet types are recognized:

      // Type code    Meaning of 'final' bit          Index range   Packet contents
      // ----------------------------------------------------------------------------------------------------
      // 0            Interpolate to new video frame  0 … 24        Up to 21 pixels, 24 - bit RGB
      // 1            Instantly apply new color LUT   0 … 24        Up to 31 16 - bit lookup table entries
      // 2            (reserved)                      0             Set configuration data
      // 3            (reserved)
      var info = "";
      switch (typeCode)
      {
        // Interpolate to new video frame
        // Up to 21 pixels, 24 - bit RGB
        case 0:
          for (var i = 1; i < data.Length; i += 3)
          {
            info += $"{data[i].ToString("x2")}{data[i + 1].ToString("x2")}{data[i + 2].ToString("x2")} ";
          }

          break;

        // Instantly apply new color LUT
        // 16 - bit lookup table entries
        // In a type 1 packet, the USB packet contains up to 31 lookup-table entries. The lookup table is structured as three arrays of 257 entries, starting with the entire red-channel LUT, then the green-channel LUT, then the blue-channel LUT. Each packet is structured as follows:

        // Byte Offset Description
        // 0            Control byte
        // 1            Reserved(0)
        // 2            LUT entry #0, low byte
        // 3            LUT entry #0, high byte
        // 4            LUT entry #1, low byte
        // 5            LUT entry #1, high byte
        // …            …
        // 62           LUT entry #30, low byte
        // 63           LUT entry #30, high byte
        case 1:
          for (var i = 2; i < data.Length; i += 2)
            info += $"{data[i].ToString("x2")}{data[i + 1].ToString("x2")} ";
          break;

        // Set configuration data
        //
        // Byte Offset  Bits    Description
        // 0            7 … 0   Control byte
        // 1            7 … 5   (reserved)
        // 1            4       0 = Normal mode, 1 = Reserved operation mode
        // 1            3       Manual LED control bit
        // 1            2       0 = LED shows USB activity, 1 = LED under manual control
        // 1            1       Disable keyframe interpolation
        // 1            0       Disable dithering
        // 2 … 63       7 … 0   (reserved)
        case 2:
          var config = data[1];
          info += $"Normal Mode={(config & 0b10000) == 0}, Manual LED={(config & 0b1000) != 0}, Show USB Activity={(config & 0b100) == 0}, Disable Interpolation={(config & 0b10) != 0}, Disable Dithering={(config & 0b1) != 0}";
          break;

        default:
          for (var i = 1; i < data.Length; i++)
            info += data[i].ToString("x2") + " ";
          break;
      }

      var typeInfo = (new string[] { "Video", "LUT", "Config", "Reserved" })[typeCode];
      Logger.Info($"WriteData [{data.Length}]: [type={typeInfo}, final={finalBit}, index={packetIndex} ({packetIndex * 21})]: {info}");
    }
  }
}