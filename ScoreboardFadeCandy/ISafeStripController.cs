namespace ScoreboardFadeCandy
{
  public interface ISafeStripController: IStripController
  {
    void Inc(bool throwException);

    void Dec(bool throwException);
  }
}