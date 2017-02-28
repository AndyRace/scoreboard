using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsbTests.Models;

namespace UsbTests.ViewModel
{
  public class MainViewModel : ViewModelBase
  {
    public MainViewModel()
    {
      ConnectCommand = new RelayCommand(DoConnect, ConnectCanExecute);
      GetDevicesCommand = new RelayCommand(async () => await DoGetDevicesAsync());
    }

    private bool ConnectCanExecute()
    {
      return SelectedDevice != null;
    }

    private async Task DoGetDevicesAsync()
    {
      await _devices.GetDevices();
      this.RaisePropertyChanged(() => this.Devices);
    }

    private readonly UsbDevices _devices = new UsbDevices();
    private IUsbDevice _selectedDevice;

    public IUsbDevices Devices { get { return _devices; } }

    public IUsbDevice SelectedDevice
    {
      get { return _selectedDevice; }
      set
      {
        _selectedDevice = value;
        RaisePropertyChanged();

        // we know that selecting a device changes the ConnectCommand's can-execute state
        // so raise it here
        ConnectCommand.RaiseCanExecuteChanged();
      }
    }

    public RelayCommand ConnectCommand { get; private set; }
    public RelayCommand GetDevicesCommand { get; private set; }

    public async void DoConnect()
    {
      throw new NotImplementedException();
    }
  }
}
