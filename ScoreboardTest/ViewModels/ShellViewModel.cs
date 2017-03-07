using Caliburn.Micro;
using ScoreboardTest.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ScoreboardTest.ViewModels
{
  public class ShellViewModel : Screen
  {
    IStripController controller = new StripController();

    public ObservableCollection<string> DebugInfo { get; private set; }

    public ShellViewModel()
    {
      DebugInfo = new ObservableCollection<string>();
    }

    public async Task TestSequence()
    {
      //await controller.ExecuteTestAsync();

      DebugInfo.Add("Testing...");
    }

    public string Info
    {
      get { return "Ok"; }
    }
  }
}
