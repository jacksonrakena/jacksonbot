use serenity::builder::{CreateApplicationCommand, CreateEmbed};
use crate::jacksonbot::infra::execution::CommandContext;

pub type CommandFunctionPtr = fn(&mut CommandContext);

pub enum CommandResult {
    Text(String),
    Embed(CreateEmbed)
}

pub struct CommandExecutable {
    pub(crate) manifest: Box<CreateApplicationCommand>,
    pub(crate) invoke: CommandFunctionPtr
}