using System;
using System.Threading.Tasks;
using Abyssal.Common;
using Disqord;
using Lament.Discord;
using Lament.Persistence.Relational;
using Microsoft.Extensions.DependencyInjection;

namespace Lament.Services
{
    public class ReminderService
    {
        private readonly IActionScheduler _actionScheduler;
        private readonly IServiceProvider _services;
        private readonly LamentDiscordBot _bot;
    
        public ReminderService(IActionScheduler actionScheduler, IServiceProvider services, LamentDiscordBot bot)
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
            using var context = scope.ServiceProvider.GetRequiredService<LamentPersistenceContext>();
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
                await using var context = scope.ServiceProvider.GetRequiredService<LamentPersistenceContext>();
                context.Reminders.Remove(reminder);
                await context.SaveChangesAsync();
                var channel = _bot.GetGuildChannel(reminder.ChannelId) as CachedTextChannel;
                var member = channel.Guild.GetMember(reminder.CreatorId);
                if (member == null) return;
                var embed = new LocalEmbedBuilder().WithColor(Color.LightPink).WithTitle("Reminder").WithDescription(reminder.Description + "\n\n" + Markdown.Link("Click to jump!", Discord.MessageJumpLink(reminder.GuildId, reminder.ChannelId, reminder.MessageId)));
                embed.WithFooter("Reminder set at");
                embed.WithTimestamp(reminder.CreatedAt);
                await channel.SendMessageAsync(member.Mention, embed: embed.Build(), mentions: new LocalMentionsBuilder().WithUserIds(reminder.Creator).Build());
            });
        }
    }
}