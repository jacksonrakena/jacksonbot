using System;
using System.Threading.Tasks;
using Abyssal.Common;
using Disqord;
using Abyss.Persistence.Relational;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;

namespace Abyss.Services;

public class ReminderService
{
    private readonly IActionScheduler _actionScheduler;
    private readonly IServiceProvider _services;
    private readonly DiscordBot _bot;
    
    public ReminderService(IActionScheduler actionScheduler, IServiceProvider services, DiscordBot bot)
    {
        _actionScheduler = actionScheduler;
        _bot = bot;
        _services = services;
    }

    public async Task<Reminder> CreateReminderAsync(ulong guild, ulong channel, ulong message, ulong author, string description, DateTimeOffset due)
    {
        var reminder = new Reminder
        {
            ChannelId = channel,
            CreatorId = author,
            Text = description,
            DueAt = due,
            GuildId = guild,
            MessageId = message,
            CreatedAt = DateTimeOffset.Now
        };
        using var scope = _services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<AbyssDatabaseContext>();
        context.Reminders.Add(reminder);
        await context.SaveChangesAsync();

        ScheduleReminder(reminder);
        return reminder;
    }
        
    public void ScheduleReminder(Reminder reminder)
    {
        var due = reminder.DueAt;
        if ((due - DateTimeOffset.Now).TotalSeconds <= 0) due = DateTimeOffset.Now.AddSeconds(10);
        _actionScheduler.Schedule(due, async () =>
        {
            using var scope = _services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<AbyssDatabaseContext>();
            context.Reminders.Remove(reminder);
            await context.SaveChangesAsync();
            var channel = _bot.GetChannel(reminder.GuildId, reminder.ChannelId) as ITextChannel;
            var member = _bot.GetMember(channel!.GuildId, reminder.CreatorId);
            if (member == null) return;
            var embed = new LocalEmbed()
                .WithColor(Constants.Theme)
                .WithTitle("Reminder")
                .WithDescription(reminder.Text + "\n\n" + $"This reminder was set at {Markdown.Timestamp(reminder.CreatedAt, Markdown.TimestampFormat.ShortDateTime)}.");
                
            await channel.SendMessageAsync(new LocalMessage()
                .WithContent(member.Mention)
                .WithEmbeds(embed)
                .WithReference(new LocalMessageReference().WithChannelId(reminder.ChannelId).WithMessageId(reminder.MessageId).WithGuildId(reminder.GuildId))
            );
        });
    }
}