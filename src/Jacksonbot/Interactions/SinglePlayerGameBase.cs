using Disqord;
using Disqord.Bot.Commands;

namespace Jacksonbot.Interactions;

public abstract class SinglePlayerGameBase : BotGeneralViewBase
{
    protected readonly ulong PlayerId;
    protected readonly ulong ChannelId;
    protected readonly DateTimeOffset StartTime;
    protected readonly IDiscordCommandContext Context;
    public SinglePlayerGameBase(IDiscordCommandContext context, Action<LocalMessageBase>? templateMessage) : base(templateMessage)
    {
        PlayerId = context.Author.Id;
        StartTime = DateTimeOffset.Now;
        ChannelId = context.ChannelId;
        Context = context;
    }
}