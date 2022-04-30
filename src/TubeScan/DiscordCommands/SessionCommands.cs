using System.Diagnostics.CodeAnalysis;
using Discord.Commands;
using TubeScan.Telemetry;

namespace TubeScan.DiscordCommands
{
    [ExcludeFromCodeCoverage] // Excluded until Discord.Net provides complete interfaces
    internal class SessionCommands : ModuleBase<SocketCommandContext>
    {
        private readonly ITelemetry _telemetry;

        public SessionCommands(ITelemetry telemetry)
        {
            _telemetry = telemetry;
        }

        [Command("start", RunMode = RunMode.Async)]
        public async Task StartConversationCommand()
        {
            try
            {
                var authorId = Context.GetAuthorId();

                if (!Context.IsDMChannel())
                {
                    var replyChannel = await Context.Message.Author.CreateDMChannelAsync();
                    await replyChannel.SendMessageAsync("Starting a new conversation...");

                    ReplyAsync($"Check your DMs {Context.GetAuthorMention()}");
                }
                else
                {
                    ReplyAsync("Starting a new conversation...");
                }
            }
            catch (Exception ex)
            {
                _telemetry.Message(ex.ToString());
                ReplyAsync(ex.Message);
            }
        }
    }
}
