use crate::execution::CommandContext;
use crate::registry::CommandMap;
use serenity::builder::{CreateApplicationCommand, CreateEmbed};
use std::collections::HashMap;
use std::fmt::{Debug, Display, Formatter};

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

#[macro_export]
macro_rules! err {
    ($($t:tt)*) => {{
        Err(CommandError::new(format!($($t)*)))
    }};
}
pub use err;

#[macro_export]
macro_rules! text {
    ($($t:tt)*) => {{
        Ok(Text(format!($($t)*)))
    }};
}
pub use text;

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

/// A built command, held by a registry.
pub struct CommandRegistration {
    /// The name of this command.
    pub(crate) name: &'static str,

    /// A pointer to the generated invoke handler, created by the `command!` macro.
    pub(crate) generated_invoke: CommandInvokePtr,

    /// The compiled command information.
    pub(crate) manifest: CreateApplicationCommand,
}

impl Debug for CommandRegistration {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        write!(
            f,
            "Command {} (pointer {:x})",
            self.name, self.generated_invoke as usize
        )
    }
}

/// A generated invoke handler, created by the `command!` macro.
/// This invoke handler should called the user-provided function with the mapped types from `&CommandMap`.
pub type CommandInvokePtr = fn(&CommandContext, &CommandMap) -> CommandOutput;

/// A parameter to a command.
pub struct CommandParameter {
    /// The name of the Rust type.
    pub rust_type: &'static str,

    /// The name of the parameter.
    pub name: &'static str,

    /// The compiled attributes.
    pub attrs: HashMap<&'static str, String>,
}
