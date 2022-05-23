using System.ComponentModel;
using Discord.Commands;
using Tk.Extensions;

namespace TubeScan.DiscordCommands
{
    internal static class HelpExtensions
    {
        public static IEnumerable<Type> GetDiscordCommandTypes(this Type type) => type.Assembly.GetTypes()
                            .Where(t => t.IsAssignableTo(typeof(ModuleBase<SocketCommandContext>)));

        public static IEnumerable<(string, string, string[])> GetCommandHelp(this IEnumerable<Type> discordCommands)
        {            
            var methods = discordCommands.SelectMany(t => t.GetMethods())
                                         .Select(mi => new
                                         {
                                             method = mi,
                                             cmdAttr = mi.GetCustomAttributes(false).OfType<CommandAttribute>().FirstOrDefault(),
                                             descAttr = mi.GetCustomAttributes(false).OfType<DescriptionAttribute>().FirstOrDefault(),
                                             aliasAttr = mi.GetCustomAttributes(false).OfType<AliasAttribute>().FirstOrDefault(),
                                         })
                                         .Where(a => !string.IsNullOrWhiteSpace(a.cmdAttr?.Text));

            return methods.Select(a => (a.cmdAttr.Text, a.descAttr?.Description, a.aliasAttr?.Aliases))
                .OrderBy(t => t.Text);
        }

        public static IEnumerable<string> FormatCommandHelp(this IEnumerable<(string, string, string[])> cmds)
        {
            var pad = cmds.Max(t => t.Item1.Length);

            Func<string[], string> joinAliases = xs => xs.Select(s => $"``{s}``").Join(", ");
            Func<string[], string> aliases = xs => xs != null ? $" Aliases: {joinAliases(xs)}" : "";

            return cmds.Select(t => $"``{t.Item1.PadRight(pad)}`` {t.Item2}{aliases(t.Item3)}");
        }
    }
}
