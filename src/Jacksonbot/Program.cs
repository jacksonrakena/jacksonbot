// See https://aka.ms/new-console-template for more information

using Abyssal.Common;
using AbyssalSpotify;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Jacksonbot.Persistence.Relational.Contexts;
using Jacksonbot.Persistence.Relational.Transactions;
using Jacksonbot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var host = new HostBuilder()
    .ConfigureLogging((host, logging) =>
    {
        logging.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
        });
        logging.AddConfiguration(host.Configuration.GetSection("Logging"));
    }).ConfigureAppConfiguration(appConfigure =>
    {
        appConfigure.AddJsonFile("abyss.appsettings.json", optional: true);
        appConfigure.AddJsonFile("jacksonbot.appsettings.json", optional: true);
        appConfigure.AddEnvironmentVariables("ABYSS_");
        appConfigure.AddEnvironmentVariables("JACKSONBOT_");
    })
    .ConfigureServices((b, s) =>
    {
        s.AddDbContext<BotDatabaseContext>((options, ctx) =>
        {
            ctx.UseNpgsql(b.Configuration.GetConnectionString("Database"));
        });
        s.AddSingleton<IActionScheduler, ActionScheduler>();
        s.AddSingleton<ReminderService>();
        s.AddScoped<TransactionManager>();
        s.AddSingleton(SpotifyClient.FromClientCredentials(
                b.Configuration.GetSection("Secrets").GetSection("Spotify")["ClientId"],
                b.Configuration.GetSection("Secrets").GetSection("Spotify")["ClientSecret"]
            )
        );
    })
    .ConfigureDiscordBot((ctx, bot) =>
    {
        bot.Token = ctx.Configuration.GetSection("Secrets").GetSection("Discord")["Token"];
        bot.Intents = GatewayIntents.MessageContent;
        bot.Prefixes = new[] { "ad." };

    })
    .Build();

using var scope = host.Services.CreateScope();
await using (var ctx = scope.ServiceProvider.GetRequiredService<BotDatabaseContext>())
{
    await ctx.Database.MigrateAsync();
    await ctx.Database.EnsureCreatedAsync();
}

await host.RunAsync();
