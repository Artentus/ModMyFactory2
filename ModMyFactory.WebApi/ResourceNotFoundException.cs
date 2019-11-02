using System;

namespace ModMyFactory.WebApi
{
    public class ResourceNotFoundException : ApiException
    {
        protected internal ResourceNotFoundException(Exception innerException = null)
            : base("Remote resource not found.", innerException)
        { }
    }
}
