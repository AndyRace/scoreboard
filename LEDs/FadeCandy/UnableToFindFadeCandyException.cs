using System;

namespace FadeCandy
{
  internal class UnableToFindFadeCandyException : FadeCandyException
  {
    public UnableToFindFadeCandyException()
    {
    }

    public UnableToFindFadeCandyException(string message) : base(message)
    {
    }

    public UnableToFindFadeCandyException(string message, Exception innerException) : base(message, innerException)
    {
    }
  }
}