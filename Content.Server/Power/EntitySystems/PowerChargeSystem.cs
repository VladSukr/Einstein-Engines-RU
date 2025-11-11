using Content.Server.Administration.Logs;
using Content.Server.Audio;
using Content.Server.Gravity;
using Content.Server.Power.Components;
using Content.Shared.Database;
using Content.Shared.Power;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Physics.Components;

namespace Content.Server.Power.EntitySystems;

public sealed class PowerChargeSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly AmbientSoundSystem _ambientSoundSystem = default!;
    private EntityQuery<PhysicsComponent> _physicsQuery = default!;
    private EntityQuery<TransformComponent> _xformQuery = default!;
    private EntityQuery<GridGravityWellComponent> _gravityWellQuery = default!;

    public override void Initialize()
    {
        base.Initialize();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
        _xformQuery = GetEntityQuery<TransformComponent>();
        _gravityWellQuery = GetEntityQuery<GridGravityWellComponent>();
        SubscribeLocalEvent<PowerChargeComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<PowerChargeComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<PowerChargeComponent, ActivatableUIOpenAttemptEvent>(OnUIOpenAttempt);
        SubscribeLocalEvent<PowerChargeComponent, AfterActivatableUIOpenEvent>(OnAfterUiOpened);
        SubscribeLocalEvent<PowerChargeComponent, AnchorStateChangedEvent>(OnAnchorStateChange);

        // This needs to be ui key agnostic
        SubscribeLocalEvent<PowerChargeComponent, SwitchChargingMachineMessage>(OnSwitchGenerator);
    }

    private void OnAnchorStateChange(EntityUid uid, PowerChargeComponent component, AnchorStateChangedEvent args)
    {
        if (args.Anchored || !TryComp<ApcPowerReceiverComponent>(uid, out var powerReceiverComponent))
            return;

        component.Active = false;
        component.Charge = 0;
        UpdateState(new Entity<PowerChargeComponent, ApcPowerReceiverComponent>(uid, component, powerReceiverComponent));
    }

    private void OnAfterUiOpened(EntityUid uid, PowerChargeComponent component, AfterActivatableUIOpenEvent args)
    {
        if (!TryComp<ApcPowerReceiverComponent>(uid, out var apcPowerReceiver))
            return;

        UpdateUI((uid, component, apcPowerReceiver), component.ChargeRate);
    }

    private void OnSwitchGenerator(EntityUid uid, PowerChargeComponent component, SwitchChargingMachineMessage args)
    {
        SetSwitchedOn(uid, component, args.On, user: args.Actor);
    }

    private void OnUIOpenAttempt(EntityUid uid, PowerChargeComponent component, ActivatableUIOpenAttemptEvent args)
    {
        if (!component.Intact)
            args.Cancel();
    }

    private void OnComponentShutdown(EntityUid uid, PowerChargeComponent component, ComponentShutdown args)
    {
        if (!component.Active)
            return;

        component.Active = false;

        var eventArgs = new ChargedMachineDeactivatedEvent();
        RaiseLocalEvent(uid, ref eventArgs);
    }

    private void OnMapInit(Entity<PowerChargeComponent> ent, ref MapInitEvent args)
    {
        ApcPowerReceiverComponent? powerReceiver = null;
        if (!Resolve(ent, ref powerReceiver, false))
            return;

        UpdatePowerState(ent, powerReceiver);
        UpdateState((ent, ent.Comp, powerReceiver));
    }

    private void SetSwitchedOn(EntityUid uid, PowerChargeComponent component, bool on,
        ApcPowerReceiverComponent? powerReceiver = null, EntityUid? user = null)
    {
        if (!Resolve(uid, ref powerReceiver))
            return;

        if (user is { } )
            _adminLogger.Add(LogType.Action, on ? LogImpact.Medium : LogImpact.High, $"{ToPrettyString(user):player} set ${ToPrettyString(uid):target} to {(on ? "on" : "off")}");

        component.SwitchedOn = on;
        UpdatePowerState(component, powerReceiver);
        component.NeedUIUpdate = true;
    }

    private static void UpdatePowerState(PowerChargeComponent component, ApcPowerReceiverComponent powerReceiver)
    {
        powerReceiver.Load = component.SwitchedOn ? component.ActivePowerUse : component.IdlePowerUse;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<PowerChargeComponent, ApcPowerReceiverComponent>();
        while (query.MoveNext(out var uid, out var chargingMachine, out var powerReceiver))
        {
            var ent = (uid, gravGen: chargingMachine, powerReceiver);
            if (!chargingMachine.Intact)
                continue;

            var statusChanged = RefreshGravityGeneratorStatus(uid, chargingMachine);

            // Calculate charge rate based on power state and such.
            // Negative charge rate means discharging.
            float chargeRate;
            if (chargingMachine.SwitchedOn)
            {
                if (powerReceiver.Powered)
                {
                    chargeRate = chargingMachine.ChargeRate;
                }
                else
                {
                    // Scale discharge rate such that if we're at 25% active power we discharge at 75% rate.
                    var receiving = powerReceiver.PowerReceived;
                    var mainSystemPower = Math.Max(0, receiving - chargingMachine.IdlePowerUse);
                    var ratio = 1 - mainSystemPower / (chargingMachine.ActivePowerUse - chargingMachine.IdlePowerUse);
                    chargeRate = -(ratio * chargingMachine.ChargeRate);
                }
            }
            else
            {
                chargeRate = -chargingMachine.ChargeRate;
            }

            var active = chargingMachine.Active;
            var lastCharge = chargingMachine.Charge;
            chargingMachine.Charge = Math.Clamp(chargingMachine.Charge + frameTime * chargeRate, 0, chargingMachine.MaxCharge);
            if (chargeRate > 0)
            {
                // Charging.
                if (MathHelper.CloseTo(chargingMachine.Charge, chargingMachine.MaxCharge) && !chargingMachine.Active)
                {
                    chargingMachine.Active = true;
                }
            }
            else
            {
                // Discharging
                if (MathHelper.CloseTo(chargingMachine.Charge, 0) && chargingMachine.Active)
                {
                    chargingMachine.Active = false;
                }
            }

            var updateUI = chargingMachine.NeedUIUpdate || statusChanged;
            if (!MathHelper.CloseTo(lastCharge, chargingMachine.Charge))
            {
                UpdateState(ent);
                updateUI = true;
            }

            if (updateUI)
                UpdateUI(ent, chargeRate);

            if (active == chargingMachine.Active)
                continue;

            if (chargingMachine.Active)
            {
                var eventArgs = new ChargedMachineActivatedEvent();
                RaiseLocalEvent(uid, ref eventArgs);
            }
            else
            {
                var eventArgs = new ChargedMachineDeactivatedEvent();
                RaiseLocalEvent(uid, ref eventArgs);
            }
        }
    }

    private void UpdateUI(Entity<PowerChargeComponent, ApcPowerReceiverComponent> ent, float chargeRate)
    {
        var (_, component, powerReceiver) = ent;
        if (!_uiSystem.IsUiOpen(ent.Owner, component.UiKey))
            return;

        var chargeTarget = chargeRate < 0 ? 0 : component.MaxCharge;
        short chargeEta;
        var atTarget = false;
        if (MathHelper.CloseTo(component.Charge, chargeTarget))
        {
            chargeEta = short.MinValue; // N/A
            atTarget = true;
        }
        else
        {
            var diff = chargeTarget - component.Charge;
            chargeEta = (short) Math.Abs(diff / chargeRate);
        }

        var status = chargeRate switch
        {
            > 0 when atTarget => PowerChargePowerStatus.FullyCharged,
            < 0 when atTarget => PowerChargePowerStatus.Off,
            > 0 => PowerChargePowerStatus.Charging,
            < 0 => PowerChargePowerStatus.Discharging,
            _ => throw new ArgumentOutOfRangeException()
        };

        var (hasStationStatus, stationMass, stationFtlLocked) = GetGravityGeneratorStatus(ent.Owner, component);

        var state = new PowerChargeState(
            component.SwitchedOn,
            (byte) (component.Charge * 255),
            status,
            (short) Math.Round(powerReceiver.PowerReceived),
            (short) Math.Round(powerReceiver.Load),
            chargeEta,
            hasStationStatus,
            stationMass,
            stationFtlLocked
        );

        _uiSystem.SetUiState(
            ent.Owner,
            component.UiKey,
            state);

        component.NeedUIUpdate = false;
    }

    private void UpdateState(Entity<PowerChargeComponent, ApcPowerReceiverComponent> ent)
    {
        var (uid, machine, powerReceiver) = ent;
        var appearance = EntityManager.GetComponentOrNull<AppearanceComponent>(uid);
        _appearance.SetData(uid, PowerChargeVisuals.Charge, machine.Charge, appearance);
        _appearance.SetData(uid, PowerChargeVisuals.Active, machine.Active);


        if (!machine.Intact)
        {
            MakeBroken((uid, machine), appearance);
        }
        else if (powerReceiver.PowerReceived < machine.IdlePowerUse)
        {
            MakeUnpowered((uid, machine), appearance);
        }
        else if (!machine.SwitchedOn)
        {
            MakeOff((uid, machine), appearance);
        }
        else
        {
            MakeOn((uid, machine), appearance);
        }
    }

    private void MakeBroken(Entity<PowerChargeComponent> ent, AppearanceComponent? appearance)
    {
        _ambientSoundSystem.SetAmbience(ent, false);

        _appearance.SetData(ent, PowerChargeVisuals.State, PowerChargeStatus.Broken, appearance);
    }

    private void MakeUnpowered(Entity<PowerChargeComponent> ent, AppearanceComponent? appearance)
    {
        _ambientSoundSystem.SetAmbience(ent, false);

        _appearance.SetData(ent, PowerChargeVisuals.State, PowerChargeStatus.Unpowered, appearance);
    }

    private void MakeOff(Entity<PowerChargeComponent> ent, AppearanceComponent? appearance)
    {
        _ambientSoundSystem.SetAmbience(ent, false);

        _appearance.SetData(ent, PowerChargeVisuals.State, PowerChargeStatus.Off, appearance);
    }

    private void MakeOn(Entity<PowerChargeComponent> ent, AppearanceComponent? appearance)
    {
        _ambientSoundSystem.SetAmbience(ent, true);

        _appearance.SetData(ent, PowerChargeVisuals.State, PowerChargeStatus.On, appearance);
    }


    private (bool showStatus, float mass, bool ftlLocked) GetGravityGeneratorStatus(EntityUid uid, PowerChargeComponent component)
        => (component.HasStationStatus, component.StationMassCache, component.StationFtlLockedCache);

    private bool RefreshGravityGeneratorStatus(EntityUid uid, PowerChargeComponent component)
    {
        if (!HasComp<GravityGeneratorComponent>(uid))
        {
            if (!component.HasStationStatus)
                return false;

            component.HasStationStatus = false;
            component.StationMassCache = 0f;
            component.StationFtlLockedCache = false;
            return true;
        }

        if (!_xformQuery.TryGetComponent(uid, out var xform))
            return false;

        var parent = xform.ParentUid;
        if (!_physicsQuery.TryGetComponent(parent, out var physics))
            return false;

        var locked = _gravityWellQuery.TryGetComponent(parent, out var well) && well.BlocksFtl;
        var mass = physics.Mass;

        var changed = !component.HasStationStatus
                      || !MathHelper.CloseTo(component.StationMassCache, mass)
                      || component.StationFtlLockedCache != locked;

        component.HasStationStatus = true;
        component.StationMassCache = mass;
        component.StationFtlLockedCache = locked;
        return changed;
    }
}

[ByRefEvent] public record struct ChargedMachineActivatedEvent;
[ByRefEvent] public record struct ChargedMachineDeactivatedEvent;
