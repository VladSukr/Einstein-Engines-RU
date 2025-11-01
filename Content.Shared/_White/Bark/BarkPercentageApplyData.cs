using System;
using Robust.Shared.ViewVariables;
using Robust.Shared.Serialization;

namespace Content.Shared._White.Bark;

[Serializable, NetSerializable]
public sealed partial class BarkPercentageApplyData
{
    [ViewVariables(VVAccess.ReadWrite)] public byte Pause { get; set; }
    [ViewVariables(VVAccess.ReadWrite)] public byte Pitch { get; set; }
    [ViewVariables(VVAccess.ReadWrite)] public byte PitchVariance { get; set; }
    [ViewVariables(VVAccess.ReadWrite)] public byte Volume { get; set; }

    public static BarkPercentageApplyData Default => new();

    public BarkPercentageApplyData Clone()
    {
        return new BarkPercentageApplyData
        {
            Pause = Pause,
            Pitch = Pitch,
            PitchVariance = PitchVariance,
            Volume = Volume,
        };
    }
}
