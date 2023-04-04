use crate::command::CommandRegistration;
use crate::execution::{CommandContext, CommandResponse};
use crate::parameter_value::FromCommandParameterValue;
use log::{error, info, trace};
use serenity::builder::CreateApplicationCommand;
use serenity::model::prelude::interaction::application_command::CommandDataOption;
use serenity::model::prelude::interaction::{Interaction, InteractionResponseType};
use serenity::prelude::Context;
#[allow(dead_code, unused, unused_variables)]
use std::collections::HashMap;
use std::fmt::{Debug, Display, Formatter};
use std::time::SystemTime;

#[derive(Debug)]
pub struct CommandMap {
    options: Vec<CommandDataOption>,
}

impl CommandMap {
    pub fn get<T>(&self, name: &'static str) -> T
    where
        T: FromCommandParameterValue,
    {
        for opt in &self.options {
            if opt.name == name {
                return T::from_command_parameter_value(opt.resolved.clone());
            }
        }
        return T::from_command_parameter_value(None);
    }
}

/// The command registry contains all commands and their names, and is responsible
/// for handling interactions as they are received.
pub struct CommandRegistry {
    /// A map of command names to their registration objects.
    pub commands: HashMap<&'static str, CommandRegistration>,
}
impl CommandRegistry {
    /// Registers a command.
    pub fn register(&mut self, info: CommandRegistration) {
        self.commands.insert(info.name, info);
    }

    /// Converts the internal command mapping into a vector of manifest objects,
    /// which can be sent to Discord to register commands.
    ///
    /// This command clones the command manifests.
    pub fn make_commands(&self) -> Vec<CreateApplicationCommand> {
        self.commands
            .values()
            .into_iter()
            .map(|c| c.manifest.clone())
            .collect()
    }

    /// Handles an incoming Discord interaction.  
    /// This method consumes the interaction and will respond to it based on the detected command.
    pub async fn handle(&self, ctx: Context, interaction: Interaction) {
        let Interaction::ApplicationCommand(command) = interaction else { return };

        let name = command.data.name.clone();
        trace!("Handling {}", name);
        let start = SystemTime::now();

        match self.commands.get(name.as_str()) {
            Some(exec) => {
                trace!("Executing '{}'", &name);
                let opts = command.data.options.clone();
                let command_ctx = CommandContext {
                    interaction: command,
                    command: exec,
                    map: CommandMap { options: opts },
                    registry: &self,
                };

                let command_output = (exec.generated_invoke)(&command_ctx);
                trace!(
                    "Executed '{}' in {}μs",
                    command_ctx.interaction.data.name,
                    (SystemTime::now().duration_since(start).unwrap().as_micros())
                );

                command_ctx
                    .interaction
                    .create_interaction_response(&ctx.http, |response| {
                        response
                            .kind(InteractionResponseType::ChannelMessageWithSource)
                            .interaction_response_data(|msg| match command_output {
                                Ok(res) => {
                                    return match res {
                                        CommandResponse::Text(text) => msg.content(text),
                                        CommandResponse::Embed(e) => msg.set_embed(e),
                                    };
                                }
                                Err(why) => msg.content(format!(
                                    ":warning: `{}`",
                                    why.to_string(&command_ctx)
                                )),
                            })
                    })
                    .await
                    .unwrap_or_else(|why| {
                        error!(
                            "Failed to send response for '{}': {}",
                            command_ctx.interaction.data.name, why
                        );
                    });

                info!(
                    "Finished in {}ms: {}",
                    (SystemTime::now().duration_since(start).unwrap().as_millis()),
                    command_ctx,
                );
            }
            None => {
                error!("Unknown command '{}'", name.as_str());
                if let Err(why) = command.create_interaction_response(&ctx.http, |response|{
                        response.kind(InteractionResponseType::ChannelMessageWithSource).interaction_response_data(|msg|{
                            msg.content("I do not know a command by that name. Discord may be out of date. Please check back later.".to_string())
                        })
                    }).await {
                        error!("Failed to send unknown command response: {}", why);
                    }
            }
        }
    }
}
impl Default for CommandRegistry {
    fn default() -> Self {
        CommandRegistry {
            commands: HashMap::new(),
        }
    }
}

impl Debug for CommandRegistry {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", self)
    }
}
impl Display for CommandRegistry {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        write!(f, "[CommandRegistry, {} commands]", self.commands.len())
    }
}
