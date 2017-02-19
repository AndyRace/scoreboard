using System;
using System.Net;

namespace SelfHostedHttpServer
{
    // See: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes
    public class ResponseException : Exception
    {
        public readonly uint ErrorCode;
        public readonly string ErrorCodeShortDescription;

        public ResponseException(string message, uint errorCode, string errorCodeShortDescription) : base(message)
        {
            ErrorCode = errorCode;
            ErrorCodeShortDescription = errorCodeShortDescription;
        }
    }

    public class HttpVersionNotSupportedException : ResponseException
    {
        public HttpVersionNotSupportedException(string message) : base(message, 505, "HTTP Version Not Supported")
        {
        }
    }

    public class InvalidApiMethodException : ResponseException
    {
        public InvalidApiMethodException(string message) : base(message, 400, "Bad Request")
        {
        }
    }

    public class InvalidRequestException : ResponseException
    {
        public InvalidRequestException(string message) : base(message, 400, "Bad Request")
        {
        }
    }

}