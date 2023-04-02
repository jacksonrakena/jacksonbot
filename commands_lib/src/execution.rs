use crate::command::CommandRegistration;
use crate::registry::{CommandMap, CommandRegistry};
use serenity::builder::CreateEmbed;
use serenity::model::prelude::interaction::application_command::ApplicationCommandInteraction;
use std::fmt::{Display, Formatter};

#[derive(Debug)]
/// Represents the context surrounding a command invocation.
pub struct CommandContext<'a> {
    /// The interaction that created this command invocation.
    pub interaction: ApplicationCommandInteraction,

    /// The command that was invoked
    pub(crate) command: &'a CommandRegistration,

    /// The command map, containing the mapped values.
    pub map: CommandMap,

    /// The registry that handled this execution.
    pub registry: &'a CommandRegistry,
}

impl<'a> Display for CommandContext<'a> {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        write!(
            f,
            "[command @{} called by {}#{} in channel {} (guild {:?})]",
            self.command.name,
            self.interaction.user.name,
            self.interaction.user.discriminator,
            self.interaction.channel_id,
            self.interaction.guild_id
        )
    }
}

/// Represents the response to a successful command invocation.
#[derive(Debug)]
pub enum CommandResponse {
    Text(String),
    Embed(CreateEmbed),
}

/// Represents a failure from a command.
#[derive(Clone, Debug)]
pub struct CommandError {
    message: String,
}

impl CommandError {
    pub fn new<F: ToString>(message: F) -> CommandError {
        CommandError {
            message: message.to_string(),
        }
    }

    pub fn to_string(&self, ctx: &CommandContext) -> String {
        format!(
            "error in {}: {}",
            ctx.command
                .manifest
                .0
                .get("name")
                .unwrap()
                .as_str()
                .unwrap(),
            self.message
        )
    }
}

pub fn embed<T>(activate: T) -> CommandOutput
where
    T: FnOnce(&mut CreateEmbed),
{
    let mut emb = CreateEmbed::default();
    (activate)(&mut emb);
    Ok(CommandResponse::Embed(emb))
}

/// Represents a possible successful or failure command invocation.
pub type CommandOutput = Result<CommandResponse, CommandError>;
