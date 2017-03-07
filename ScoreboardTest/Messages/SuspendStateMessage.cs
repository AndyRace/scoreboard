using Windows.ApplicationModel;

namespace ScoreboardTest.Messages
{
  public class SuspendStateMessage
  {
    public SuspendStateMessage(SuspendingOperation operation)
    {
      Operation = operation;
    }

    public SuspendingOperation Operation { get; }
  }
}
