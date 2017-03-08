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

    Task ExecuteTestAsync(CancellationToken ct);
  }
}