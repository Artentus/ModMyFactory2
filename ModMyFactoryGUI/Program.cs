//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;
using CommandLine;
using CommandLine.Text;
using ModMyFactoryGUI.CommandLine;
using ModMyFactoryGUI.Helpers;
using System;

namespace ModMyFactoryGUI
{
    internal static class Program
    {
        public const int NoError = 0;
        public const int GeneralError = 1;

        private static int StartGame(StartGameOptions options)
        {
            return NoError;
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        private static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
#if DEBUG
                .LogToDebug()
#endif
                .UsePlatformDetect()
                .UseReactiveUI()
                .UseManagedSystemDialogs();
        }

        private static int StartApp(string[] args, OptionsBase options)
        {
            return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
        }

        private static HelpText ConfigureHelpText(HelpText helpText)
        {
            helpText.AdditionalNewLineAfterOption = false;
            return helpText;
        }

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static int Main(string[] args)
        {
            bool hasConsole = ConsoleHelper.TryAttachConsole(out var consoleHandle);
            var parser = new Parser(config =>
            {
                config.CaseSensitive = false;
                config.HelpWriter = Console.Out;
            });

            var parsedOptions = Parser.Default.ParseArguments<RunOptions, StartGameOptions>(args);
            var helpText = HelpText.AutoBuild(parsedOptions, ConfigureHelpText, e => e);

            int result = parsedOptions.MapResult(
                (RunOptions opts) => StartApp(args, opts),
                (StartGameOptions opts) => StartGame(opts),
                errs => GeneralError);

            if (hasConsole) ConsoleHelper.FreeConsole(consoleHandle);
            return result;
        }
    }
}
