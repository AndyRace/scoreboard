using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace ScoreboardTest.Models
{
  public interface IStripController: INotifyPropertyChanged
  {
    bool IsInitialised { get; }

    void Initialise();

    Task ExecuteTestAsync(bool start);
    Task ExecuteNumberTestAsync(bool start);

    void SetStringValue(string value);

    string GetStringValue();

    void Dec();

    void Inc();
  }
}