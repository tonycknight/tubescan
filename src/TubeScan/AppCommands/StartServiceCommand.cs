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
            $"{attrs.GetAttributeValue<AssemblyProductAttribute>(a => a.Product)} {attrs.GetAttributeValue<AssemblyInformationalVersionAttribute>(a => a.InformationalVersion).Format("Version {0}")}"
                .CreateTelemetryEvent(TelemetryEventKind.Highlight).Send(_telemetry);

            "Starting services...".CreateTelemetryEvent().Send(_telemetry);

            "Starting client...".CreateTelemetryEvent().Send(_telemetry);
            await _discordProxy.StartAsync();
            await CreateCommandHandler(_discordProxy);
            "Started client.".CreateTelemetryEvent().Send(_telemetry);

            "Starting job scheduler...".CreateTelemetryEvent().Send(_telemetry);
            var jobs = GetJobSchedules().ToList();
            $"Found {jobs.Count} job(s)".CreateTelemetryEvent().Send(_telemetry);
            _jobScheduler.Register(jobs);
            _jobScheduler.Start();
            "Finished job scheduler.".CreateTelemetryEvent().Send(_telemetry);

            "Startup complete.".CreateTelemetryEvent().Send(_telemetry);
            $"Bot registration URI: {_discordProxy.BotRegistrationUri}".CreateTelemetryEvent(TelemetryEventKind.Highlight).Send(_telemetry);
            "Proxy started. Hit CTRL-C to quit".CreateTelemetryEvent(TelemetryEventKind.Highlight).Send(_telemetry);

            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += async (object? sender, ConsoleCancelEventArgs e) =>
            {                
                "Shutting down job scheduler...".CreateTelemetryEvent().Send(_telemetry);
                _jobScheduler.Stop();
                "Shut down job scheduler.".CreateTelemetryEvent().Send(_telemetry);

                "Shutting down services...".CreateTelemetryEvent().Send(_telemetry);

                await _discordProxy.StopAsync();

                _discordProxy.Dispose();
                
                cts.Cancel();

                "Services shutdown".CreateTelemetryEvent().Send(_telemetry);
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

        private IEnumerable<JobScheduleInfo> GetJobSchedules()
        {
            var jobTypes = this.GetType().Assembly.GetTypes()
                               .Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(typeof(IJob)));

            return jobTypes.Select(_serviceProvider.GetService)
                           .Where(o => o != null)
                           .OfType<IJob>()
                           .Select(j => new JobScheduleInfo(j, j.Frequency))
                           .ToList();
        }
    }
}
