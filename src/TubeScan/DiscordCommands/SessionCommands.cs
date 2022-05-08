using System.Diagnostics.CodeAnalysis;
using Discord.Commands;
using Tk.Extensions;
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
        [System.ComponentModel.Description("Start a private conversation with the bot.")]
        public async Task StartConversationCommand()
        {
            try
            {
                var authorId = Context.GetAuthorId();

                var helpMsg = this.GetType().GetDiscordCommandTypes()
                            .GetCommandHelp()
                            .FormatCommandHelp()
                            .Join(Environment.NewLine);

                if (!Context.IsDMChannel())
                {
                    var replyChannel = await Context.Message.Author.CreateDMChannelAsync();
                    await replyChannel.SendMessageAsync(helpMsg);

                    ReplyAsync($"Check your DMs {Context.GetAuthorMention()}");
                }
                else
                {                    
                    ReplyAsync(helpMsg);
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
