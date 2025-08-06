using Content.Shared.AWS.Economy.Bank;

namespace Content.Client.AWS.Economy.Bank.UI;

public sealed class EconomyTerminalBoundUserInterface(EntityUid owner, Enum uiKey)
    : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private EconomyTerminalMenu? _menu;

    public void OnPayPressed(ulong amount, string reason)
    {
        SendMessage(new EconomyTerminalMessage(amount, reason));
    }

    protected override void Open()
    {
        base.Open();

        _menu = new EconomyTerminalMenu(this);
        _menu.OnClose += Close;

        _menu.OpenCentered();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _menu?.Dispose();
    }
}
