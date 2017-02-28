using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Storage;

namespace UsbTests.Models
{
  public class UsbDevices : ObservableCollection<IUsbDevice>, IUsbDevices
  {
    public UsbDevices() : base()
    {
    }

    public async Task GetDevices()
    {
      var devices = await DeviceInformation.FindAllAsync();
      this.ClearItems();
      devices.ToList().ForEach((info) => Add(new UsbDevice(info)));
    }
  }

  public class UsbDevice : ObservableObject, IUsbDevice
  {
    private string _name;
    private DeviceInformation _info;
    
    // todo: Remove this
    public UsbDevice(string name)
    {
      _name = name;
    }
    public UsbDevice(DeviceInformation info)
    {
      _info = info;
      _name = info.Name;
    }

    public string Name
    {
      get { return _name; }
    }
  }
}
