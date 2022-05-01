using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Discord.Commands;
using Tk.Extensions;
using TubeScan.Telemetry;

namespace TubeScan.DiscordCommands
{
    [ExcludeFromCodeCoverage] // Excluded until Discord.Net provides complete interfaces
    internal class AboutCommands : ModuleBase<SocketCommandContext>
    {
        private readonly ITelemetry _telemetry;

        public AboutCommands(ITelemetry telemetry)
        {
            _telemetry = telemetry;
        }

        [Command("about", RunMode = RunMode.Async)]
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
                .Concat(new[] { "", "For command help, just enter ``help``.", ""})
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

        [Command("help", RunMode = RunMode.Async)]
        public Task ShowHelpAsync()
        {
            try
            {
                var cmds = new[] {
                    ("start", "Start a private conversation with the bot."),
                    ("lines", "Show line status"),
                    ("station", "Get a station's status. Form: ``station <tag>``"),
                    ("tags", "List all your tags"),
                    ("tag", "Set a tag. Form: ``tag <tag name> <station name>``"),
                    ("about", "About this bot"),
                };

                var pad = cmds.Max(t => t.Item1.Length);
                var msg = cmds.Select(t => $"``{t.Item1.PadRight(pad)}`` {t.Item2}").Join(Environment.NewLine);

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
