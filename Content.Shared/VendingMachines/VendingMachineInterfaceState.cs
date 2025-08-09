using Robust.Shared.Serialization;

namespace Content.Shared.VendingMachines
{
    [Serializable, NetSerializable]
    public sealed class VendingMachineEjectMessage : BoundUserInterfaceMessage
    {
        public readonly InventoryType Type;
        public readonly string ID;
        public VendingMachineEjectMessage(InventoryType type, string id)
        {
            Type = type;
            ID = id;
        }
    }

    //SS14-RU
    [Serializable, NetSerializable]
    public sealed class VendingMachineSelectMessage : BoundUserInterfaceMessage
    {
        public readonly InventoryType Type;
        public readonly string ID;
        public VendingMachineSelectMessage(InventoryType type, string id)
        {
            Type = type;
            ID = id;
        }
    }
    //SS14-RU

    [Serializable, NetSerializable]
    public enum VendingMachineUiKey
    {
        Key,
    }
}
