using System.Text;
using Abyss.Persistence.Relational;
using Abyss.Services;
using Abyss.Modules.Abstract;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Disqord.Gateway;
using HumanDateParser;
using Microsoft.EntityFrameworkCore;
using Qmmands;
using Qmmands.Text;

namespace Abyss.Modules.Utility;

public class ReminderModule : AbyssModuleBase
{
    public ReminderModule(ReminderService reminder)
    {
        _reminders = reminder;
    }

    private readonly ReminderService _reminders;

    [SlashCommand("reminders")]
    [Description("Shows all of your reminders.")]
    public async Task<DiscordInteractionResponseCommandResult> RemindersAsync()
    {
        string Map(Reminder r)
        {
            return new StringBuilder()
                .AppendLine($"**{r.Text}** (ID {r.Id})")
                .AppendLine($"- Due {Markdown.Timestamp(r.DueAt, Constants.TIMESTAMP_FORMAT)}")
                .AppendLine($"- Created {Markdown.Timestamp(r.CreatedAt, Constants.TIMESTAMP_FORMAT)}")
                .ToString();
        }

        var gsr = await Database.Reminders.Where(d =>
            d.CreatorId == (ulong)Context.Author.Id && d.ChannelId == (ulong)Context.ChannelId).ToListAsync();
        return Response(
            new LocalEmbed()
                .WithColor(Constants.Theme)
                .WithAuthor("Your reminders in this channel")
                .WithDescription(string.Join("\n", gsr.Select(Map)))
                .WithFooter("Start a reminder with 'remindme 14h {message}'.")
        );
    }

    [SlashCommand("remindme")]
    [Description("Starts a reminder.")]
    public async Task<DiscordInteractionResponseCommandResult> AddPrefixAsync([Name("Time")] [Description("The time to remind you.")]
        string time,
        [Name("Message")] [Description("What to remind you of.")] [Remainder]
        string description)
    {
        try
        {
            var offset = HumanDateParser.HumanDateParser.Parse(time);
            var diff = offset - DateTime.Now;
            if (diff.TotalSeconds <= 0 || diff.TotalMilliseconds > 2147400000)
            {
                return Response(
                    "Invalid time. I can only take times in the future, and not past 3.5 weeks from today.");
            }

            var reminder = await _reminders.CreateReminderAsync(Context.ChannelId,
                Context.Interaction.Id, Context.Author.Id, description, offset);
            return Response(
                new LocalEmbed()
                    .WithColor(Constants.Theme)
                    .WithTitle("Reminder created")
                    .WithDescription(
                        $"I'll remind you at {Markdown.Timestamp(offset, Markdown.TimestampFormat.ShortDateTime)} of \"{description}\" in this channel.")
            );
        }
        catch (ParseException)
        {
            return Response(
                "Invalid time. I can only take times in the future, and not past 3.5 weeks from today.");
        }
    }

    [SlashCommand("remainderdelete")]
    [Description("Deletes a reminder.")]
    public async Task<DiscordInteractionResponseCommandResult> DeletePrefixAsync(
        [Name("Reminder")] [Description("The ID of the reminder to delete.")]
        int reminderId)
    {
        var reminder = await Database.Reminders.SingleOrDefaultAsync(r => r.Id == reminderId);
        if (reminder == null)
        {
            return Response("There's no reminder with that ID.");
        }

        if (reminder.CreatorId != Context.Author.Id /*&& Context.Author.GetPermissions().Has(Permission.ManageGuild)*/)
        {
            return Response("You can't delete reminders that aren't yours.");
        }

        Database.Reminders.Remove(reminder);
        await Database.SaveChangesAsync();
        return Response($"Deleted reminder {reminderId}.");
    }
}