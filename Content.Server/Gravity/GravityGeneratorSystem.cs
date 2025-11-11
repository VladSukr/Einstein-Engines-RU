using System;
using System.Numerics;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.Gravity;
using Robust.Shared.Localization;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Maths;

namespace Content.Server.Gravity;

public sealed class GravityGeneratorSystem : EntitySystem
{
    [Dependency] private readonly GravitySystem _gravitySystem = default!;
    [Dependency] private readonly SharedPointLightSystem _lights = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    private EntityQuery<MapGridComponent> _gridQuery = default!;
    private EntityQuery<PhysicsComponent> _physicsQuery = default!;
    private EntityQuery<GridGravityWellComponent> _gravityWellQuery = default!;

    public override void Initialize()
    {
        base.Initialize();

        _gridQuery = GetEntityQuery<MapGridComponent>();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
        _gravityWellQuery = GetEntityQuery<GridGravityWellComponent>();

        SubscribeLocalEvent<GravityGeneratorComponent, EntParentChangedMessage>(OnParentChanged);
        SubscribeLocalEvent<GravityGeneratorComponent, ChargedMachineActivatedEvent>(OnActivated);
        SubscribeLocalEvent<GravityGeneratorComponent, ChargedMachineDeactivatedEvent>(OnDeactivated);
        SubscribeLocalEvent<GravityGeneratorComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<GravityGeneratorComponent, ExaminedEvent>(OnExamined);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<GravityGeneratorComponent, PowerChargeComponent>();
        while (query.MoveNext(out var uid, out var grav, out var charge))
        {
            if (!_lights.TryGetLight(uid, out var pointLight))
                continue;

            _lights.SetEnabled(uid, charge.Charge > 0, pointLight);
            _lights.SetRadius(uid, MathHelper.Lerp(grav.LightRadiusMin, grav.LightRadiusMax, charge.Charge),
                pointLight);
        }
    }

    private void OnActivated(Entity<GravityGeneratorComponent> ent, ref ChargedMachineActivatedEvent args)
    {
        ent.Comp.GravityActive = true;
        TryEnableField(ent.Owner, ent.Comp);

        if (TryComp<TransformComponent>(ent, out var xform) &&
            TryComp(xform.ParentUid, out GravityComponent? gravity))
        {
            _gravitySystem.EnableGravity(xform.ParentUid, gravity);
        }
    }

    private void OnDeactivated(Entity<GravityGeneratorComponent> ent, ref ChargedMachineDeactivatedEvent args)
    {
        ent.Comp.GravityActive = false;
        DisableField(ent.Owner, ent.Comp);

        if (TryComp<TransformComponent>(ent, out var xform) &&
            TryComp(xform.ParentUid, out GravityComponent? gravity))
        {
            _gravitySystem.RefreshGravity(xform.ParentUid, gravity);
        }
    }

    private void OnShutdown(Entity<GravityGeneratorComponent> ent, ref ComponentShutdown args)
    {
        DisableField(ent.Owner, ent.Comp);
    }

    private void OnParentChanged(EntityUid uid, GravityGeneratorComponent component, ref EntParentChangedMessage args)
    {
        if (component.GravityActive && TryComp(args.OldParent, out GravityComponent? gravity))
        {
            _gravitySystem.RefreshGravity(args.OldParent.Value, gravity);
        }

        if (!component.GravityActive)
            return;

        if (args.OldParent != null && args.OldParent.Value == component.CurrentGrid)
            RemoveFromGrid(args.OldParent.Value, uid, component);

        TryEnableField(uid, component);
    }

    private void TryEnableField(EntityUid generator, GravityGeneratorComponent component)
    {
        if (!component.GravityActive || !TryGetParentGrid(generator, out var gridUid))
            return;

        var well = EnsureComp<GridGravityWellComponent>(gridUid);
        well.SetGenerator(generator, MathF.Max(component.MassMultiplier, 1f), component.ProtectRadius, component.BlocksFtl);
        Dirty(gridUid, well);
        component.CurrentGrid = gridUid;

        if (_physicsQuery.TryGetComponent(gridUid, out var body))
        {
            _physics.SetLinearVelocity(gridUid, Vector2.Zero, body: body);
            _physics.SetAngularVelocity(gridUid, 0f, body: body);
        }
    }

    private void DisableField(EntityUid generator, GravityGeneratorComponent component)
    {
        if (component.CurrentGrid is not { } grid)
            return;

        RemoveFromGrid(grid, generator, component);
    }

    private void RemoveFromGrid(EntityUid gridUid, EntityUid generator, GravityGeneratorComponent component)
    {
        if (!_gravityWellQuery.TryGetComponent(gridUid, out var well))
        {
            component.CurrentGrid = null;
            return;
        }

        if (!well.RemoveGenerator(generator))
            return;

        component.CurrentGrid = null;

        if (!well.Active)
        {
            RemCompDeferred<GridGravityWellComponent>(gridUid);
        }
        else
        {
            Dirty(gridUid, well);
        }
    }

    private bool TryGetParentGrid(EntityUid uid, out EntityUid gridUid)
    {
        gridUid = EntityUid.Invalid;

        if (!TryComp(uid, out TransformComponent? xform))
            return false;

        var parent = xform.ParentUid;

        if (!parent.IsValid())
            return false;

        if (!_gridQuery.HasComponent(parent))
            return false;

        gridUid = parent;
        return true;
    }

    private void OnExamined(Entity<GravityGeneratorComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange || !TryGetParentGrid(ent.Owner, out var gridUid))
            return;

        if (!_physicsQuery.TryGetComponent(gridUid, out var physics))
            return;

        var mass = physics.Mass;
        args.PushMarkup(Loc.GetString("gravity-generator-examine-mass", ("mass", Math.Round(mass, 1))));

        if (_gravityWellQuery.TryGetComponent(gridUid, out var well) && well.Active)
        {
            var state = well.BlocksFtl ? "gravity-generator-examine-ftl-locked" : "gravity-generator-examine-ftl-free";
            args.PushMarkup(Loc.GetString("gravity-generator-examine-ftl", ("state", Loc.GetString(state))));
        }
        else
        {
            args.PushMarkup(Loc.GetString("gravity-generator-examine-ftl", ("state", Loc.GetString("gravity-generator-examine-ftl-free"))));
        }
    }
}
