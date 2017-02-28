using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsbTests.Models
{
  public class TestUsbDevices : ObservableCollection<TestUsbDevice>
  {
    public TestUsbDevices() : base()
    {
      Add(new TestUsbDevice("Willa", "Cather"));
    }

    public async Task GetDevices()
    {
      Add(new TestUsbDevice("Isak", "Dinesen"));
      Add(new TestUsbDevice("Victor", "Hugo"));
      Add(new TestUsbDevice("Jules", "Verne"));

      await Task.Yield();
    }
  }

  public class TestUsbDevice
  {
    private string name;
    private string lastName;

    public TestUsbDevice(string first, string last)
    {
      this.name = first;
      this.lastName = last;
    }

    public string Name
    {
      get { return name; }
      set { name = value; }
    }

    public string LastName
    {
      get { return lastName; }
      set { lastName = value; }
    }
  }
}
