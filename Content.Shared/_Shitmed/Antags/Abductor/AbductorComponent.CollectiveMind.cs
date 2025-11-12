using Content.Shared._Sunrise.CollectiveMind;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Shitmed.Antags.Abductor;

public sealed partial class AbductorComponent
{
    [ValidatePrototypeId<CollectiveMindPrototype>]
    [DataField]
    public string AbductorCollectiveMindProto = "Abductor";
}
