using System;

namespace RPlay.Games
{
    public sealed class RPlayApiException : Exception
    {
        public RPlayApiException(
            string message,
            string errorCode = null,
            long httpStatusCode = 0,
            string responseBody = null,
            Exception innerException = null
        )
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            HttpStatusCode = httpStatusCode;
            ResponseBody = responseBody;
        }

        public string ErrorCode { get; }

        public long HttpStatusCode { get; }

        public string ResponseBody { get; }
    }
}
