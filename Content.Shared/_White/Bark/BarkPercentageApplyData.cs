using System;
using Robust.Shared.Serialization;
using Robust.Shared.ViewVariables;

namespace Content.Shared._White.Bark;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class BarkPercentageApplyData
{
    [DataField("pause")]
    [ViewVariables(VVAccess.ReadWrite)]
    public byte Pause { get; set; }

    [DataField("pitch")]
    [ViewVariables(VVAccess.ReadWrite)]
    public byte Pitch { get; set; }

    [DataField("pitchVariance")]
    [ViewVariables(VVAccess.ReadWrite)]
    public byte PitchVariance { get; set; }

    [DataField("volume")]
    [ViewVariables(VVAccess.ReadWrite)]
    public byte Volume { get; set; }

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
