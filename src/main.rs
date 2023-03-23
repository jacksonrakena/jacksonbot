mod jacksonbot;

use std::collections::HashMap;
use std::env;
use std::iter::Map;
use std::sync::{Arc, Mutex};
use std::time::SystemTime;
use log::LevelFilter;
use pretty_env_logger::env_logger::{Builder, Target};
use serde_json::Value;

use serenity::model::prelude::interaction::application_command::ApplicationCommandInteraction;
use serenity::{
    async_trait,
    model::prelude::{
        interaction::{self, Interaction},
        GuildId, Ready,
    },
    prelude::{Context, EventHandler, GatewayIntents},
    Client,
};
use serenity::builder::CreateApplicationCommand;
use serenity::model::prelude::Embed;
use serenity::model::prelude::interaction::InteractionResponseType;
use crate::CommandResult::Text;
use crate::jacksonbot::infra::module::Module;
use crate::jacksonbot::modules::fun::fun_module;
extern crate pretty_env_logger;
#[macro_use] extern crate log;

#[tokio::main]
async fn main() {
    //pretty_env_logger::init();
    let mut log = Builder::from_default_env();
    log.target(Target::Stdout);
    log.filter_module("jacksonbot", LevelFilter::Info);
    log.init();
    let text = std::fs::read_to_string("jacksonbot.json").unwrap();
    let config = serde_json::from_str::<Value>(&text).unwrap();
    let token =  config["Secrets"]["Discord"]["Token"].as_str().unwrap();

    let mut handl = JacksonbotEventHandler { registry: CommandRegistry::new() };
    handl.registry.register_simple("pingx", "Checks if I am not dead.", |cmd|{
        cmd.response = Some(Text("hello! :crab:".to_string()));
    });

    handl.registry.register_module(fun_module());

    let mut client = Client::builder(token, GatewayIntents::empty())
        .event_handler(handl)
        .await
        .expect("error creating client");

    if let Err(why) = client.start().await {
        error!("Client error: {:?}", why);
    }
}

enum CommandResult {
    Text(String),
    Embed(Embed)
}

struct CommandExecContext {
    interaction: ApplicationCommandInteraction,
    response: Option<CommandResult>
}
impl CommandExecContext {
    fn ok(&mut self, result: CommandResult) {
        self.response = Some(result)
    }
}
struct CommandExecutable {
    manifest: Box<CreateApplicationCommand>,
    invoke: Box<dyn Fn(&mut CommandExecContext) + Send + Sync>
}


pub struct CommandRegistry {
    commands: HashMap<String, CommandExecutable>,
    modules: Vec<Module>,
    ready: bool
}
impl CommandRegistry {
    fn new() -> CommandRegistry {
        let reg = CommandRegistry { commands: HashMap::<String,CommandExecutable>::new(), modules: vec!(), ready: false };
        reg
    }

    fn register_module(&mut self, module: Module) {
        (module.registrant)(self);
        self.modules.push(module);
    }

    fn register_simple<F, D: ToString>(&mut self, name: D, description: D, invoke: F) -> &CommandRegistry where F: Fn(&mut CommandExecContext) + Send + Sync + 'static  {
        let mut cmd = CreateApplicationCommand::default();
        cmd.name(name).description(description);
        let exec = CommandExecutable {
            invoke: Box::new(invoke),
            manifest: Box::new(cmd)
        };
        self.commands.insert(exec.manifest.0["name"].as_str().unwrap().to_string(), exec);
        self
    }

    fn register_custom<E, F>(&mut self, attributes: E, invoke: F) -> &CommandRegistry where E: Fn(&mut CreateApplicationCommand), F: Fn(&mut CommandExecContext) + Send + Sync + 'static {
        let mut cmd = CreateApplicationCommand::default();
        attributes(&mut cmd);
        let exec = CommandExecutable {
            invoke: Box::new(invoke),
            manifest: Box::new(cmd)
        };
        self.commands.insert(exec.manifest.0["name"].as_str().unwrap().to_string(), exec);
        self
    }

    async fn handle(&self, ctx: Context, interaction: Interaction) {
        if let Interaction::ApplicationCommand(command) = interaction {
            let name = command.data.name.clone();
            info!("Handling {}", name);
            let start = SystemTime::now();
            match self.commands.get(name.as_str()) {
                Some(exec) => {
                    let mut command_ctx = CommandExecContext{
                        interaction: command,
                        response: None
                    };
                    (exec.invoke)(&mut command_ctx);
                    info!("Executed '{}' in {}ms", command_ctx.interaction.data.name, (SystemTime::now().duration_since(start).unwrap().as_millis()));
                    match command_ctx.response {
                        None => {}
                        Some(CommandResult::Text(text)) => {
                            command_ctx.interaction.create_interaction_response(&ctx.http, |response| {
                                response.kind(InteractionResponseType::ChannelMessageWithSource).interaction_response_data(|message| {
                                    message.content(text.clone())
                                })
                            }).await;
                            info!("Finished '{}' in {}ms", command_ctx.interaction.data.name, (SystemTime::now().duration_since(start).unwrap().as_millis()));
                        }
                        Some(CommandResult::Embed(embed)) => {}
                    }
                }
                None => {
                    command.create_interaction_response(&ctx.http, |response|{
                        response.kind(InteractionResponseType::ChannelMessageWithSource).interaction_response_data(|msg|{
                            msg.content("unknown command :(".to_string())
                        })
                    }).await;
                }
            }
        }
    }
}


struct JacksonbotEventHandler {
    registry: CommandRegistry
}

#[async_trait]
impl EventHandler for JacksonbotEventHandler {
    async fn ready(&self, ctx: Context, ready: Ready) {
        info!("Ready.");
        let guild_id = GuildId(679929597982539778);

        match guild_id
            .set_application_commands(&ctx.http, |cmds| {
                for entry in &self.registry.commands {
                    // TODO don't clone this
                    cmds.add_application_command(*entry.1.manifest.clone());
                }
                cmds
            })
            .await {
            Ok(commands) => {
                info!("Uploaded {} commands.", commands.len());
            }
            Err(why) => {
                error!("Failed to update commands: {:#?}", why);
            }
        }
    }

    async fn interaction_create(&self, ctx: Context, interaction: Interaction) {
        self.registry.handle(ctx,interaction).await;
    }
}
