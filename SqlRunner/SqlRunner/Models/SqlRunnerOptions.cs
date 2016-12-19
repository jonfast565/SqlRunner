using CommandLine;

namespace SqlRunner.Models
{
    [Verb("run", HelpText = "Run the application")]
    internal class SqlRunnerOptions
    {
        [Option('d', "daemon", HelpText = "This process will run as a daemon", Required = false)]
        public bool AsDaemonProcess { get; set; }
    }
}