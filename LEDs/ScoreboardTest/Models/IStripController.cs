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

    void SetStringValue(string value);

    string GetStringValue();

    void Dec();

    void Inc();

    Color OnColour { get; set; }
  }
}