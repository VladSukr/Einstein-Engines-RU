using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.AWS.Economy.Insurance;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class EconomyInsuranceComponent : Component
{
    // ONLY FOR CLIENT
    [ViewVariables(VVAccess.ReadWrite)]
    public EconomyInsuranceIconPrototype Icon { get; set; }

    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public ProtoId<EconomyInsuranceIconPrototype> IconPrototype { get; set; }

    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public int InsuranceInfoId { get; set; } = default!;
}
