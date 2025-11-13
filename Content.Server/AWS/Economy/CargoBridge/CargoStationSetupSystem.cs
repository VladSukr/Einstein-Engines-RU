using Content.Server.AWS.Economy.Bank;
using Content.Server.Cargo.Components;
using Content.Server.Cargo.Systems;
using Content.Server.Station.Systems;

using Robust.Shared.GameObjects;

namespace Content.Server.AWS.Economy.CargoBridge;

public sealed class CargoStationSetupSystem : EntitySystem
{
    [Dependency] private readonly EconomyBankAccountSystem _economyBankAccount = default!;
    [Dependency] private readonly CargoSystem _cargoSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<StationInitializedEvent>(OnStationInitialized);
    }

    private void OnStationInitialized(StationInitializedEvent args)
    {
        StationInitialized(args.Station);
    }

    private void StationInitialized(EntityUid station)
    {
        var component = EnsureComp<EconomyThalerCargoComponent>(station);
        if (string.IsNullOrWhiteSpace(component.AccountId))
            component.AccountId = "NT-Cargo";

        if (!_economyBankAccount.TryGetAccount(component.AccountId, out var account))
            return;

        if (!TryComp(station, out StationBankAccountComponent? bank))
            return;

        var newBalance = account.Value.Comp.Balance > int.MaxValue
            ? int.MaxValue
            : (int) account.Value.Comp.Balance;

        if (bank.Balance == newBalance)
            return;

        var delta = newBalance - bank.Balance;
        _cargoSystem.UpdateBankAccount(station, bank, delta);
    }
}
