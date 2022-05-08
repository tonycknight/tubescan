using System.ComponentModel;
using Discord.Commands;

namespace TubeScan.DiscordCommands
{
    internal static class HelpExtensions
    {
        public static IEnumerable<Type> GetDiscordCommandTypes(this Type type) => type.Assembly.GetTypes()
                            .Where(t => t.IsAssignableTo(typeof(ModuleBase<SocketCommandContext>)));

        public static IEnumerable<(string, string)> GetCommandHelp(this IEnumerable<Type> discordCommands)
        {            
            var methods = discordCommands.SelectMany(t => t.GetMethods())
                                         .Select(mi => new
                                         {
                                             method = mi,
                                             cmdAttr = mi.GetCustomAttributes(false).OfType<CommandAttribute>().FirstOrDefault(),
                                             descAttr = mi.GetCustomAttributes(false).OfType<DescriptionAttribute>().FirstOrDefault(),
                                         })
                                         .Where(a => !string.IsNullOrWhiteSpace(a.cmdAttr?.Text));

            return methods.Select(a => (a.cmdAttr.Text, a.descAttr?.Description))
                .OrderBy(t => t.Text);
        }

        public static IEnumerable<string> FormatCommandHelp(this IEnumerable<(string, string)> cmds)
        {
            var pad = cmds.Max(t => t.Item1.Length);
            return cmds.Select(t => $"``{t.Item1.PadRight(pad)}`` {t.Item2}");
        }
    }
}
