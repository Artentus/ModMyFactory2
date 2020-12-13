//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Net;

namespace ModMyFactory.WebApi
{
    public class ApiException : Exception
    {
        protected ApiException(string message, Exception? innerException = null)
            : base(message, innerException)
        { }

        public static ApiException FromWebException(WebException ex)
        {
            if (ex?.Response is HttpWebResponse response)
            {
                return (ex.Status, response.StatusCode) switch
                {
                    (WebExceptionStatus.ProtocolError, HttpStatusCode.NotFound) => new ResourceNotFoundException(ex),
                    (WebExceptionStatus.ProtocolError, HttpStatusCode.Unauthorized) => new AuthenticationFailureException(ex),
                    (WebExceptionStatus.Timeout, _) => new TimeoutException(ex),
                    (WebExceptionStatus.ConnectFailure, _) => new ConnectFailureException(ex),
                    (WebExceptionStatus.SecureChannelFailure, _) => new ConnectFailureException(ex),
                    _ => new ApiException("General API exception", ex) // No matching exception found
                };
            }
            else
            {
                return new ApiException("Unknown web exception", ex); // Exception is not related to the actual API
            }
        }
    }
}
