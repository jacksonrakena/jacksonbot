mod jacksonbot;

use log::LevelFilter;
use pretty_env_logger::env_logger::{Builder, Target};
use serde_json::Value;
use std::collections::HashMap;
use std::env;
use std::iter::Map;
use std::sync::{Arc, Mutex};
use std::time::SystemTime;

use crate::jacksonbot::infra::execution::CommandContext;
use crate::jacksonbot::infra::registry::CommandRegistry;
use crate::jacksonbot::modules::fun::fun_module;
use crate::jacksonbot::modules::profile::meta::profile_module;
use crate::jacksonbot::modules::user::user_module;
use serenity::builder::{CreateApplicationCommand, CreateEmbed};
use serenity::model::prelude::interaction::application_command::{
    ApplicationCommandInteraction, CommandDataOptionValue,
};
use serenity::{
    async_trait,
    model::prelude::{
        interaction::{self, Interaction},
        GuildId, Ready,
    },
    prelude::{Context, EventHandler, GatewayIntents},
    Client,
};

extern crate pretty_env_logger;
#[macro_use]
extern crate log;

#[tokio::main]
async fn main() {
    //pretty_env_logger::init();
    let mut log = Builder::from_default_env();
    log.target(Target::Stdout);
    log.filter_module("jacksonbot", LevelFilter::Info);
    log.init();
    let text = std::fs::read_to_string("jacksonbot.json").unwrap();
    let config = serde_json::from_str::<Value>(&text).unwrap();
    let token = config["Secrets"]["Discord"]["Token"].as_str().unwrap();

    let mut handler = BotEventHandler {
        registry: CommandRegistry::new(),
    };

    handler.registry.register_module(fun_module());
    handler.registry.register_module(profile_module());
    handler.registry.register_module(user_module());

    let mut client = Client::builder(token, GatewayIntents::empty())
        .event_handler(handler)
        .await
        .expect("error creating client");

    if let Err(why) = client.start().await {
        error!("Client error: {:?}", why);
    }
}

struct BotEventHandler {
    registry: CommandRegistry,
}

#[async_trait]
impl EventHandler for BotEventHandler {
    async fn ready(&self, ctx: Context, ready: Ready) {
        info!("Ready.");
        let guild_id = GuildId(679929597982539778);

        match guild_id
            .set_application_commands(&ctx.http, |cmds| {
                cmds.set_application_commands(
                    self.registry
                        .commands
                        .values()
                        .into_iter()
                        .map(|c| c.manifest.clone())
                        .collect(),
                );
                cmds
            })
            .await
        {
            Ok(commands) => {
                info!("Uploaded {} commands.", commands.len());
            }
            Err(why) => {
                error!("Failed to update commands: {:#?}", why);
            }
        }
    }

    async fn interaction_create(&self, ctx: Context, interaction: Interaction) {
        self.registry.handle(ctx, interaction).await;
    }
}
