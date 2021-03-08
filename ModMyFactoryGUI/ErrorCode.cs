//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using CommandLine;
using System;
using System.Collections.Generic;

namespace ModMyFactoryGUI
{
    [Flags]
    internal enum ErrorCode : int
    {
        NoError = 0x00000000,
        General = 0x00000001,

        CommandLine = 0x01000000,
        CommandLine_General = CommandLine | General,
        CommandLine_HelpRequested = NoError, // This is not actually an error
        CommandLine_MissingOption = CommandLine | 0x00000002,
        CommandLine_DuplicateOption = CommandLine | 0x00000004,
        CommandLine_ListOutOfRange = CommandLine | 0x00000008,
        CommandLine_UnknownOption = CommandLine | 0x00000010,
        CommandLine_BadFormat = CommandLine | 0x00000020,

        GameStart = 0x02000000,
        GameStart_General = GameStart | General,
        GameStart_InvalidInstance = GameStart | 0x00000002,
    }

    internal static class ErrorCodeFactory
    {
        public static ErrorCode FromCommandLineErrors(IEnumerable<Error> errors)
        {
            var code = ErrorCode.NoError;
            foreach (var error in errors)
            {
                code |= error.Tag switch
                {
                    ErrorType.HelpRequestedError => ErrorCode.CommandLine_HelpRequested,
                    ErrorType.HelpVerbRequestedError => ErrorCode.CommandLine_HelpRequested,
                    ErrorType.VersionRequestedError => ErrorCode.CommandLine_HelpRequested,

                    ErrorType.MissingGroupOptionError => ErrorCode.CommandLine_MissingOption,
                    ErrorType.MissingRequiredOptionError => ErrorCode.CommandLine_MissingOption,
                    ErrorType.MissingValueOptionError => ErrorCode.CommandLine_MissingOption,

                    ErrorType.RepeatedOptionError => ErrorCode.CommandLine_DuplicateOption,

                    ErrorType.SequenceOutOfRangeError => ErrorCode.CommandLine_ListOutOfRange,

                    ErrorType.UnknownOptionError => ErrorCode.CommandLine_UnknownOption,
                    ErrorType.BadVerbSelectedError => ErrorCode.CommandLine_UnknownOption,

                    ErrorType.BadFormatConversionError => ErrorCode.CommandLine_BadFormat,
                    ErrorType.BadFormatTokenError => ErrorCode.CommandLine_BadFormat,

                    _ => ErrorCode.CommandLine_General
                };
            }

            return code;
        }
    }
}
