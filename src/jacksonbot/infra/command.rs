use crate::jacksonbot::infra::execution::CommandContext;
use serenity::builder::{CreateApplicationCommand, CreateEmbed};
use std::error::Error;
use std::fmt;
use std::fmt::Display;

pub type CommandFunctionPtr = fn(&mut CommandContext) -> CommandOutput;

pub enum CommandResult {
    Text(String),
    Embed(CreateEmbed),
}

pub struct CommandExecutable {
    pub(crate) manifest: CreateApplicationCommand,
    pub(crate) invoke: CommandFunctionPtr,
}

#[derive(Clone)]
pub struct CommandError {
    message: String,
}

impl CommandError {
    pub(crate) fn new<F: ToString>(message: F) -> CommandError {
        CommandError {
            message: message.to_string(),
        }
    }

    pub(crate) fn to_string(&self, ctx: &CommandContext) -> String {
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

pub type CommandOutput = Result<CommandResult, CommandError>;
