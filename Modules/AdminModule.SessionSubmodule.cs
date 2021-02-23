using System;
using System.Threading.Tasks;
using Disqord;
using Humanizer;
using Qmmands;

namespace Lament.Modules
{
    public partial class AdminModule
    {
        [Group("session", "cur", "current")]
        public class SessionSubmodule : LamentModuleBase
        {
            [Command("state", "status", "stat", "info")]
            public async Task Session()
            {
                await ReplyAsync(embed: new LocalEmbedBuilder()
                    .WithColor(Context.Color)
                    .WithDescription(
                        $"Lament v17 session active as **{Context.Bot.CurrentUser}** ({Context.Bot.CurrentUser.Id}), " +
                        $"owned by **{Context.Bot.CurrentApplication.Value.Owner}** ({Context.Bot.CurrentApplication.Value.Owner.Id}) " +
                        $"as part of application **{Context.Bot.CurrentApplication.Value.Name}**")
                    .WithTimestamp(DateTimeOffset.Now)
                    .AddField("Runtime flags", Context.Flags.Humanize())
                    .WithFooter("Session " + Constants.SessionId)
                    .Build());
            }
        }
    }
}