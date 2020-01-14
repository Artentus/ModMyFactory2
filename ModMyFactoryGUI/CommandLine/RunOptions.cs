using CommandLine;

namespace ModMyFactoryGUI.CommandLine
{
    [Verb("run", HelpText = "Starts ModMyFactory GUI.")]
    sealed class RunOptions : OptionsBase
    {


        public RunOptions(bool verbose, bool noLog, string appDataPath, bool noAutoUpdate)
            : base(verbose, noLog, appDataPath, noAutoUpdate)
        {

        }
    }
}
