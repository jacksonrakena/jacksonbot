pub mod modules;

use crate::modules::fun::fun_module;
use crate::modules::user::user_module;
use chrono::{DateTime, Utc};
use commands_lib::registry::CommandRegistrar;
use log::LevelFilter;
use pretty_env_logger::env_logger::{Builder, Target};
use serde_json::Value;
use std::time::SystemTime;

use serenity::{
    async_trait,
    model::prelude::{interaction::Interaction, GuildId, Ready},
    prelude::{Context, EventHandler, GatewayIntents},
    Client,
};

extern crate pretty_env_logger;
#[macro_use]
extern crate log;

#[tokio::main]
async fn main() {
    let startup = SystemTime::now();
    let date: DateTime<Utc> = startup.into();
    println!("Starting Jacksonbot 21.1.0 at {}", date.format("%+"));

    //pretty_env_logger::init();
    let mut log = Builder::from_default_env();
    log.target(Target::Stdout);
    log.filter_module(stringify!(jacksonbot), LevelFilter::Info);
    log.filter_module(stringify!(commands_lib), LevelFilter::Info);
    log.init();

    info!(
        "Initialised logging services in {}µs",
        SystemTime::now()
            .duration_since(startup)
            .unwrap()
            .as_micros()
    );

    let config_load_start = SystemTime::now();
    let text = std::fs::read_to_string("jacksonbot.json")
        .unwrap_or_else(|why| panic!("couldn't find jacksonbot.json: {why}"));

    let config = serde_json::from_str::<Value>(&text).unwrap_or_else(|why| {
        panic!("couldn't read jacksonbot.json: {why}");
    });

    let token = config["Secrets"]["Discord"]["Token"]
        .as_str()
        .expect("expected a token at Secrets->Discord->Token");

    info!(
        "Loaded configuration in {}µs",
        SystemTime::now()
            .duration_since(config_load_start)
            .unwrap()
            .as_micros()
    );

    let commands_load_start = SystemTime::now();

    let mut handler = BotEventHandler {
        registry: CommandRegistrar::default(),
    };

    user_module(&mut handler.registry);
    fun_module(&mut handler.registry);
    // handler.registry.register_module(fun_module());
    // handler.registry.register_module(profile_module());
    // handler.registry.register_module(user_module());

    let mut client = Client::builder(token, GatewayIntents::empty())
        .event_handler(handler)
        .await
        .expect("error creating client");

    info!(
        "Built client and commands in {}ms",
        SystemTime::now()
            .duration_since(commands_load_start)
            .unwrap()
            .as_millis()
    );

    if let Err(why) = client.start().await {
        error!("Client error: {:?}", why);
    }
}

struct BotEventHandler {
    registry: CommandRegistrar,
}

#[async_trait]
impl EventHandler for BotEventHandler {
    async fn ready(&self, ctx: Context, ready: Ready) {
        info!(
            "Ready. Connected as {}#{} ({})",
            ready.user.name, ready.user.discriminator, ready.user.id
        );
        let guild_id = GuildId(679929597982539778);

        match guild_id
            .set_application_commands(&ctx.http, |cmds| {
                cmds.set_application_commands(self.registry.make_commands());
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
