//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace ModMyFactory.ModSettings.Serialization
{
    public sealed class SerializerException : Exception
    {
        internal SerializerException(string message, Exception? innerException = null)
            : base(message, innerException)
        { }
    }

    public static class Serializer
    {
        private static readonly AccurateVersion MinVersion = (0, 16);
        private static readonly AccurateVersion ByteSwitch = (0, 17); // Starting with 0.17 there is an additional byte in the file.
        private static readonly AccurateVersion DefaultWriteVersion = (0, 17, 73, 4); // Used by Factorio 0.17.73

        private static bool FileVersionSupported(AccurateVersion fileVersion) => fileVersion >= MinVersion;

        private static void ReadList(BinaryReader reader, JsonWriter writer)
        {
            writer.WriteStartObject();

            uint count = reader.ReadUInt32();
            for (int i = 0; i < count; i++)
            {
                reader.ReadString(); // List is dictionary with empty keys
                ReadPropertyTree(reader, writer);
            }

            writer.WriteEndObject();
        }

        private static void ReadDictionary(BinaryReader reader, JsonWriter writer)
        {
            writer.WriteStartObject();

            uint count = reader.ReadUInt32();
            for (int i = 0; i < count; i++)
            {
                writer.WritePropertyName(reader.ReadString());
                ReadPropertyTree(reader, writer);
            }

            writer.WriteEndObject();
        }

        private static void ReadPropertyTree(BinaryReader reader, JsonWriter writer)
        {
            var type = (PropertyTreeType)reader.ReadByte();
            reader.ReadByte(); // Reserved

            switch (type)
            {
                case PropertyTreeType.None:
                    break;

                case PropertyTreeType.Bool:
                    writer.WriteValue(reader.ReadBoolean());
                    break;

                case PropertyTreeType.Number:
                    writer.WriteValue(reader.ReadDouble());
                    break;

                case PropertyTreeType.String:
                    writer.WriteValue(reader.ReadString());
                    break;

                case PropertyTreeType.List:
                    ReadList(reader, writer);
                    break;

                case PropertyTreeType.Dictionary:
                    ReadDictionary(reader, writer);
                    break;

                default:
                    throw new SerializerException($"Found unknown type {type} in property tree.");
            }
        }

        private static void WriteList(BinaryWriter writer, JToken token)
        {
            writer.Write((uint)token.Count());

            foreach (var child in token.Children())
            {
                writer.Write(string.Empty); // Write empty key
                WritePropertyTree(writer, child);
            }
        }

        private static void WriteDictionary(BinaryWriter writer, JToken token)
        {
            writer.Write((uint)token.Count());

            var dict = token.Value<IDictionary<string, JToken>>();
            if (dict is null) return;

            foreach (var kvp in dict)
            {
                writer.Write(kvp.Key);
                WritePropertyTree(writer, kvp.Value);
            }
        }

        private static void WritePropertyTree(BinaryWriter writer, JToken token)
        {
            var type = token.Type.ToTreeType();
            writer.Write((byte)type);
            writer.Write(type == PropertyTreeType.String); // Write reserved byte; value taken from Rsedings code, meaning unknown

            switch (type)
            {
                case PropertyTreeType.None:
                    break;

                case PropertyTreeType.Bool:
                    writer.Write(token.Value<bool>());
                    break;

                case PropertyTreeType.Number:
                    writer.Write(token.Value<double>());
                    break;

                case PropertyTreeType.String:
                    writer.Write(token.Value<string>());
                    break;

                case PropertyTreeType.List:
                    WriteList(writer, token);
                    break;

                case PropertyTreeType.Dictionary:
                    WriteDictionary(writer, token);
                    break;
            }
        }

        /// <summary>
        /// Loads a mod settings file into a JSON string.
        /// </summary>
        /// <param name="file">The file to load.</param>
        /// <param name="fileVersion">Out. The version of the file.</param>
        /// <param name="formatting">Optional. Formatting of the JSON string.</param>
        public static string LoadFile(FileInfo file, out AccurateVersion fileVersion, Formatting formatting = Formatting.Indented)
        {
            using var stream = file.OpenRead();
            using var reader = new AccurateBinaryReader(stream);

            fileVersion = reader.ReadVersion();
            if (!FileVersionSupported(fileVersion)) throw new SerializerException("File version not supported.");
            if (fileVersion >= ByteSwitch) reader.ReadByte();

            var sb = new StringBuilder();
            var sw = new StringWriter(sb);
            var writer = new JsonTextWriter(sw) { Formatting = formatting };

            try
            {
                ReadPropertyTree(reader, writer);
                return sw.ToString();
            }
            catch (Exception ex) when (ex is EndOfStreamException || ex is JsonException)
            {
                throw new SerializerException("Specified file is not a valid settings file.", ex);
            }
        }

        /// <summary>
        /// Loads a mod settings file into a JSON string.
        /// </summary>
        /// <param name="fileName">The file name to load.</param>
        /// <param name="fileVersion">Out. The version of the file.</param>
        /// <param name="formatting">Optional. Formatting of the JSON string.</param>
        public static string LoadFile(string fileName, out AccurateVersion fileVersion, Formatting formatting = Formatting.Indented)
        {
            var file = new FileInfo(fileName);
            return LoadFile(file, out fileVersion, formatting);
        }

        /// <summary>
        /// Saves a JSON string to a settings file.
        /// </summary>
        /// <param name="json">The string to save.</param>
        /// <param name="file">The file to save to.</param>
        /// <param name="fileVersion">The desired file version.</param>
        public static void SaveToFile(string json, FileInfo file, AccurateVersion fileVersion)
        {
            if (!file.Directory.Exists) file.Directory.Create();
            using var stream = file.Open(FileMode.Create, FileAccess.Write);
            using var writer = new AccurateBinaryWriter(stream);

            writer.Write(fileVersion);
            if (fileVersion > ByteSwitch) writer.Write(byte.MinValue);

            if (string.IsNullOrWhiteSpace(json))
            {
                writer.Write((byte)PropertyTreeType.None);
                return;
            }

            try
            {
                var token = JObject.Parse(json);
                WritePropertyTree(writer, token);
            }
            catch (Exception ex) when (ex is InvalidEnumArgumentException || ex is JsonException)
            {
                throw new ArgumentException("Invalid JSON string.", nameof(file), ex);
            }
        }

        /// <summary>
        /// Saves a JSON string to a settings file.
        /// </summary>
        /// <param name="json">The string to save.</param>
        /// <param name="file">The file to save to.</param>
        public static void SaveToFile(string json, FileInfo file)
            => SaveToFile(json, file, DefaultWriteVersion);

        /// <summary>
        /// Saves a JSON string to a settings file.
        /// </summary>
        /// <param name="json">The string to save.</param>
        /// <param name="fileName">The file name to save to.</param>
        /// <param name="fileVersion">The desired file version.</param>
        public static FileInfo SaveToFile(string json, string fileName, AccurateVersion fileVersion)
        {
            var file = new FileInfo(fileName);
            SaveToFile(json, file, fileVersion);
            return file;
        }

        /// <summary>
        /// Saves a JSON string to a settings file.
        /// </summary>
        /// <param name="json">The string to save.</param>
        /// <param name="fileName">The file name to save to.</param>
        public static FileInfo SaveToFile(string json, string fileName)
            => SaveToFile(json, fileName, DefaultWriteVersion);
    }
}
