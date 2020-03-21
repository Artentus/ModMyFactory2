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
    internal abstract class StringJsonConverter<T> : ExtendedJsonConverter<T>
    {
        protected abstract T Create(string token);

        protected abstract string Tokenize(T value);

        public override T CreateFromToken(object token)
            => Create((string)token);

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string s = (string)reader.Value;
            return Create(s);
        }

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            string s = Tokenize(value);
            writer.WriteValue(s);
        }

        public override bool CanConvertFrom(Type type)
            => type.Equals(typeof(string));
    }
}
