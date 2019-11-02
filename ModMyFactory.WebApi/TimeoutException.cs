using System;

namespace ModMyFactory.WebApi
{
    public class TimeoutException : ApiException
    {
        protected internal TimeoutException(Exception innerException = null)
            : base("A timeout occured when trying to connect to the server.", innerException)
        { }
    }
}
