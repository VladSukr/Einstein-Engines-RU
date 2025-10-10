using Content.Server.Players.PlayTimeTracking;
using Content.Shared._EE.Contractors.Prototypes;
using Content.Shared.CCVar;
using Content.Shared.Customization.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.Players;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Configuration;
using Robust.Shared.Log;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Utility;

namespace Content.Server._EE.Contractors.Systems;

public sealed class NationalitySystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;
    [Dependency] private readonly CharacterRequirementsSystem _characterRequirements = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeTracking = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
        SubscribeLocalEvent<LoadProfileExtensionsEvent>(OnProfileLoad);
    }

    // When the player is spawned in, add the nationality components selected during character creation
    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args) =>
        ApplyNationality(args.Mob, args.JobId, args.Profile,
            _playTimeTracking.GetTrackerTimes(args.Player), args.Player.ContentData()?.Whitelisted ?? false);

    private void OnProfileLoad(LoadProfileExtensionsEvent args) =>
        ApplyNationality(args.Mob, args.JobId, args.Profile,
            _playTimeTracking.GetTrackerTimes(args.Player), args.Player.ContentData()?.Whitelisted ?? false);

    /// <summary>
    ///     Adds the nationality selected by a player to an entity.
    /// </summary>
    public void ApplyNationality(EntityUid uid, ProtoId<JobPrototype>? jobId, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes, bool whitelisted)
    {
        if (jobId == null || !_prototype.TryIndex(jobId, out _))
            return;

        var jobPrototypeToUse = _prototype.Index(jobId.Value);

        var nationalityId = profile.Nationality != string.Empty
            ? profile.Nationality
            : SharedHumanoidAppearanceSystem.DefaultNationality;
        var nationality = (ProtoId<NationalityPrototype>) nationalityId;

        if (!_prototype.TryIndex(nationality, out var nationalityPrototype))
        {
            //SS14RU - Start
            var fallbackId = SharedHumanoidAppearanceSystem.DefaultNationality;
            var fallbackNationality = (ProtoId<NationalityPrototype>) fallbackId;

            if (!_prototype.TryIndex(fallbackNationality, out nationalityPrototype))
            {
                DebugTools.Assert($"Default nationality '{fallbackId}' not found!");
                return;
            }

            Log.Warning(
                $"Profile '{profile.Name}' tried to use missing nationality '{nationalityId}'. Falling back to '{fallbackId}'.");

            nationality = fallbackNationality;
            nationalityId = fallbackId;

            if (profile.Nationality != fallbackId)
                profile.Nationality = fallbackId;
            //SS14RU - End
        }

        if (!_characterRequirements.CheckRequirementsValid(
            nationalityPrototype.Requirements,
            jobPrototypeToUse,
            profile, playTimes, whitelisted, nationalityPrototype,
            EntityManager, _prototype, _configuration,
            out _))
            return;

        AddNationality(uid, nationalityPrototype);
    }

    /// <summary>
    ///     Adds a single Nationality Prototype to an Entity.
    /// </summary>
    public void AddNationality(EntityUid uid, NationalityPrototype nationalityPrototype)
    {
        if (!_configuration.GetCVar(CCVars.ContractorsEnabled) ||
            !_configuration.GetCVar(CCVars.ContractorsTraitFunctionsEnabled))
        {
            return;
        }

        foreach (var function in nationalityPrototype.Functions)
            function.OnPlayerSpawn(uid, _componentFactory, EntityManager, _serialization);
    }
}
