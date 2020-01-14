using CommandLine;

namespace ModMyFactoryGUI.CommandLine
{
    abstract class OptionsBase
    {
        [Option('v', "verbose", SetName = "_log_active", HelpText = "Enables verbose logging.")]
        public bool Verbose { get; }

        [Option('l', "no-log", SetName = "_log_inactive", HelpText = "Disables logging to file (console logging still enabled).")]
        public bool NoLog { get; }

        [Option('a', "app-data", HelpText = "Overrides the application data path.")]
        public string AppDataPath { get; }

        [Option('u', "no-update", HelpText = "Disables the automatic update check.")]
        public bool NoAutoUpdate { get; }

        protected OptionsBase(bool verbose, bool noLog, string appDataPath, bool noAutoUpdate)
        {
            Verbose = verbose;
            NoLog = noLog;
            AppDataPath = appDataPath;
            NoAutoUpdate = noAutoUpdate;
        }
    }
}
