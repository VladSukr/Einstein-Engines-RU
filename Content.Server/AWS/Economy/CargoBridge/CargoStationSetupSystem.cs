using Content.Server.AWS.Economy.Bank;
using Content.Server.Cargo.Components;
using Content.Server.Cargo.Systems;
using Content.Server.Station.Systems;

using Robust.Shared.GameObjects;

namespace Content.Server.AWS.Economy.CargoBridge;

public sealed class CargoStationSetupSystem : EntitySystem
{
    [Dependency] private readonly EconomyBankAccountSystem _economyBankAccount = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<StationInitializedEvent>(OnStationInitialized);
        SubscribeLocalEvent<EconomyThalerCargoComponent, ComponentStartup>(OnCargoComponentStartup);
    }

    private void OnStationInitialized(StationInitializedEvent args)
    {
        if (HasComp<EconomyThalerCargoComponent>(args.Station))
            return;

        var component = EnsureComp<EconomyThalerCargoComponent>(args.Station);
        if (string.IsNullOrWhiteSpace(component.AccountId))
            component.AccountId = "NT-Cargo";
    }

    private void OnCargoComponentStartup(EntityUid uid, EconomyThalerCargoComponent component, ref ComponentStartup args)
    {
        if (string.IsNullOrWhiteSpace(component.AccountId))
            return;

        if (!_economyBankAccount.TryGetAccount(component.AccountId, out var account))
            return;

        if (!TryComp(uid, out StationBankAccountComponent? bank))
            return;

        var newBalance = account.Value.Comp.Balance > int.MaxValue
            ? int.MaxValue
            : (int) account.Value.Comp.Balance;

        if (bank.Balance == newBalance)
            return;

        bank.Balance = newBalance;
        Dirty(uid, bank);
    }
}
