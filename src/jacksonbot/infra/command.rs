use crate::jacksonbot::infra::execution::CommandContext;
use serenity::builder::{CreateApplicationCommand, CreateEmbed};

pub type CommandFunctionPtr = fn(&mut CommandContext);

pub enum CommandResult {
    Text(String),
    Embed(CreateEmbed),
}

pub struct CommandExecutable {
    pub(crate) manifest: Box<CreateApplicationCommand>,
    pub(crate) invoke: CommandFunctionPtr,
}
