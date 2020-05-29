//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System.Text;

namespace ModMyFactory.Game
{
    internal class ArgumentBuilder
    {
        private readonly StringBuilder _builder = new StringBuilder();

        private static bool ContainsSpace(string argument)
        {
            foreach (char c in argument)
            {
                if (char.IsWhiteSpace(c))
                    return true;
            }

            return false;
        }

        public void AppendArgument(string argument)
        {
            if (_builder.Length > 0) _builder.Append(' ');

            if (ContainsSpace(argument)) _builder.AppendFormat("\"{0}\"", argument);
            else _builder.Append(argument);
        }

        public void AppendArguments(params string[] arguments)
        {
            foreach (var arg in arguments)
                AppendArgument(arg);
        }

        public void AppendExisting(string arguments)
        {
            if (_builder.Length > 0) _builder.Append(' ');
            _builder.Append(arguments);
        }

        public override string ToString() => _builder.ToString();
    }
}
