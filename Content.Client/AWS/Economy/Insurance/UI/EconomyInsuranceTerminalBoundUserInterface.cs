using Content.Shared.AWS.Economy.Bank;
using Content.Shared.AWS.Economy.Insurance;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed.TypeParsers;
using System.Linq;

namespace Content.Client.AWS.Economy.Insurance.UI;

public sealed class EconomyInsuranceTerminalBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    [ViewVariables]
    private EconomyInsuranceTerminalMenu? _menu;

    [ViewVariables]
    private EconomyInsuranceTerminalRights _insuranceRights = EconomyInsuranceTerminalRights.Its;

    [ViewVariables]
    private Dictionary<int, EconomyInsuranceInfo> _infos = new();

    [ViewVariables]
    private int _insertedInsuranceId = 0;

    [ViewVariables]
    private List<EconomyInsurancePrototype> _insurancePrototypes = new();

    protected override void Open()
    {
        base.Open();

        FetchPrototypes();

        _menu = new(_insurancePrototypes);
        _menu.ConfirmEditInsurance += insuranceInfo => SendMessage(new EconomyInsuranceEditMessage(insuranceInfo));
        _menu.OnClose += Close;

        _menu.OpenCentered();
        _menu.UpdateInfo(_insertedInsuranceId, _insuranceRights, _infos);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is EconomyInsuranceUserInterfaceState insuranceState)
        {
            _infos = insuranceState.Infos;
            _insuranceRights = insuranceState.Rights;
            _insertedInsuranceId = insuranceState.Id;


            _menu?.UpdateInfo(_insertedInsuranceId, _insuranceRights, _infos);

            return;
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _menu?.Dispose();
    }

    private void FetchPrototypes()
    {
        if (_prototype.TryGetInstances<EconomyInsurancePrototype>(out var instances))
            _insurancePrototypes = instances.Values.ToList();
    }
}
