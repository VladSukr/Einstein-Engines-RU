using Content.Shared._Sunrise.SunriseCCVars;
using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Controls if the connections queue is enabled
    ///     If enabled plyaers will be added to a queue instead of being kicked after SoftMaxPlayers is reached
    /// </summary>
    public static CVarDef<bool> QueueEnabled => SunriseCCVars.QueueEnabled;
}
