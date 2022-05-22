using System.Diagnostics.CodeAnalysis;
using Discord.Commands;
using TubeScan.Lines;
using TubeScan.Telemetry;

namespace TubeScan.DiscordCommands
{
    [ExcludeFromCodeCoverage] // Excluded until Discord.Net provides complete interfaces
    internal class LineCommands : ModuleBase<SocketCommandContext>
    {
        private readonly ITelemetry _telemetry;
        private readonly ILineProvider _lineProvider;

        public LineCommands(ITelemetry telemetry, Lines.ILineProvider lineProvider)
        {
            _telemetry = telemetry;
            _lineProvider = lineProvider;
        }
                
        [Command("lines", RunMode = RunMode.Async)]
        [System.ComponentModel.Description("Show line status")]
        public async Task ShowLineStatusAsync()
        {
            try
            {
                var responseMsg = await ReplyAsync(":thinking: *Thinking...*");
                
                var lines = await _lineProvider.GetLinesAsync();
                var statuses = await _lineProvider.GetLineStatusAsync();
                var stats = statuses.ToDictionary(l => l.Id, l => l, StringComparer.InvariantCultureIgnoreCase);
                
                var msg = lines.RenderLinesStatus(stats, true);
                if(msg.Length > 4095)
                {
                    msg = lines.RenderLinesStatus(stats, false);
                }

                var eb = new Discord.EmbedBuilder()
                                    .WithTitle("Tube lines")
                                    .WithUrl("https://tfl.gov.uk/tube-dlr-overground/status/")
                                    .WithDescription(msg)
                                    .Build();

                responseMsg.ModifyAsync(mp =>
                {
                    mp.Content = "";
                    mp.Embed = eb;
                });

            }
            catch (Exception ex)
            {
                _telemetry.Message(ex.ToString());
                ReplyAsync(ex.Message);
            }
        }
    }
}
