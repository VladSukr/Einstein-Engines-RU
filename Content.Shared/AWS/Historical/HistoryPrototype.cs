using Robust.Shared.Prototypes;
using Content.Shared.AWS.Skills;
using Content.Shared.Roles;

namespace Content.Shared.AWS.Historical;

[Prototype("history")]
public sealed partial class HistoryPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = string.Empty;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public bool IsDefault { get; set; } = false;

    [ViewVariables(VVAccess.ReadWrite), DataField(required: true)]
    public HistoryType HistoryType { get; private set; } = HistoryType.Culture;

    [ViewVariables(VVAccess.ReadWrite), DataField("name", required: true)]
    public string Name { get; private set; } = string.Empty;

    [ViewVariables(VVAccess.ReadWrite), DataField("description", required: true)]
    public string Description { get; private set; } = string.Empty;

    [ViewVariables(VVAccess.ReadWrite), DataField("blockedForSpecies")]
    public ProtoId<SkillCategoryPrototype>? BlockedForSpecies = null;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public List<ProtoId<HistoryPrototype>> BlockingHistories = new();

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public List<ProtoId<JobPrototype>> BlockingJobs = new();

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public SkillContainer Container { get; private set; } = new();
}

public enum HistoryType
{
    Culture,
    Lifestyle,
    Faction,
}
