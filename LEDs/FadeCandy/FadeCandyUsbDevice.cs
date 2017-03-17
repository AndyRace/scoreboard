using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Usb;
using Windows.Storage.Streams;

namespace FadeCandy
{
  public class FadeCandyUsbDevice
  {
    private static FadeCandyUsbDevice _singleton;
    private static object _singletonLock = new object();

    public static FadeCandyUsbDevice Singleton
    {
      get
      {
        if (_singleton == null)
        {
          lock (_singletonLock)
          {
            if (_singleton == null)
            {
              _singleton = GetDevice();
            }
          }
        }

        return _singleton;
      }
    }

    private static FadeCandyUsbDevice GetDevice()
    {
      // DeviceInterfaceGUID: {62fd4123-87e3-47fe-946a-e044f36f2fb3}
      // HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\USB\VID_1D50&PID_607A&MI_00\7&34f36986&0&0000\Device Parameters
      // VendorId: 0x1D50
      // ProductId: 0x607A

      UInt32 vid = 0x1D50;
      UInt32 pid = 0x607A;

      string aqs = UsbDevice.GetDeviceSelector(vid, pid);


      var findAllDevicesTask = Task.Run(async () => await DeviceInformation.FindAllAsync(aqs, null));
      findAllDevicesTask.Wait();
      DeviceInformationCollection myDevices = findAllDevicesTask.Result;

      // we expect 2 'devices' but we're only interested in the first
      if (myDevices == null || myDevices.Count != 2)
      {
        throw new Exception("Unable to find FadeCandy device!");
        //return null;
      }

      try
      {
        Debug.WriteLine($"Found FadeCandy device");

        var deviceFromIdTask = Task.Run(async () => await UsbDevice.FromIdAsync(myDevices[0].Id));
        deviceFromIdTask.Wait();
        if (deviceFromIdTask.Result == null) throw new Exception("Unable to access device!");
        return new FadeCandyUsbDevice(deviceFromIdTask.Result);
      }
      catch (Exception)
      {
        // Debug.WriteLine(ex.Message, "Error");

        throw;
      }
    }

    private UsbDevice _usbDevice;
    private UsbBulkOutPipe _writePipe;
    private IOutputStream _stream;
    private DataWriter _writer;
    private ConcurrentQueue<byte[]> _dataQueue = new ConcurrentQueue<byte[]>();
    private CancellationTokenSource _cts = new CancellationTokenSource();

    private FadeCandyUsbDevice(UsbDevice usbDevice)
    {
      _usbDevice = usbDevice;

      _writePipe = _usbDevice.DefaultInterface.BulkOutPipes[0];

      _writePipe.WriteOptions |= UsbWriteOptions.ShortPacketTerminate;

      _stream = _writePipe.OutputStream;

      _writer = new DataWriter(_stream);

      Task.Factory.StartNew(() =>
      {
        while (!_cts.Token.IsCancellationRequested)
        {
          // todo: delay?
          //if (_deviceQueue.TryDequeue(out Action<FadeCandyUsbDevice> action))
          //{
          //  action(_singleton);
          //}

          if (_dataQueue.TryDequeue(out byte[] data))
          {
            WriteData(this, data);
          }

          Task.Yield();
        }
      }, TaskCreationOptions.LongRunning);
    }

    public void WriteDataBackground(byte[] data)
    {
      Singleton._dataQueue.Enqueue(data);
    }

    private int _count;

    private void WriteData(FadeCandyUsbDevice fadeCandyUsbDevice, byte[] data)
    {
      // todo: Is this the right approach?
      // allow callers to call this with await even though we're no longer really async
      // await Task.FromResult(0);

      // Debug.WriteLine("WriteDataAsync: START");

      try
      {
        var count = Interlocked.Increment(ref _count);

        // bytesWritten = await writer.StoreAsync();
        //var storeTask = writer.StoreAsync();

        //bytesWritten = await storeTask.GetResults();

        var storeAsyncTask = Task.Run(async () =>
        {
          fadeCandyUsbDevice._writer.WriteBytes(data);

          var result = await fadeCandyUsbDevice._writer.StoreAsync();
          // await fadeCandyUsbDevice._writer.FlushAsync();
          return result;
        });

        // DO NOT WAIT FOR THE STORE TO COMPLETE!
        // For some reason this causes many issues
        //    Wrong colour
        //    Incorrect on/off state
        //storeAsyncTask.Wait();
        //var bytesWritten = storeAsyncTask.Result;

        //var info = "";
        //for (var i = 0; i < data.Length; i++)
        //  info += data[i].ToString("x2") + " ";
        //Task.Run(() =>
        //{
        //  Debug.WriteLine($"{count}: WriteData [{data.Length}, {bytesWritten}]: {info}");
        //});
      }
      catch (Exception exception)
      {
        Debug.WriteLine(exception.Message.ToString(), "Exception");
        throw;
      }
      finally
      {
        //ShowStatus("Data written: " + bytesWritten + " bytes.");
        Interlocked.Decrement(ref _count);
      }
    }
  }
}