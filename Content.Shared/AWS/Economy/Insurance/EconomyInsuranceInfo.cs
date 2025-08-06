using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.AWS.Economy.Insurance;

[Serializable, NetSerializable]
public sealed partial class EconomyInsuranceInfo(int id, ProtoId<EconomyInsurancePrototype> insuranceProto, string insurerName, string payerAccountId, string dna)
{
    // you should change this through ingame terminals or API in EconomyInsuranceSystem
    [DataField(readOnly: true)] public int Id { get; set; } = id;
    [DataField] public ProtoId<EconomyInsurancePrototype> InsuranceProto { get; set; } = insuranceProto;
    [DataField] public string InsurerName { get; set; } = insurerName;
    [DataField] public string PayerAccountId { get; set; } = payerAccountId;
    [DataField] public string DNA { get; set; } = dna;
}
