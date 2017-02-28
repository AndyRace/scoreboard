using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsbTests.Models
{
  public interface IUsbDevice
  {
    string Name { get; }
  }

  // public class ObservableCollection<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
  public interface IUsbDevices : ICollection<IUsbDevice>
  {
    Task GetDevices();
  }
}
