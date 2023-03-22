use std::collections::HashMap;
use std::env;
use std::iter::Map;
use std::sync::{Arc, Mutex};

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

#[tokio::main]
async fn main() {
    let token = "NzkwMzkwNzE1ODg4NDM1MjMw.GNaDYo.nYfsQCCaI65fc2piyu2FMi8u8nUya6oEfacgDg";

    let mut reg = CommandRegistry::new();
    let mut client = Client::builder(token, GatewayIntents::empty())
        .event_handler(JacksonbotEventHandler{registry: reg })
        .await
        .expect("error creating client");

    if let Err(why) = client.start().await {
        println!("Client error: {:?}", why);
    }

    reg.register(make_cmd("ping", "Checks if I am not dead.", |cmd|{

    }));

    make_cmd("test","", |ctx| {

    });
}

struct CommandExecContext;
struct CommandExecutable {
    manifest: Box<CreateApplicationCommand>,
    invoke: Box<dyn Fn(CommandExecContext) + Send + Sync>
}
fn make_cmd<F, D: ToString>(name: D, description: D, invoke: F) -> CommandExecutable where F: Fn(CommandExecContext) + Send + Sync{
    let mut cmd = CreateApplicationCommand::default();
    cmd.name(name).description(description);
    let exec = CommandExecutable {
        invoke: Box::new(invoke),
        manifest: Box::new(cmd)
    };
    exec
}

struct CommandRegistry {
    commands: HashMap<String, CommandExecutable>
}
impl CommandRegistry {
    fn new() -> CommandRegistry {
        let reg = CommandRegistry { commands: HashMap::<String,CommandExecutable>::new() };
        reg
    }

    fn register(&mut self, exec: CommandExecutable) -> &CommandRegistry {
        self.commands.insert(exec.manifest.0["name"].as_str().unwrap().to_string(), exec);
        self
    }

    fn handle(&self, ctx:Context, interaction: Interaction) {
        if let Interaction::ApplicationCommand(command) = interaction {
            let name = command.data.name;
            match self.commands.get(name.as_str()) {
                Some(exec) => {
                    let command_ctx = CommandExecContext{};
                    (exec.invoke)(command_ctx);
                }
                None => {}
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
        println!("ready.");
        let guild_id = GuildId(679929597982539778);

        let commands = guild_id
            .set_application_commands(&ctx.http, |cmds| {
                cmds.create_application_command(|cmd| {
                    cmd.name("ping").description("Checks if I'm alive.")
                })
            })
            .await;

        println!("updated commands");
    }

    async fn interaction_create(&self, ctx: Context, interaction: Interaction) {
        if let Interaction::ApplicationCommand(command) = interaction {
            println!("RECV interaction {}", command.data.name);
            let response = match command.data.name.as_str() {
                "ping" => "Pong!".to_string(),
                _ => "unknown command :(".to_string(),
            };

            if let Err(why) = command
                .create_interaction_response(&ctx.http, |respo| {
                    respo
                        .kind(interaction::InteractionResponseType::ChannelMessageWithSource)
                        .interaction_response_data(|msg| msg.content(response))
                })
                .await
            {
                println!("failed slash command response: {}", why);
            }
        }
    }
}
