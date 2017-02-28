using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsbTests.ViewModel;

namespace UsbTests
{
  public class ViewModelLocator
  {
    public ViewModelLocator()
    {
      ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

      SimpleIoc.Default.Register<MainViewModel>();
    }

    public MainViewModel MainViewModel
    {
      get
      {
        return ServiceLocator.Current.GetInstance<MainViewModel>();
      }
    }
  }
}
