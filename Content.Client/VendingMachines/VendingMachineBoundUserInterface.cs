using Content.Client.UserInterface.Controls;
using Content.Client.VendingMachines.UI;
using Content.Shared.VendingMachines;
using Robust.Client.UserInterface;
using Robust.Shared.Input;
using System.Linq;
using Content.Shared.Emag.Components;

namespace Content.Client.VendingMachines
{
    public sealed class VendingMachineBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private VendingMachineMenu? _menu;

        [ViewVariables]
        private List<VendingMachineInventoryEntry> _cachedInventory = new();

        [ViewVariables]
        private List<int> _cachedFilteredIndex = new();

        private VendingMachineSystem _vendingMachineSystem;

        public VendingMachineBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
            _vendingMachineSystem = EntMan.System<VendingMachineSystem>();
        }

        protected override void Open()
        {
            base.Open();

            _menu = this.CreateWindow<VendingMachineMenu>();
            _menu.OpenCenteredLeft();
            _menu.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
            _menu.OnItemSelected += OnItemSelected;
            Refresh();
        }

        public void Refresh()
        {
            var system = EntMan.System<VendingMachineSystem>();
            _cachedInventory = system.GetAllInventory(Owner);

            _menu?.Populate(_cachedInventory);
        }

        private void OnItemSelected(GUIBoundKeyEventArgs args, ListData data)
        {
            if (args.Function != EngineKeyFunctions.UIClick)
                return;

            if (data is not VendorItemsListData { ItemIndex: var itemIndex })
                return;

            if (_cachedInventory.Count == 0)
                return;

            var selectedItem = _cachedInventory.ElementAtOrDefault(itemIndex);

            if (selectedItem == null)
                return;

            //SS14-RU
            // SendMessage(new VendingMachineEjectMessage(selectedItem.Type, selectedItem.ID));
            SendMessage(new VendingMachineSelectMessage(selectedItem.Type, selectedItem.ID));
            //SS14-RU
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;

            if (_menu == null)
                return;

            _menu.OnItemSelected -= OnItemSelected;
            _menu.OnClose -= Close;
            _menu.Dispose();
        }

        //SS14-RU
        private VendingMachineInventoryEntry? GetEntry(EntityUid uid, VendingMachineComponent component)
        {
            string selectedId = component.SelectedItemId!;
            if (component.SelectedItemInventoryType == InventoryType.Emagged && EntMan.HasComponent<EmaggedComponent>(uid))
                return component.EmaggedInventory.GetValueOrDefault(selectedId);

            if (component.SelectedItemInventoryType == InventoryType.Contraband && component.Contraband)
                return component.ContrabandInventory.GetValueOrDefault(selectedId);

            return component.Inventory.GetValueOrDefault(selectedId);
        }
        //SS14-RU

        private void OnSearchChanged(string? filter)
        {
            _menu?.Populate(_cachedInventory, out _cachedFilteredIndex, filter);
        }
    }
}
