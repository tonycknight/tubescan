using System.Reflection;
using Crayon;
using Microsoft.Extensions.DependencyInjection;
using Tk.Extensions;
using Tk.Extensions.Reflection;
using Tk.Extensions.Time;

namespace TubeScan
{
    internal class ProgramBootstrap
    {
        public static IServiceProvider CreateServiceCollection() =>
            new ServiceCollection()
                .AddSingleton<DiscordClient.IDiscordProxy, DiscordClient.DiscordProxy>()
                .AddSingleton<IList<Telemetry.ITelemetry>>(sp => new Telemetry.ITelemetry[] { new Telemetry.ConsoleTelemetry() })
                .AddSingleton<Telemetry.ITelemetry, Telemetry.AggregatedTelemetry>()
                .AddSingleton<ITimeProvider, TimeProvider>()
                .AddSingleton<Io.IIoProvider, Io.IoProvider>()
                .AddSingleton<Config.FileAppConfigurationProvider>()
                .AddSingleton<Config.EnvVarAppConfigurationProvider>()
                .AddSingleton<Config.IAppConfigurationProvider, Config.AppConfigurationProvider>()
                .AddHttpClient()
                .AddSingleton<Tfl.ITflClient, Tfl.TflHttpClient>()
                .AddSingleton<Lines.ILineProvider, Lines.TflLineProvider>()
                .AddSingleton<Stations.IStationProvider, Stations.StationProvider>()
                .AddSingleton<Stations.TflStationProvider, Stations.TflStationProvider>()
                .AddSingleton<Stations.IStationTagRepository, Stations.MongoStationTagRepository>()
                .AddSingleton<Users.IUsersRepository, Users.MongoUsersRepository>()
                .AddSingleton<Scheduling.IJobScheduler, Scheduling.JobScheduler>()
                //.AddSingleton<Lines.LineStatusPollingJob>()
                .BuildServiceProvider();


        public static string GetDescription()
        {
            var attrs = typeof(ProgramBootstrap).Assembly.GetCustomAttributes();

            return "".Singleton()
                .Append(new[]
                            {
                                Output.Bright.Magenta(attrs.GetAttributeValue<AssemblyProductAttribute>(a => a.Product)),
                                attrs.GetAttributeValue<AssemblyDescriptionAttribute>(a => a.Description),
                    
                            }.Join(" - "))
                .Append("")
                .Concat(GetVersionNotices(attrs).Select(x => Output.Bright.Yellow(x)))
                .Append("")
                .Concat(Get3rdPartyNotices())
                .Join(Environment.NewLine);
        }

        public static IEnumerable<string> GetVersionNotices(IEnumerable<Attribute> attrs) => new[]
            {
                attrs.GetAttributeValue<AssemblyInformationalVersionAttribute>(a => a.InformationalVersion).Format("Version {0} alpha"),
                attrs.GetAttributeValue<AssemblyCopyrightAttribute>(a => a.Copyright),
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
