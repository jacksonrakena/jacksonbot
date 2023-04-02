use crate::command::{CommandRegistration, CommandResponse};
use crate::execution::CommandContext;
use crate::parameter_value::ParameterValue;
use log::{error, info, trace};
use serenity::builder::CreateApplicationCommand;
use serenity::model::prelude::interaction::application_command::CommandDataOption;
use serenity::model::prelude::interaction::{Interaction, InteractionResponseType};
use serenity::prelude::Context;
#[allow(dead_code, unused, unused_variables)]
use std::collections::HashMap;
use std::time::SystemTime;

pub struct CommandMap {
    options: Vec<CommandDataOption>,
}

impl CommandMap {
    pub fn get<T>(&self, name: &'static str) -> T
    where
        T: ParameterValue,
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
    pub commands: HashMap<&'static str, CommandRegistration>,
}
impl CommandRegistrar {
    pub fn register(&mut self, info: CommandRegistration) {
        self.commands.insert(info.name, info);
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
            trace!("Handling {}", name);
            let start = SystemTime::now();

            match self.commands.get(name.as_str()) {
                Some(exec) => {
                    trace!("Executing '{}'", &name);
                    let map = CommandMap {
                        options: command.data.options.clone(),
                    };
                    let command_ctx = CommandContext {
                        interaction: command,
                        command: exec,
                    };
                    let m = (exec.generated_invoke)(&command_ctx, &map);
                    trace!(
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
                                                CommandResponse::Text(text) => {
                                                    message.content(text)
                                                }
                                                CommandResponse::Embed(e) => message.set_embed(e),
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
}
impl Default for CommandRegistrar {
    fn default() -> Self {
        CommandRegistrar {
            commands: HashMap::new(),
        }
    }
}
