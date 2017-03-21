using System;

namespace FadeCandy
{
  internal class FadeCandyException : Exception
  {
    public FadeCandyException()
    {
    }

    public FadeCandyException(string message) : base(message)
    {
    }

    public FadeCandyException(string message, Exception innerException) : base(message, innerException)
    {
    }
  }
}