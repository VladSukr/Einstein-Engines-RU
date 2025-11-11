//IH - Start
using System;
using System.Collections.Generic;

namespace Content.Server.Gravity;

/// <summary>
///     Tracks active gravity generator fields on a grid so other systems can query thrust modifiers or FTL locks.
/// </summary>
[RegisterComponent]
public sealed partial class GridGravityWellComponent : Component
{
    private readonly Dictionary<EntityUid, GridGravityWellGeneratorInfo> _generators = new();

    [ViewVariables] public float MassMultiplier { get; private set; } = 1f;
    [ViewVariables] public float ProtectRadius { get; private set; }
    [ViewVariables] public bool BlocksFtl { get; private set; }

    public bool Active => _generators.Count > 0;

    /// <summary>
    ///     Value applied to shuttle thrust / torque to simulate increased mass.
    /// </summary>
    public float ThrustScale => MassMultiplier <= 1f ? 1f : 1f / MassMultiplier;

    public void SetGenerator(EntityUid uid, float massMultiplier, float radius, bool blockFtl)
    {
        _generators[uid] = new GridGravityWellGeneratorInfo(massMultiplier, radius, blockFtl);
        Recalculate();
    }

    public bool RemoveGenerator(EntityUid uid)
    {
        if (!_generators.Remove(uid))
            return false;

        Recalculate();
        return true;
    }

    private void Recalculate()
    {
        if (_generators.Count == 0)
        {
            MassMultiplier = 1f;
            ProtectRadius = 0f;
            BlocksFtl = false;
            return;
        }

        var maxMultiplier = 1f;
        var maxRadius = 0f;
        var blockFtl = false;

        foreach (var entry in _generators.Values)
        {
            maxMultiplier = MathF.Max(maxMultiplier, entry.MassMultiplier);
            maxRadius = MathF.Max(maxRadius, entry.Radius);
            blockFtl |= entry.BlocksFtl;
        }

        MassMultiplier = maxMultiplier;
        ProtectRadius = maxRadius;
        BlocksFtl = blockFtl;
    }
}

public readonly record struct GridGravityWellGeneratorInfo(float MassMultiplier, float Radius, bool BlocksFtl);
//IH - End
