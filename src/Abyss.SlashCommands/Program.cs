// See https://aka.ms/new-console-template for more information

using Abyss;
using Abyss.Interactions.Blackjack;
using Abyss.Interactions.Slots;
using Abyss.Interactions.Trivia;
using Abyss.Persistence.Relational;
using Disqord.Bot;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qmmands;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

await new HostBuilder()
    .ConfigureLogging(logging =>
    {
        logging.AddSimpleConsole();
    }).ConfigureAppConfiguration(appConfigure =>
    {
        appConfigure.AddJsonFile(Constants.CONFIGURATION_FILENAME);
        appConfigure.AddEnvironmentVariables(Constants.ENVIRONMENT_VARIABLE_PREFIX);
    })
    .ConfigureServices((b, s) =>
    {
        s.AddDbContext<AbyssDatabaseContext>((options, ctx) =>
        {
            ctx.UseNpgsql(b.Configuration.GetConnectionString("Database"));
        });
        s.AddScoped<TransactionManager>();
        
        s.AddHostedService<Listener>();
    })
    .ConfigureDiscordBot((ctx, bot) =>
    {
        bot.Token = ctx.Configuration.GetSection("Secrets").GetSection("Discord")["Token"];
        bot.Prefixes = new[] { "a." };
    })
    .RunConsoleAsync();

public class Listener : IHostedService
{
    public DiscordBotBase Bot { get; }
    public Listener(DiscordBotBase b)
    {
        Bot = b;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Bot.Ready += BotOnReady;
    }

    private async ValueTask BotOnReady(object? sender, ReadyEventArgs e)
    {
        await Bot.InitializeApplicationCommands(CancellationToken.None);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }
}

public class AbyssModuleBase : DiscordApplicationModuleBase
{
    protected AbyssDatabaseContext Database => Context.Services.GetRequiredService<AbyssDatabaseContext>();
}

public class ExampleModule : AbyssModuleBase
{
    [SlashCommand("trivia")]
    [Description("Play some trivia!")]
    public async ValueTask<DiscordMenuCommandResult> Ping()
    {
        return View(new TriviaGame(await TriviaData.GetCategoriesAsync(), Context));
    }

    [SlashCommand("blackjack")]
    [Description("Play blackjack against the house, and bet for coins.")]
    public async Task<IDiscordCommandResult> Blackjack(int bet)
    {
        if (bet <= 0) return Response("You have to bet a real number.");
        var account = await Database.GetUserAccountAsync(Context.Author.Id);
        if (bet > account.Coins) return Response("You don't have enough coins.");
        return View(new BlackjackGame(bet, Context));
    }

    [SlashCommand("slots")]
    [Description("Play slots and lose all your money.")]
    public async ValueTask<DiscordMenuCommandResult> Slots(int bet)
    {
        return View(new SlotsGame(bet, Context));
    }
}