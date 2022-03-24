using System;
using Disqord;
using Disqord.Bot;

namespace Abyss.Interactions;

public abstract class AbyssSinglePlayerGameBase : AbyssViewBase
{
    protected readonly ulong PlayerId;
    protected readonly ulong ChannelId;
    protected readonly DateTimeOffset StartTime;
    public AbyssSinglePlayerGameBase(DiscordCommandContext context, LocalMessage templateMessage) : base(templateMessage)
    {
        PlayerId = context.Author.Id;
        StartTime = DateTimeOffset.Now;
        ChannelId = context.ChannelId;
    }
}