using System.Diagnostics.CodeAnalysis;
using TubeScan.Io;

namespace TubeScan.AppCommands
{
    [ExcludeFromCodeCoverage]
    internal class StartServiceCommandValidator
    {
        public StartServiceCommand Validate(StartServiceCommand command)
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            command.ConfigurationFile = command.ConfigurationFile?
                .InvalidOpArg(string.IsNullOrWhiteSpace, $"The {nameof(command.ConfigurationFile)} parameter is missing.")
                .ResolveWorkingPath()
                .AssertFileExists();
#pragma warning restore CS8601 // Possible null reference assignment.

            return command;
        }        
    }
}
