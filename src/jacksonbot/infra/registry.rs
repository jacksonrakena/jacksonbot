use crate::jacksonbot::infra::command::{
    CommandExecutable, CommandFunctionPtr, CommandOutput, CommandResult,
};
use crate::jacksonbot::infra::module::Module;
use crate::CommandContext;
use serenity::builder::{CreateApplicationCommand, CreateEmbed, CreateInteractionResponse};
use serenity::model::prelude::interaction::application_command::ApplicationCommandInteraction;
use serenity::model::prelude::interaction::{Interaction, InteractionResponseType};
use serenity::prelude::Context;
use std::collections::HashMap;
use std::time::SystemTime;

pub struct CommandRegistry {
    pub(crate) commands: HashMap<String, CommandExecutable>,
    modules: Vec<Module>,
    ready: bool,
}

impl CommandRegistry {
    pub(crate) fn new() -> CommandRegistry {
        let reg = CommandRegistry {
            commands: HashMap::<String, CommandExecutable>::new(),
            modules: vec![],
            ready: false,
        };
        reg
    }

    pub(crate) fn register_module(&mut self, module: Module) {
        (module.registrant)(self);
        self.modules.push(module);
    }

    pub(crate) fn register_simple<D: ToString>(
        &mut self,
        name: D,
        description: D,
        invoke: CommandFunctionPtr,
    ) -> &CommandRegistry {
        let mut cmd = CreateApplicationCommand::default();
        cmd.name(name).description(description);
        let exec = CommandExecutable {
            invoke,
            manifest: cmd,
        };
        self.commands
            .insert(exec.manifest.0["name"].as_str().unwrap().to_string(), exec);
        self
    }

    pub(crate) fn register_custom(
        &mut self,
        attributes: fn(&mut CreateApplicationCommand),
        invoke: CommandFunctionPtr,
    ) -> &CommandRegistry {
        let mut cmd = CreateApplicationCommand::default();
        attributes(&mut cmd);
        let exec = CommandExecutable {
            invoke,
            manifest: cmd,
        };
        self.commands
            .insert(exec.manifest.0["name"].as_str().unwrap().to_string(), exec);
        self
    }

    pub(crate) async fn handle(&self, ctx: Context, interaction: Interaction) {
        if let Interaction::ApplicationCommand(command) = interaction {
            let name = command.data.name.clone();
            info!("Handling {}", name);
            let start = SystemTime::now();

            match self.commands.get(&name) {
                Some(exec) => {
                    info!("Executing '{}'", &name);
                    let mut command_ctx = CommandContext {
                        interaction: command,
                        command: exec,
                    };
                    let m = (exec.invoke)(&mut command_ctx);
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
                                                ":warning: error: `{}`",
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
