using System.Reflection;
using Crayon;
using Microsoft.Extensions.DependencyInjection;
using Tk.Extensions;
using Tk.Extensions.Linq;
using Polly;
using Tk.Extensions.Time;
using Tk.Extensions.Reflection;

namespace TubeScan
{
    internal class ProgramBootstrap
    {
        public static IServiceProvider CreateServiceCollection()
        {
            var col = new ServiceCollection()
                .AddSingleton<DiscordClient.IDiscordProxy, DiscordClient.DiscordProxy>()
                .AddSingleton<IList<Telemetry.ITelemetry>>(sp => new Telemetry.ITelemetry[] { new Telemetry.ConsoleTelemetry() })
                .AddSingleton<Telemetry.ITelemetry, Telemetry.AggregatedTelemetry>()
                .AddSingleton<ITimeProvider, Tk.Extensions.Time.TimeProvider>()
                .AddSingleton<Io.IIoProvider, Io.IoProvider>()
                .AddSingleton<Config.FileAppConfigurationProvider>()
                .AddSingleton<Config.EnvVarAppConfigurationProvider>()
                .AddSingleton<Config.AppConfigurationProvider>()
                .AddSingleton<Config.IAppConfigurationProvider>(s => s.GetRequiredService<Config.AppConfigurationProvider>())
                .AddSingleton<Config.IFileAppConfigurationProvider>(s => s.GetRequiredService<Config.AppConfigurationProvider>())
                .AddSingleton<Tfl.ITflClient, Tfl.TflHttpClient>()
                .AddSingleton<Lines.ILineReferenceProvider, Lines.LineReferenceProvider>()
                .AddSingleton<Lines.ILineStatusProvider, Lines.TflLineStatusProvider>()
                .AddSingleton<Lines.ILineProvider, Lines.LineProvider>()
                .AddSingleton<ISettable<Models.LineStatus>>(sp => (ISettable<Models.LineStatus>)sp.GetService<Lines.ILineProvider>())
                .AddSingleton<Stations.TflStationProvider, Stations.TflStationProvider>()
                .AddSingleton<Stations.IStationProvider, Stations.StationProvider>()
                .AddSingleton<ISettable<Models.Station>>(sp => (ISettable<Models.Station>)sp.GetService<Stations.IStationProvider>())
                .AddSingleton<Stations.IStationTagRepository, Stations.MongoStationTagRepository>()
                .AddSingleton<Users.IUsersRepository, Users.MongoUsersRepository>()
                .AddSingleton<Scheduling.IJobScheduler, Scheduling.JobScheduler>()
                .AddSingleton<Lines.LineStatusPollingJob>()
                .AddSingleton<Stations.StationCacheRefreshJob>();

            var rateLimit = Policy.RateLimitAsync<HttpResponseMessage>(500, TimeSpan.FromMinutes(1), 20);

            var hcb = col.AddHttpClient(Tfl.ITflClient.HttpClientName)
                         .AddPolicyHandler(rateLimit);

            return col.BuildServiceProvider();
        }

        public static string GetDescription()
        {
            var attrs = typeof(ProgramBootstrap).Assembly.GetCustomAttributes();

            return "".Singleton()
                .Append(new[]
                            {
                                Output.Bright.Magenta(attrs.GetAttributeValue<AssemblyProductAttribute, string>(a => a.Product)),
                                attrs.GetAttributeValue<AssemblyDescriptionAttribute, string>(a => a.Description),

                            }.Join(" - "))
                .Append("")
                .Concat(GetVersionNotices(attrs).Select(x => Output.Bright.Yellow(x)))
                .Append("")
                .Concat(Get3rdPartyNotices())
                .Join(Environment.NewLine);
        }

        public static IEnumerable<string> GetVersionNotices(IEnumerable<Attribute> attrs) => new[]
            {
                attrs.GetAttributeValue<AssemblyInformationalVersionAttribute, string>(a => a.InformationalVersion).Format("Version {0} alpha"),
                attrs.GetAttributeValue<AssemblyCopyrightAttribute, string>(a => a.Copyright),
                "You can find the repository at https://github.com/tonycknight/tubescan"
            };

        public static IEnumerable<string> Get3rdPartyNotices() => new[]
            {
                "Powered by TfL Open Data",
                "Contains OS data (c) Crown copyright and database rights 2016",
                "Geomni UK Map data (c) and database rights 2019"
            };

    }
}
