use serenity::builder::CreateApplicationCommand;
use serenity::model::prelude::interaction::application_command::ApplicationCommandInteraction;
pub struct CommandContext {
    pub interaction: ApplicationCommandInteraction,
    pub(crate) command: CreateApplicationCommand,
}
