using System;
using System.Numerics;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.Gravity;
using Robust.Shared.Localization;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Maths;

namespace Content.Server.Gravity;

public sealed class GravityGeneratorSystem : EntitySystem
{
    [Dependency] private readonly GravitySystem _gravitySystem = default!;
    [Dependency] private readonly SharedPointLightSystem _lights = default!;
    //IH - Start
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    private EntityQuery<MapGridComponent> _gridQuery = default!;
    private EntityQuery<PhysicsComponent> _physicsQuery = default!;
    private EntityQuery<GridGravityWellComponent> _gravityWellQuery = default!;
    private EntityQuery<FixturesComponent> _fixturesQuery = default!;
    //IH - End

    public override void Initialize()
    {
        base.Initialize();

        //IH - Start
        _gridQuery = GetEntityQuery<MapGridComponent>();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
        _gravityWellQuery = GetEntityQuery<GridGravityWellComponent>();
        _fixturesQuery = GetEntityQuery<FixturesComponent>();
        //IH - End

        SubscribeLocalEvent<GravityGeneratorComponent, EntParentChangedMessage>(OnParentChanged);
        SubscribeLocalEvent<GravityGeneratorComponent, ChargedMachineActivatedEvent>(OnActivated);
        SubscribeLocalEvent<GravityGeneratorComponent, ChargedMachineDeactivatedEvent>(OnDeactivated);
        //IH - Start
        SubscribeLocalEvent<GravityGeneratorComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<GravityGeneratorComponent, ExaminedEvent>(OnExamined);
        //IH - End
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<GravityGeneratorComponent, PowerChargeComponent>();
        while (query.MoveNext(out var uid, out var grav, out var charge))
        {
            var switchedOn = charge.SwitchedOn;

            if (!switchedOn && grav.CurrentGrid is { } offGrid && !grav.GravityActive)
            {
                DisableField(uid, grav);
            }
            else if (switchedOn && grav.CurrentGrid is null)
            {
                TryEnableField(uid, grav);
            }

            if (!_lights.TryGetLight(uid, out var pointLight))
                continue;

            _lights.SetEnabled(uid, charge.Charge > 0, pointLight);
            _lights.SetRadius(uid, MathHelper.Lerp(grav.LightRadiusMin, grav.LightRadiusMax, charge.Charge),
                pointLight);

            if (grav.CurrentGrid is { } grid && _gravityWellQuery.TryGetComponent(grid, out var well))
            {
                var effectiveMultiplier = GetEffectiveMassMultiplier(uid, grav, charge);
                if (well.TryUpdateGeneratorMultiplier(uid, effectiveMultiplier))
                {
                    ApplyGridEffects(grid, well);
                }
            }
        }
    }

    private void OnActivated(Entity<GravityGeneratorComponent> ent, ref ChargedMachineActivatedEvent args)
    {
        ent.Comp.GravityActive = true;
        //IH - Start
        TryEnableField(ent.Owner, ent.Comp);
        //IH - End

        if (TryComp<TransformComponent>(ent, out var xform) &&
            TryComp(xform.ParentUid, out GravityComponent? gravity))
        {
            _gravitySystem.EnableGravity(xform.ParentUid, gravity);
        }
    }

    private void OnDeactivated(Entity<GravityGeneratorComponent> ent, ref ChargedMachineDeactivatedEvent args)
    {
        ent.Comp.GravityActive = false;
        //IH - Start
        DisableField(ent.Owner, ent.Comp);
        //IH - End

        if (TryComp<TransformComponent>(ent, out var xform) &&
            TryComp(xform.ParentUid, out GravityComponent? gravity))
        {
            _gravitySystem.RefreshGravity(xform.ParentUid, gravity);
        }
    }

    //IH - Start
    private void OnShutdown(Entity<GravityGeneratorComponent> ent, ref ComponentShutdown args)
    {
        DisableField(ent.Owner, ent.Comp);
    }
    //IH - End

    private void OnParentChanged(EntityUid uid, GravityGeneratorComponent component, ref EntParentChangedMessage args)
    {
        if (component.GravityActive && TryComp(args.OldParent, out GravityComponent? gravity))
        {
            _gravitySystem.RefreshGravity(args.OldParent.Value, gravity);
        }

        //IH - Start
        if (!component.GravityActive)
            return;

        if (args.OldParent != null && args.OldParent.Value == component.CurrentGrid)
            RemoveFromGrid(args.OldParent.Value, uid, component);

        TryEnableField(uid, component);
        //IH - End
    }

    //IH - Start
    private void TryEnableField(EntityUid generator, GravityGeneratorComponent component)
    {
        if (!TryGetParentGrid(generator, out var gridUid))
            return;

        var well = EnsureComp<GridGravityWellComponent>(gridUid);
        var effectiveMultiplier = GetEffectiveMassMultiplier(generator, component);
        var blocksFtl = component.GravityActive && component.BlocksFtl;
        well.SetGenerator(generator, effectiveMultiplier, component.ProtectRadius, blocksFtl);
        ApplyGridEffects(gridUid, well);
        component.CurrentGrid = gridUid;

        if (_physicsQuery.TryGetComponent(gridUid, out var body))
        {
            _physics.SetLinearVelocity(gridUid, Vector2.Zero, body: body);
            _physics.SetAngularVelocity(gridUid, 0f, body: body);
        }
    }
    //IH - End

    //IH - Start
    private void DisableField(EntityUid generator, GravityGeneratorComponent component)
    {
        if (component.CurrentGrid is not { } grid)
            return;

        RemoveFromGrid(grid, generator, component);
    }
    //IH - End

    //IH - Start
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

        ApplyGridEffects(gridUid, well);
        if (!well.Active)
            RemCompDeferred<GridGravityWellComponent>(gridUid);
    }
    //IH - End

    //IH - Start
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
    //IH - End

    //IH - Start
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
    //IH - End

    //IH - Start
    private void ApplyGridEffects(EntityUid gridUid, GridGravityWellComponent well)
    {
        UpdateGridMass(gridUid, well);
        Dirty(gridUid, well);
    }

    private void UpdateGridMass(EntityUid gridUid, GridGravityWellComponent well)
    {
        if (!_fixturesQuery.TryGetComponent(gridUid, out var fixtures))
        {
            well.ClearBaseDensities();
            return;
        }

        var targetMultiplier = MathF.Max(1f, well.MassMultiplier);

        if (MathHelper.CloseTo(targetMultiplier, 1f))
        {
            if (!MathHelper.CloseTo(well.AppliedMassMultiplier, 1f))
            {
                foreach (var (id, fixture) in fixtures.Fixtures)
                {
                    if (!well.TryGetBaseDensity(id, out var baseDensity))
                        continue;

                    _physics.SetDensity(gridUid, id, fixture, baseDensity, manager: fixtures);
                }
            }

            well.ClearBaseDensities();
            return;
        }

        foreach (var (id, fixture) in fixtures.Fixtures)
        {
            if (!well.TryGetBaseDensity(id, out var baseDensity))
            {
                baseDensity = fixture.Density;
                well.RememberBaseDensity(id, baseDensity);
            }

            _physics.SetDensity(gridUid, id, fixture, baseDensity * targetMultiplier, manager: fixtures);
        }

        well.AppliedMassMultiplier = targetMultiplier;
    }
    //IH - End

    //IH - Start
    private float GetEffectiveMassMultiplier(EntityUid generator, GravityGeneratorComponent component, PowerChargeComponent? chargeComp = null)
    {
        var maxMultiplier = MathF.Max(component.MassMultiplier, 1f);
        var chargeLevel = chargeComp?.Charge ??
            (TryComp(generator, out PowerChargeComponent? storedCharge) ? storedCharge.Charge : 1f);

        chargeLevel = Math.Clamp(chargeLevel, 0f, 1f);
        return MathHelper.Lerp(1f, maxMultiplier, chargeLevel);
    }
    //IH - End
}
