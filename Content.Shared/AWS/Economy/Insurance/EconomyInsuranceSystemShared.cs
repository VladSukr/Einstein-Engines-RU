using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.AWS.Economy.Bank;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.PDA;
using Content.Shared.Preferences;
using JetBrains.Annotations;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.AWS.Economy.Insurance;

public abstract class EconomyInsuranceSystemShared : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _userInterfaceSystem = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] private readonly AccessReaderSystem _accessReaderSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EconomyInsuranceComponent, ExaminedEvent>(OnIdCardExamine);

        SubscribeLocalEvent<EconomyInsuranceTerminalComponent, ComponentInit>(OnTerminalComponentInit);
        SubscribeLocalEvent<EconomyInsuranceTerminalComponent, ComponentRemove>(OnTerminalComponentRemove);
        SubscribeLocalEvent<EconomyInsuranceTerminalComponent, EntInsertedIntoContainerMessage>(OnTerminalInsert);
        SubscribeLocalEvent<EconomyInsuranceTerminalComponent, EntRemovedFromContainerMessage>(OnTerminalRemove);
    }

    private void OnTerminalComponentInit(Entity<EconomyInsuranceTerminalComponent> ent, ref ComponentInit args)
    {
        _itemSlotsSystem.AddItemSlot(ent, EconomyInsuranceTerminalComponent.ConsoleCardID, ent.Comp.CardSlot);

        UpdateTerminalUserInterface(ent, null);
    }

    private void OnTerminalComponentRemove(Entity<EconomyInsuranceTerminalComponent> ent, ref ComponentRemove args)
    {
        _itemSlotsSystem.RemoveItemSlot(ent, ent.Comp.CardSlot);
    }

    private void OnTerminalInsert(Entity<EconomyInsuranceTerminalComponent> ent, ref EntInsertedIntoContainerMessage args)
        => UpdateTerminalUserInterface(ent);


    private void OnTerminalRemove(Entity<EconomyInsuranceTerminalComponent> ent, ref EntRemovedFromContainerMessage args)
        => UpdateTerminalUserInterface(ent);

    [PublicAPI]
    public void UpdateTerminalUserInterface(Entity<EconomyInsuranceTerminalComponent> entity, int? defaultTerminalSelect = null)
    {
        if (_netManager.IsServer)
        {
            if (TryGetTerminalInsertedInsurance(entity, out var insuranceEnt))
            {
                RaiseLocalEvent(entity, new EconomyInsuranceTerminalUpdateEvent(insuranceEnt));
                return;
            }
            RaiseLocalEvent(entity, new EconomyInsuranceTerminalUpdateEvent());
        }
    }

    [PublicAPI]
    public bool TryGetTerminalInsertedInsurance(EconomyInsuranceTerminalComponent terminal, [NotNullWhen(true)] out Entity<EconomyInsuranceComponent> ent)
    {

        if (TryComp(terminal.CardSlot.Item, out EconomyInsuranceComponent? insuranceComponent))
        {
            ent = (terminal.CardSlot.Item.Value, insuranceComponent);
            return true;
        }

        ent = default;

        return false;
    }

    private void OnIdCardExamine(Entity<EconomyInsuranceComponent> entity, ref ExaminedEvent args)
    {
        var insuranceId = entity.Comp.InsuranceInfoId;

        args.PushMarkup(Loc.GetString("economy-insurance-component-insuranceid",
            ("insuranceId", insuranceId)));
    }

    public bool CanEditAnyInsurance(Entity<EconomyInsuranceTerminalComponent> ent)
    {
        if (!TryComp<AccessReaderComponent>(ent, out var accessReader) || ent.Comp.CardSlot.Item is not { } idCard)
            return false;

        if (_accessReaderSystem.IsAllowed(idCard, ent.Owner, accessReader))
            return true;

        return false;
    }

    public bool CanOnlyEditInsuranceProto(Entity<EconomyInsuranceTerminalComponent> ent, int id)
    {
        if (!CanEditAnyInsurance(ent) &&
            TryGetTerminalInsertedInsurance(ent, out var insuranceEnt) && insuranceEnt.Comp.InsuranceInfoId == id)
            return true;

        return false;
    }

    [PublicAPI]
    public bool TryGetInsuranceInfo(int id, [NotNullWhen(true)] out EconomyInsuranceInfo? info)
    {
        info = null;

        if (!TryGetServer(out var server))
            return false;

        if (server.Comp.InsuranceInfo.TryGetValue(id, out info))
            return true;

        return false;
    }

    [PublicAPI]
    public bool TryGetInsuranceInfo(EconomyInsuranceComponent comp, [NotNullWhen(true)] out EconomyInsuranceInfo? info)
    {
        return TryGetInsuranceInfo(comp.InsuranceInfoId, out info);
    }


    [PublicAPI]
    public bool TryGetServer([NotNullWhen(true)] out Entity<EconomyInsuranceServerComponent> server)
    {
        if (GetServer() is { Comp: not null } fetchedServer)
        {
            server = fetchedServer;
            return true;
        }

        server = default;
        return false;
    }

    private Entity<EconomyInsuranceServerComponent>? GetServer()
    {
        EntityManager.EntityQueryEnumerator<EconomyInsuranceServerComponent>().MoveNext(out var uid, out var comp);

        return (uid, comp)!;
    }
}
