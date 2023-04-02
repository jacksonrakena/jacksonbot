use crate::command::{CommandOutput, CommandResult};
use crate::execution::CommandContext;
use crate::types::CommandValueCoercable;
use log::{error, info};
use serenity::builder::CreateApplicationCommand;
use serenity::model::prelude::interaction::application_command::CommandDataOption;
use serenity::model::prelude::interaction::{Interaction, InteractionResponseType};
use serenity::prelude::Context;
#[allow(dead_code, unused, unused_variables)]
use std::collections::HashMap;
use std::time::SystemTime;

pub struct CommandRegistration {
    pub(crate) name: String,
    pub(crate) generated_invoke: fn(&CommandContext, &CommandMap) -> CommandOutput,
    pub(crate) manifest: CreateApplicationCommand,
}

pub type CommandInvokePtr = fn(&CommandContext, &CommandMap) -> CommandOutput;

pub struct CommandMap {
    options: Vec<CommandDataOption>,
}
pub struct CommandParameter {
    pub ty: &'static str,
    pub name: &'static str,
    pub attrs: HashMap<&'static str, String>,
}
impl CommandMap {
    pub fn get<T>(&self, name: &'static str) -> T
    where
        T: CommandValueCoercable,
    {
        for opt in &self.options {
            if opt.name == name {
                return T::get_value(opt.resolved.clone());
            }
        }
        return T::get_value(None);
    }
}

pub struct CommandRegistrar {
    pub commands: HashMap<String, CommandRegistration>,
}
impl CommandRegistrar {
    pub fn register(&mut self, info: CommandRegistration) {
        self.commands.insert(info.name.clone(), info);
    }

    pub fn make_commands(&self) -> Vec<CreateApplicationCommand> {
        self.commands
            .values()
            .into_iter()
            .map(|c| c.manifest.clone())
            .collect()
    }

    pub async fn handle(&self, ctx: Context, interaction: Interaction) {
        if let Interaction::ApplicationCommand(command) = interaction {
            let name = command.data.name.clone();
            info!("Handling {}", name);
            let start = SystemTime::now();

            match self.commands.get(name.as_str()) {
                Some(exec) => {
                    info!("Executing '{}'", &name);
                    let map = CommandMap {
                        options: command.data.options.clone(),
                    };
                    let command_ctx = CommandContext {
                        interaction: command,
                        command: exec.manifest.clone(),
                    };
                    let m = (exec.generated_invoke)(&command_ctx, &map);
                    info!(
                        "Executed '{}' in {}μs",
                        command_ctx.interaction.data.name,
                        (SystemTime::now().duration_since(start).unwrap().as_micros())
                    );

                    match m {
                        Ok(res) => {
                            command_ctx
                                .interaction
                                .create_interaction_response(&ctx.http, |response| {
                                    response
                                        .kind(InteractionResponseType::ChannelMessageWithSource)
                                        .interaction_response_data(|message| {
                                            return match res {
                                                CommandResult::Text(text) => message.content(text),
                                                CommandResult::Embed(e) => message.set_embed(e),
                                            };
                                        })
                                })
                                .await
                                .unwrap_or_else(|why| {
                                    error!(
                                        "Failed to send response for '{}': {}",
                                        command_ctx.interaction.data.name, why
                                    );
                                });
                        }
                        Err(why) => {
                            command_ctx
                                .interaction
                                .create_interaction_response(&ctx.http, |response| {
                                    response
                                        .kind(InteractionResponseType::ChannelMessageWithSource)
                                        .interaction_response_data(|msg| {
                                            msg.content(format!(
                                                ":warning: `{}`",
                                                why.to_string(&command_ctx)
                                            ))
                                        })
                                })
                                .await
                                .unwrap_or_else(|why| {
                                    error!(
                                        "Failed to send response for '{}': {}",
                                        command_ctx.interaction.data.name, why
                                    );
                                });
                        }
                    }
                    info!(
                        "Finished '{}' in {}ms",
                        command_ctx.interaction.data.name,
                        (SystemTime::now().duration_since(start).unwrap().as_millis())
                    );
                }
                None => {
                    info!("Unknown command '{}'", name.as_str());
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
}
impl Default for CommandRegistrar {
    fn default() -> Self {
        CommandRegistrar {
            commands: HashMap::new(),
        }
    }
}
