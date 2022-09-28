using System;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Interaction;

namespace Abyss.Interactions;

public abstract class AbyssSinglePlayerGameBase : AbyssViewBase
{
    protected readonly ulong PlayerId;
    protected readonly ulong ChannelId;
    protected readonly DateTimeOffset StartTime;
    protected readonly IDiscordCommandContext Context;
    public AbyssSinglePlayerGameBase(IDiscordCommandContext context, Action<LocalMessageBase>? templateMessage) : base(templateMessage)
    {
        PlayerId = context.Author.Id;
        StartTime = DateTimeOffset.Now;
        ChannelId = context.ChannelId;
        Context = context;
    }
}