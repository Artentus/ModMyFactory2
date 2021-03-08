//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;

namespace ModMyFactory.WebApi
{
    public class AuthenticationFailureException : ApiException
    {
        protected internal AuthenticationFailureException(Exception? innerException = null)
            : base("Failed to authenticate on the server.", innerException)
        { }
    }
}
