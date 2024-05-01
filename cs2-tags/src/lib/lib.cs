using CounterStrikeSharp.API.Modules.Utils;
using System.Reflection;
using static Tags.Tags;

namespace Tags;

public static class Lib
{
    public static string TeamName(CsTeam team)
    {
        return team switch
        {
            CsTeam.Spectator => ReplaceTags(Instance.Config.Settings["specname"], CsTeam.Spectator),
            CsTeam.Terrorist => ReplaceTags(Instance.Config.Settings["tname"], CsTeam.Terrorist),
            CsTeam.CounterTerrorist => ReplaceTags(Instance.Config.Settings["ctname"], CsTeam.CounterTerrorist),
            CsTeam.None => ReplaceTags(Instance.Config.Settings["nonename"], CsTeam.None),
            _ => ReplaceTags(Instance.Config.Settings["nonename"], CsTeam.None)
        };
    }

    public static string ReplaceTags(string message, CsTeam team)
    {
        if (message.Contains('{'))
        {
            string modifiedValue = message;

            foreach (FieldInfo field in typeof(ChatColors).GetFields())
            {
                string pattern = $"{{{field.Name}}}";

                if (message.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    modifiedValue = modifiedValue.Replace(pattern, field.GetValue(null)!.ToString(), StringComparison.OrdinalIgnoreCase);
                }
            }
            return modifiedValue.Replace("{TeamColor}", ChatColors.ForTeam(team).ToString());
        }

        return message;
    }
}