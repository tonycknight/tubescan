using System.Diagnostics.CodeAnalysis;
using Discord.Commands;
using Tk.Extensions;
using TubeScan.Telemetry;
using TubeScan.Users;

namespace TubeScan.DiscordCommands
{
    [ExcludeFromCodeCoverage] // Excluded until Discord.Net provides complete interfaces
    internal class SessionCommands : ModuleBase<SocketCommandContext>
    {
        private readonly ITelemetry _telemetry;
        private readonly IUsersRepository _usersRepo;

        public SessionCommands(ITelemetry telemetry, IUsersRepository usersRepo)
        {
            _telemetry = telemetry;
            _usersRepo = usersRepo;
        }

        [Command("start", ignoreExtraArgs: true, RunMode = RunMode.Async)]
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

                await _usersRepo.SetUserAsync(new Models.User(authorId, Context.GetAuthorMention()));
            }
            catch (Exception ex)
            {
                ex.ToString().CreateTelemetryEvent().Send(_telemetry);
                ReplyAsync(ex.Message);
            }
        }
    }
}
