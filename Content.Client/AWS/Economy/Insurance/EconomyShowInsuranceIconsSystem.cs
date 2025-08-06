using Content.Shared.Overlays;
using Content.Shared.Security.Components;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;
using Content.Client.Overlays;
using Content.Shared.AWS.Economy.Insurance;
using Robust.Shared.Timing;
using Robust.Client.Timing;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;
using Content.Shared.IdentityManagement.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Store;
using Content.Shared.Mobs.Components;
using Content.Shared.Inventory;
using Content.Shared.Access.Components;
using Content.Shared.PDA;
using Content.Shared.Access.Systems;

namespace Content.Client.AWS.Economy.Insurance;

public sealed class EconomyShowInsuranceIconsSystem : EquipmentHudSystem<EconomyShowInsuranceIconsComponent>
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IClientGameTiming _timing = default!;
    [Dependency] private readonly EconomyInsuranceSystem _insurance = default!;
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;

    private readonly TimeSpan _checkTimeRelay = TimeSpan.FromSeconds(3);
    private readonly ProtoId<EconomyInsuranceIconPrototype> _defaultIcon = "NonStatus";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobStateComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent, after: new[] { typeof(ShowJobIconsSystem) });
    }

    private void OnGetStatusIconsEvent(Entity<MobStateComponent> ent, ref GetStatusIconsEvent ev)
    {
        if (!IsActive)
            return;

        var comp = FindAnyCardWithComponent(ent);

        if (comp is not null && TryGetInsuranceIcon(comp, out var icon))
            ev.StatusIcons.Add(icon);
    }

    private EconomyInsuranceComponent? FindAnyCardWithComponent(EntityUid ent)
    {
        if (_accessReader.FindAccessItemsInventory(ent, out var items))
        {
            foreach (var item in items)
            {
                if (TryComp<EconomyInsuranceComponent>(item, out var comp))
                    return comp;

                if (TryComp<PdaComponent>(item, out var pda)
                    && pda.ContainedId is not null
                    && TryComp(pda.ContainedId, out comp))
                    return comp;
            }
        }

        return null;
    }

    private bool TryGetInsuranceIcon(EconomyInsuranceComponent comp, [NotNullWhen(true)] out EconomyInsuranceIconPrototype? icon)
    {
        //icon = null;

        //var curTime = _timing.CurTime;
        //if (comp.NextIconCheck >= curTime)
        //{
        //    icon = comp.Icon;
        //    return true;
        //}

        ////prevent memory leak
        //comp.Icon = null!;

        icon = null;

        if (_prototype.TryIndex(comp.IconPrototype, out var indexedIcon))
        {
            comp.Icon = indexedIcon;

            icon = indexedIcon;

            return true;
        }

        icon = comp.Icon;
        return false;
    }
}
