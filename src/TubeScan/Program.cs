using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;

namespace TubeScan
{
    [ExcludeFromCodeCoverage]
    [Subcommand(typeof(AppCommands.StartServiceCommand))]
    [Subcommand(typeof(AppCommands.VersionCommand))]
    public class Program
    {
        public static int Main(string[] args)
        {
            using var app = new CommandLineApplication<Program>();

            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(ProgramBootstrap.CreateServiceCollection());

            try
            {
                return app.Execute(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return false.ToReturnCode();
            }
        }

        private int OnExecute(CommandLineApplication app)
        {
            app.Description = typeof(ProgramBootstrap).Assembly.GetCustomAttributes()
                                                      .GetAttributeValue<AssemblyProductAttribute>(a => a.Product);
            app.ShowHelp();
            return true.ToReturnCode();
        }
    }
}