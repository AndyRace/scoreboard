using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace ScoreboardTest.Models
{
  public interface IStripController: INotifyPropertyChanged
  {
    bool IsInitialised { get; }

    bool IsExecutingTest { get; }

    Task InitialiseAsync();

    Task ExecuteTestAsync(bool start);

    Task SetValueAsync(string value);

    Task Dec();

    Task Inc();
  }
}