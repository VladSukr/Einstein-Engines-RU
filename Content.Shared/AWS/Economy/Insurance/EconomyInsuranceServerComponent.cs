using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.AWS.Economy.Insurance;

[RegisterComponent]
public sealed partial class EconomyInsuranceServerComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<int, EconomyInsuranceInfo> InsuranceInfo { get; set; } = new();
}
