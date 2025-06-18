using Robust.Shared.GameStates;

namespace Content.Shared.AWS.Economy.Insurance;

[RegisterComponent, NetworkedComponent]
public sealed partial class EconomyInsuranceConsoleComponent : Component
{
    [DataField, ViewVariables]
    bool ViewOnly { get; set; } = true;
}