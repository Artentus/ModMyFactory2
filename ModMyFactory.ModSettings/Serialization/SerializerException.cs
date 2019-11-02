using System;

namespace ModMyFactory.ModSettings.Serialization
{
    public class SerializerException : Exception
    {
        protected internal SerializerException(string message, Exception innerException = null)
            : base(message, innerException)
        { }
    }
}
