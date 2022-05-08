using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Discord.Commands;
using Tk.Extensions;
using Tk.Extensions.Reflection;
using TubeScan.Telemetry;

namespace TubeScan.DiscordCommands
{
    [ExcludeFromCodeCoverage] // Excluded until Discord.Net provides complete interfaces
    internal class AboutCommands : ModuleBase<SocketCommandContext>
    {
        private readonly ITelemetry _telemetry;
        private const string HelpCommand = "help";

        public AboutCommands(ITelemetry telemetry)
        {
            _telemetry = telemetry;
        }

        [Command("about", RunMode = RunMode.Async)]
        [System.ComponentModel.Description("About this bot")]
        public Task ShowAboutAsync()
        {
            try
            {
                var attrs = typeof(ProgramBootstrap).Assembly.GetCustomAttributes();

                var msg = new[]
                {
                    attrs.GetAttributeValue<AssemblyDescriptionAttribute>(a => a.Description),
                    ""
                }
                .Concat(ProgramBootstrap.GetVersionNotices(attrs))
                .Concat(ProgramBootstrap.Get3rdPartyNotices())
                .Concat(new[] { "", $"For command help, just enter ``{HelpCommand}``.", ""})
                .Where(x => x != null).Join(Environment.NewLine);

                var eb = new Discord.EmbedBuilder()
                                    .WithTitle(attrs.GetAttributeValue<AssemblyProductAttribute>(a => a.Product))
                                    .WithUrl("https://github.com/tonycknight/tubescan")
                                    .WithDescription(msg)
                                    .Build();

                return ReplyAsync(embed: eb);
            }
            catch (Exception ex)
            {
                _telemetry.Message(ex.ToString());
                return ReplyAsync(ex.Message);
            }
        }

        [Command(HelpCommand, RunMode = RunMode.Async)]
        public Task ShowHelpAsync()
        {
            try
            {                
                var msg = this.GetType().GetDiscordCommandTypes()
                                .GetCommandHelp()
                                .FormatCommandHelp()
                                .Join(Environment.NewLine);
                
                return ReplyAsync(msg);
            }
            catch (Exception ex)
            {
                _telemetry.Message(ex.ToString());
                return ReplyAsync(ex.Message);
            }
        }
        }
}
