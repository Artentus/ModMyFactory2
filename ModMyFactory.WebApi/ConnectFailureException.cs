//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

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
