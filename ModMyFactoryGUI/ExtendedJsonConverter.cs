//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;
using System;

namespace ModMyFactoryGUI
{
    internal interface IExtendedJsonConverter
    {
        object CreateFromToken(object token);

        bool CanConvertFrom(Type type);
    }

    // We need specialized JsonConverters to handle dynamic loading and saving of settings.
    // The standard ones do no suffice because we don't know the types at deserialization.
    internal abstract class ExtendedJsonConverter<T> : JsonConverter<T>, IExtendedJsonConverter
    {
        public abstract T CreateFromToken(object token);

        public abstract bool CanConvertFrom(Type type);

        object IExtendedJsonConverter.CreateFromToken(object token)
                    => CreateFromToken(token);
    }
}
