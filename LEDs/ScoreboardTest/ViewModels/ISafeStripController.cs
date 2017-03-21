using ScoreboardTest.Models;

namespace ScoreboardTest.ViewModels
{
  internal interface ISafeStripController: IStripController
  {
    void Inc(bool throwException);

    void Dec(bool throwException);
  }
}