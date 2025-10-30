using Content.Shared._Sunrise.SunriseCCVars;
using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /*
        * Mood System
        */

    /*public static readonly CVarDef<bool> MoodEnabled =
#if RELEASE
        CVarDef.Create("mood.enabled", true, CVar.SERVER);
#else
        CVarDef.Create("mood.enabled", false, CVar.SERVER);
#endif*/
    public static CVarDef<bool> MoodEnabled => SunriseCCVars.MoodEnabled;

    /*public static readonly CVarDef<bool> MoodIncreasesSpeed =
        CVarDef.Create("mood.increases_speed", true, CVar.SERVER);*/
    public static CVarDef<bool> MoodIncreasesSpeed => SunriseCCVars.MoodIncreasesSpeed;

    /*public static readonly CVarDef<bool> MoodDecreasesSpeed =
        CVarDef.Create("mood.decreases_speed", true, CVar.SERVER);*/
    public static CVarDef<bool> MoodDecreasesSpeed => SunriseCCVars.MoodDecreasesSpeed;

    public static readonly CVarDef<bool> MoodModifiesThresholds =
        CVarDef.Create("mood.modify_thresholds", false, CVar.SERVER);

    public static readonly CVarDef<bool> MoodVisualEffects =
        CVarDef.Create("mood.visual_effects", true, CVar.CLIENTONLY | CVar.ARCHIVE);
}
