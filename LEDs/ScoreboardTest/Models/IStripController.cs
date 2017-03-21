using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI;

namespace ScoreboardTest.Models
{
  public interface IStripController : INotifyPropertyChanged
  {
    bool IsInitialised { get; }

    Task ExecuteTestAsync(bool start);

    Task ExecuteNumberTestAsync(bool start);

    string StringValue { get; set; }

    void Dec();

    void Inc();

    Color OnColour { get; set; }

    void Reset();
  }
}