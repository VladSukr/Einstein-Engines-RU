using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Shared._Sunrise.CollectiveMind;
using Content.Shared.Chat;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;


namespace Content.Client.UserInterface.Systems.Chat.Controls;

public sealed class ChannelSelectorButton : ChatPopupButton<ChannelSelectorPopup>
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    public event Action<ChatSelectChannel>? OnChannelSelect;

    public ChatSelectChannel SelectedChannel { get; private set; }

    private const int SelectorDropdownOffset = 38;

    public ChannelSelectorButton()
    {
        IoCManager.InjectDependencies(this);
        Name = "ChannelSelector";
        StyleBoxOverride = new StyleBoxEmpty();

        Popup.Selected += OnChannelSelected;

        if (Popup.FirstChannel is { } firstSelector)
        {
            Select(firstSelector);
        }
    }

    protected override UIBox2 GetPopupPosition()
    {
        var globalLeft = GlobalPosition.X;
        var globalBot = GlobalPosition.Y + Height;
        return UIBox2.FromDimensions(
            new Vector2(globalLeft, globalBot),
            new Vector2(SizeBox.Width, SelectorDropdownOffset));
    }

    private void OnChannelSelected(ChatSelectChannel channel)
    {
        Select(channel);
    }

    public void Select(ChatSelectChannel channel)
    {
        if (Popup.Visible)
        {
            Popup.Close();
        }

        if (SelectedChannel == channel)
            return;
        SelectedChannel = channel;
        OnChannelSelect?.Invoke(channel);
    }

    public static string ChannelSelectorName(ChatSelectChannel channel)
    {
        return Loc.GetString($"hud-chatbox-select-channel-{channel}");
    }

    public Color ChannelSelectColor(ChatSelectChannel channel)
    {
        return channel switch
        {
            ChatSelectChannel.Radio => Color.LimeGreen,
            ChatSelectChannel.LOOC => Color.MediumTurquoise,
            ChatSelectChannel.OOC => Color.LightSkyBlue,
            ChatSelectChannel.Dead => Color.MediumPurple,
            ChatSelectChannel.Admin => Color.HotPink,
            ChatSelectChannel.CollectiveMind => Color.LightGreen,
            ChatSelectChannel.Telepathic => Color.PaleVioletRed, //Nyano - Summary: determines the color for the chat.
            _ => Color.DarkGray
        };
    }

    public void UpdateChannelSelectButton(ChatSelectChannel channel, Shared.Radio.RadioChannelPrototype? radio, CollectiveMindPrototype? collectiveMind = null)
    {
        string text;
        Color color;

        if (collectiveMind != null)
        {
            text = Loc.GetString(collectiveMind.Name);
            color = collectiveMind.Color;
        }
        else if (radio != null)
        {
            text = Loc.GetString(radio.Name);
            color = radio.Color;
        }
        else
        {
            text = ChannelSelectorName(channel);
            color = ChannelSelectColor(channel);
        }

        Text = $"[{text}]";
        Modulate = color;
    }
}
