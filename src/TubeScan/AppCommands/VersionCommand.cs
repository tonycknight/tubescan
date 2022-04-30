using System.Diagnostics.CodeAnalysis;
using McMaster.Extensions.CommandLineUtils;

namespace TubeScan.AppCommands
{
    [ExcludeFromCodeCoverage]
    [Command("version", Description = "Get the bot's version info.")]
    internal class VersionCommand
    {
        public Task<int> OnExecuteAsync()
        {
            var line = ProgramBootstrap.GetDescription();
            Console.Out.WriteLine(line);

            return Task.FromResult(true.ToReturnCode());
        }
    }
}
