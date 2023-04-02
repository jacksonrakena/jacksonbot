use crate::command::CommandRegistration;
use serenity::model::prelude::interaction::application_command::ApplicationCommandInteraction;
use std::fmt::{Display, Formatter};

#[derive(Debug)]
/// Represents the context surrounding a command invocation.
pub struct CommandContext<'a> {
    /// The interaction that created this command invocation.
    pub interaction: ApplicationCommandInteraction,

    /// The command that was invoked
    pub(crate) command: &'a CommandRegistration,
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
