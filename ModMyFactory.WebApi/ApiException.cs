using System;
using System.Net;

namespace ModMyFactory.WebApi
{
    public class ApiException : Exception
    {
        public static ApiException FromWebException(WebException ex)
        {
            switch (ex.Status)
            {
                case WebExceptionStatus.ProtocolError:
                    switch (((HttpWebResponse)ex.Response).StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            return new ResourceNotFoundException(ex);
                        case HttpStatusCode.Unauthorized:
                            return new AuthenticationFailureException(ex);
                    }
                    break;
                case WebExceptionStatus.Timeout:
                    return new TimeoutException(ex);
                case WebExceptionStatus.ConnectFailure:
                    return new ConnectFailureException(ex);
            }
            return new ApiException("General API exception."); // No matching exception found
        }

        protected ApiException(string message, Exception innerException = null)
            : base(message, innerException)
        { }
    }
}
