//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System.Collections.Generic;
using System.IO;

namespace ModMyFactory.Server
{
    /// <summary>
    /// Stores information required to start a Factorio headless server
    /// </summary>
    public sealed class ServerStartOptions
    {
        /// <summary>
        /// Network port to use
        /// </summary>
        public Port Port { get; set; }

        /// <summary>
        /// IP address (and optionally port) to bind to
        /// </summary>
        public BindingTarget? BindingTarget { get; set; }

        /// <summary>
        /// Port to use for RCON
        /// </summary>
        public Port RconPort { get; set; }

        /// <summary>
        /// IP address and port to use for RCON
        /// </summary>
        public BindingTarget? RconBindingTarget { get; set; }

        /// <summary>
        /// Password for RCON
        /// </summary>
        public string? RconPassword { get; set; }

        /// <summary>
        /// File with server settings
        /// </summary>
        public FileInfo? SettingsFile { get; set; }

        /// <summary>
        /// If the whitelist should be used
        /// </summary>
        public bool? UseWhitelist { get; set; }

        /// <summary>
        /// File with server whitelist
        /// </summary>
        public FileInfo? WhitelistFile { get; set; }

        /// <summary>
        /// File with server banlist
        /// </summary>
        public FileInfo? BanlistFile { get; set; }

        /// <summary>
        /// File with server adminlist
        /// </summary>
        public FileInfo? AdminlistFile { get; set; }

        /// <summary>
        /// File where a copy of the server's log will be stored
        /// </summary>
        public FileInfo? ConsoleLogFile { get; set; }

        /// <summary>
        /// File where server ID will be stored to or read from
        /// </summary>
        public FileInfo? ServerIdFile { get; set; }

        internal List<string> ToArgs(FileInfo savegameFile)
        {
            var args = new List<string>
            {
                "--start-server",
                savegameFile.FullName
            };

            if (!Port.IsNone)
            {
                args.Add("--port");
                args.Add(Port.ToString());
            }

            if (!(BindingTarget is null))
            {
                args.Add("--bind");
                args.Add(BindingTarget.ToString());
            }

            if (!RconPort.IsNone)
            {
                args.Add("--rcon-port");
                args.Add(RconPort.ToString());
            }

            if (!(RconBindingTarget is null))
            {
                args.Add("--rcon-bind");
                args.Add(RconBindingTarget.ToString());
            }

            if (!string.IsNullOrEmpty(RconPassword))
            {
                args.Add("--rcon-password");
                args.Add(RconPassword);
            }

            if (!(SettingsFile is null))
            {
                args.Add("--server-settings");
                args.Add(SettingsFile.FullName);
            }

            if (UseWhitelist.HasValue)
            {
                args.Add("--use-server-whitelist");
                args.Add(UseWhitelist.Value.ToString());
            }

            if (!(WhitelistFile is null))
            {
                args.Add("--server-whitelist");
                args.Add(WhitelistFile.FullName);
            }

            if (!(BanlistFile is null))
            {
                args.Add("--server-banlist");
                args.Add(BanlistFile.FullName);
            }

            if (!(AdminlistFile is null))
            {
                args.Add("--server-adminlist");
                args.Add(AdminlistFile.FullName);
            }

            if (!(ConsoleLogFile is null))
            {
                args.Add("--console-log");
                args.Add(ConsoleLogFile.FullName);
            }

            if (!(ServerIdFile is null))
            {
                args.Add("--server-id");
                args.Add(ServerIdFile.FullName);
            }

            return args;
        }
    }
}
