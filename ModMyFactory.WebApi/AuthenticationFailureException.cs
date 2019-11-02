using System;

namespace ModMyFactory.WebApi
{
    public class AuthenticationFailureException : ApiException
    {
        protected internal AuthenticationFailureException(Exception innerException = null)
            : base("Failed to authenticate on the server.", innerException)
        { }
    }
}
