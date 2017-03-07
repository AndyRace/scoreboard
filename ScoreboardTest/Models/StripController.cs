using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace ScoreboardTest.Models
{
  class StripController : IStripController
  {
    public StripController()
    {
      //
    }

    public async Task ExecuteTestAsync()
    {
      await new MessageDialog("Executing test").ShowAsync();
    }
  }
}
