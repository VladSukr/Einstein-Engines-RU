using Content.Shared.AWS.Economy.Bank;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using static Content.Shared.Atmos.Components.GasAnalyzerComponent;

namespace Content.Shared.AWS.Economy.Insurance;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class EconomyInsuranceTerminalComponent : Component
{
    public const string ConsoleCardID = "InsuranceTerminal-IdSlot";

    [DataField("cardSlot"), AutoNetworkedField]
    public ItemSlot CardSlot = new();
}

[Serializable, NetSerializable]
public sealed class EconomyInsuranceUserInterfaceState(int id, EconomyInsuranceTerminalRights rights, Dictionary<int, EconomyInsuranceInfo> infos)
    : BoundUserInterfaceState
{
    public int Id { get; set; } = id;
    public EconomyInsuranceTerminalRights Rights { get; set; } = rights;
    public Dictionary<int, EconomyInsuranceInfo> Infos { get; set; } = infos;
}

[Serializable, NetSerializable]
public sealed class EconomyInsuranceEditMessage(EconomyInsuranceInfo info) : BoundUserInterfaceMessage
{
    public EconomyInsuranceInfo Info = info;
}

[Serializable, NetSerializable]
public sealed class EconomyInsuranceNewInfoMessage() : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public enum EconomyInsuranceTerminalRights
{
    Its, // Only edit it's own insurance and only its type
    Full // Edit any insurance
}

[Serializable, NetSerializable]
public enum EconomyInsuranceTerminalUiKey
{
    Key
}
