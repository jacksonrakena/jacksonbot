using Disqord;
using Disqord.Bot;
using Serilog;
using Disqord.Rest;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Abyss
{
    /// <summary>
    ///     Represents a request context for an Abyss command request.
    /// </summary>
    public class AbyssCommandContext : DiscordCommandContext, IServiceProvider
    {
        internal AbyssCommandContext(AbyssBot bot, CachedUserMessage message, string prefix) : base(bot, prefix, message)
        {
            Bot = bot;

            if (!Guild.Members.TryGetValue(Bot.CurrentUser.Id, out var botu)) throw new InvalidOperationException("Guild doesn't contain current user.");
            BotMember = botu;
            Channel = (CachedTextChannel) message.Channel;
        }

        /// <summary>
        ///     This context's logger.
        /// </summary>
        public ILogger Logger
        {
            get
            {
                if (_logger == null) _logger = Log.ForContext(new CommandContextEnricher(this));
                return _logger;
            }
        }

        private ILogger? _logger;

        /// <summary>
        ///     The text channel that the command was invoked in.
        /// </summary>
        public new CachedTextChannel Channel { get; }
        
        /// <summary>
        ///     The bot that received the request.
        /// </summary>
        public new AbyssBot Bot { get; }

        /// <summary>
        ///     The member that invoked (called) this request.
        /// </summary>
        public CachedMember Invoker => Member;

        /// <summary>
        ///     The bot user, as a member of the guild in which the request was received.
        /// </summary>
        public CachedMember BotMember { get; }

        /// <summary>
        ///     A boolean indicating whether the request invoker is the current bot application owner.
        /// </summary>
        public bool InvokerIsOwner => Invoker.Id == Bot.CurrentApplication.GetOrDownloadAsync().GetAwaiter().GetResult().Owner.Id;

        /// <summary>
        ///     Converts this current request context into string representation.
        /// </summary>
        /// <remarks>
        ///     The format for this is <code>"{Command.Name} for {Invoker} (ID {Invoker.Id}) in #{Channel.Name} (ID {Channel.Id})/{Guild.Name} (ID {Guild.Id})"</code>.
        /// </remarks>
        /// <returns>String representation of the current request context.</returns>
        public override string ToString() 
        {
            return
                $"{Command.Name} for {Invoker} (ID {Invoker.Id}) in #{Channel.Name} (ID {Channel.Id})/{Guild.Name} (ID {Guild.Id})";
        }

        /// <inheritdoc />
        public object GetService(Type serviceType) => ServiceProvider.GetService(serviceType);

        /// <summary>
        ///     Replies to the current request context with the specified context.
        /// </summary>
        /// <param name="content">The string content to send.</param>
        /// <param name="embed">The embed to send.</param>
        /// <param name="options">Options for this request.</param>
        /// <returns>The message, if sent, otherwise null.</returns>
        public Task<RestUserMessage?> ReplyAsync(string? content = null, LocalEmbedBuilder? embed = null,
            RestRequestOptions? options = null)
        {
            if (!BotMember.GetPermissionsFor(Channel).SendMessages) return Task.FromResult((RestUserMessage?) null);
            return Channel.SendMessageAsync(content, false, embed?.Build(), options);
        }
    }
}