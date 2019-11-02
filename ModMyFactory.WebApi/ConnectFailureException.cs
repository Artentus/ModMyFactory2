using System;

namespace ModMyFactory.WebApi
{
    public class ConnectFailureException : ApiException
    {
        protected internal ConnectFailureException(Exception innerException = null)
            : base("Failed to connect to the server.", innerException)
        { }
    }
}
