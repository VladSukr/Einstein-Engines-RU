using Robust.Shared.Utility;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Content.Shared.Roles;

namespace Content.Shared.AWS.Economy.Insurance;

[Prototype("insuranceDefault")]
public sealed partial class EconomyInsuranceDefaultPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public Dictionary<ProtoId<JobPrototype>, ProtoId<EconomyInsurancePrototype>> Presets { get; set; } = default!;
}
