using System;

namespace SelfHostedHttpServer
{
    // See: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes
    public class ResponseException : Exception
    {
        public readonly int ErrorCode;
        public readonly string ErrorCodeShortDescription;

        public ResponseException(string message, int errorCode, string errorCodeShortDescription) : base(message)
        {
            ErrorCode = errorCode;
            ErrorCodeShortDescription = errorCodeShortDescription;
        }
    }

    public class BadRequestException : ResponseException
    {
        //  404 Not Found
        //      The requested resource could not be found but may be available in the future.Subsequent requests by the client are permissible.
        public BadRequestException(string message) : base(message, 400, "Not Found")
        {
        }
    }

    public class InvalidFilenameException : ResponseException
    {
        //  404 Not Found
        //      The requested resource could not be found but may be available in the future.Subsequent requests by the client are permissible.
        public InvalidFilenameException(string message) : base(message, 404, "Not Found")
        {
        }
    }

    public class HttpVersionNotSupportedException : ResponseException
    {
        //  505 HTTP Version Not Supported
        //      The server does not support the HTTP protocol version used in the request.
        public HttpVersionNotSupportedException(string message) : base(message, 505, "HTTP Version Not Supported")
        {
        }
    }

    public class InvalidApiMethodException : ResponseException
    {
        //  405 Method Not Allowed
        //      A request method is not supported for the requested resource; for example, a GET request on a form which requires data to be presented via POST, or a PUT request on a read-only resource.
        public InvalidApiMethodException(string message) : base(message, 405, "Method Not Allowed")
        {
        }
    }

    public class InvalidRequestException : ResponseException
    {
        //  501 Not Implemented
        //      The server either does not recognize the request method, or it lacks the ability to fulfill the request.Usually this implies future availability (e.g., a new feature of a web-service API)
        public InvalidRequestException(string message) : base(message, 501, "Not Implemented")
        {
        }
    }

}