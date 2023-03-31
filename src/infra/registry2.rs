use crate::infra::command::{CommandOutput, CommandResult};
use crate::infra::execution::CommandContext;
use serenity::builder::CreateApplicationCommand;
use serenity::model::prelude::interaction::application_command::{
    CommandDataOption, CommandDataOptionValue,
};
use serenity::model::prelude::interaction::{Interaction, InteractionResponseType};
use serenity::prelude::Context;
#[allow(dead_code, unused, unused_variables)]
use std::collections::HashMap;
use std::time::SystemTime;

macro_rules! commands {
    ($block: block) => {
        use crate::infra::command::CommandOutput;
        use crate::infra::make_command::make_command;
        use crate::infra::registry2::{CommandMap, CommandParameter, CommandRegistration};
        #[allow(dead_code, unused, unused_variables)]
        use std::collections::HashMap;
        $block()
    };
}
pub(crate) use commands;
pub struct CommandParameter {
    pub ty: &'static str,
    pub name: &'static str,
    pub attrs: HashMap<&'static str, String>,
}
macro_rules! command {
    (
        $([  $($cmd_attr_name:ident=$cmd_attr_value:expr)*  ])? $cmd_name: ident,
        $( $([  $($param_attr_name:ident=$param_attr_value:expr)*  ])?  $param_name:ident $param_type:ty,)*
        @$block: ident) => {
        {
            // Compile command-level attributes
            let mut cmd_attrs = HashMap::new();
            $(
                $(
                    cmd_attrs.insert(stringify!($cmd_attr_name), $cmd_attr_value.to_string()); // buh?
                )*
            )?

            // Compile parameters
            let mut params = Vec::<CommandParameter>::new();
            $(
                // Parameter-level attributes
                let mut attrs = HashMap::new();
                $(
                    $(
                        attrs.insert(stringify!($param_attr_name), $param_attr_value.to_string());
                    )*
                )?
                params.push(CommandParameter {
                    ty: stringify!($param_type),
                    attrs: attrs,
                    name: stringify!($param_name)
                });
            )*

            let cmd = make_command(stringify!($cmd_name), cmd_attrs, params);
            fn $cmd_name (map: &CommandMap) -> CommandOutput {
                $block($(map.get::<$param_type>(stringify!($param_name)),)*)
            }
            CommandRegistration { name: stringify!($cmd_name), generated_invoke: $cmd_name, manifest: cmd }
        }
    }
}
use crate::infra::types::CommandValueCoercable;
pub(crate) use command;

pub struct CommandRegistration {
    pub(crate) name: &'static str,
    pub(crate) generated_invoke: fn(&CommandMap) -> CommandOutput,
    pub(crate) manifest: CreateApplicationCommand,
}

pub struct CommandMap {
    options: Vec<CommandDataOption>,
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
    pub commands: HashMap<&'static str, CommandRegistration>,
}
impl CommandRegistrar {
    pub(crate) fn register(&mut self, info: CommandRegistration) {
        self.commands.insert(info.name, info);
    }
    pub(crate) async fn handle(&self, ctx: Context, interaction: Interaction) {
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
                    let m = (exec.generated_invoke)(&map);
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
impl Default for CommandRegistrar {
    fn default() -> Self {
        CommandRegistrar {
            commands: HashMap::new(),
        }
    }
}
