using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.AWS.Economy.Insurance;

public readonly record struct EconomyInsuranceTerminalUpdateEvent(
    Entity<EconomyInsuranceComponent>? InsertedInsurance);
