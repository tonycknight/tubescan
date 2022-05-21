using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using Tk.Extensions;
using Tk.Extensions.Reflection;
using TubeScan.Config;
using TubeScan.DiscordClient;
using TubeScan.Scheduling;
using TubeScan.Telemetry;

namespace TubeScan.AppCommands
{
    [ExcludeFromCodeCoverage]
    [Command("start", Description = "Start the bot")]
    internal class StartServiceCommand
    {
        private readonly IDiscordProxy _discordProxy;
        private readonly ITelemetry _telemetry;
        private readonly IAppConfigurationProvider _configProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly IJobScheduler _jobScheduler;

        public StartServiceCommand(IDiscordProxy discordProxy, ITelemetry telemetry, 
                                    IAppConfigurationProvider configProvider,
                                    IServiceProvider serviceProvider, IJobScheduler jobScheduler)
        {
            _discordProxy = discordProxy;
            _telemetry = telemetry;
            _configProvider = configProvider;
            _serviceProvider = serviceProvider;
            _jobScheduler = jobScheduler;
        }

        [Option(CommandOptionType.SingleValue, Description = "The configuration file's path.", LongName = "config", ShortName = "c")]
        public string ConfigurationFile { get; set; }

        public async Task<int> OnExecuteAsync()
        {
            var config = GetConfig();

            var attrs = typeof(ProgramBootstrap).Assembly.GetCustomAttributes();
            _telemetry.Message($"{attrs.GetAttributeValue<AssemblyProductAttribute>(a => a.Product)} {attrs.GetAttributeValue<AssemblyInformationalVersionAttribute>(a => a.InformationalVersion).Format("Version {0}")}");

            _telemetry.Message("Starting services...");
            
            _telemetry.Message("Starting client...");
            await _discordProxy.StartAsync();
            await CreateCommandHandler(_discordProxy);
            _telemetry.Message("Started client.");

            _telemetry.Message("Starting job scheduler...");            
            _jobScheduler.Start();
            _telemetry.Message("Finished job scheduler.");

            _telemetry.Message("Startup complete.");
            _telemetry.Message($"Bot registration URI: {_discordProxy.BotRegistrationUri}");
            _telemetry.Message("Proxy started. Hit CTRL-C to quit");

            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += async (object? sender, ConsoleCancelEventArgs e) =>
            {
                _telemetry.Message("Shutting down job scheduler...");
                _jobScheduler.Stop();
                _telemetry.Message("Shut down job scheduler.");

                _telemetry.Message("Shutting down services...");

                await _discordProxy.StopAsync();

                _discordProxy.Dispose();
                
                cts.Cancel();

                _telemetry.Message("Services shutdown");
            };

            WaitHandle.WaitAll(new[] { cts.Token.WaitHandle });

            return true.ToReturnCode();
        }


        private AppConfiguration GetConfig()
        {
            new StartServiceCommandValidator().Validate(this);

            return ConfigurationFile.Pipe(_configProvider.SetFilePath)
                                          .Pipe(c => c.GetAppConfiguration())
                                          .Validate();
        }

        private async Task<DiscordCommands.CommandsHandler> CreateCommandHandler(DiscordClient.IDiscordProxy client)
        {
            var adminHandler = new DiscordCommands.CommandsHandler(client.Client, new Discord.Commands.CommandService(), _telemetry, _serviceProvider);

            await adminHandler.InstallCommandsAsync();

            return adminHandler;
        }
    }
}
