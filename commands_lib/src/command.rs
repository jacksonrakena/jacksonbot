use crate::execution::{CommandContext, CommandOutput};
use serenity::builder::CreateApplicationCommand;
use std::collections::HashMap;
use std::fmt::{Debug, Formatter};

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
pub type CommandInvokePtr = fn(&CommandContext) -> CommandOutput;

/// A parameter to a command.
pub struct CommandParameter {
    /// The name of the Rust type.
    pub rust_type: &'static str,

    /// The name of the parameter.
    pub name: &'static str,

    /// The compiled attributes.
    pub attrs: HashMap<&'static str, String>,
}
