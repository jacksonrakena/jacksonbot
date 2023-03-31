use crate::infra::command::{CommandOutput, CommandResult};
use crate::infra::execution::CommandContext;
use serenity::builder::CreateApplicationCommand;
use serenity::model::prelude::interaction::application_command::{
    CommandData, CommandDataOptionValue,
};
use serenity::model::prelude::interaction::{Interaction, InteractionResponseType};
use serenity::model::prelude::User;
use serenity::prelude::Context;
#[allow(dead_code, unused, unused_variables)]
use std::collections::HashMap;
use std::time::SystemTime;

macro_rules! commands {
    ($block: block) => {
        use crate::infra::command::CommandOutput;
        use crate::infra::registry2::{CommandMap, CommandParameter};
        use lazy_static::lazy_static;
        use regex::Regex;
        use serenity::builder::{CreateApplicationCommand, CreateApplicationCommandOption};
        use serenity::model::prelude::command::CommandOptionType;
        #[allow(dead_code, unused, unused_variables)]
        use std::collections::HashMap;
        fn make(
            name: &'static str,
            cmd_attrs: HashMap<String, String>,
            params: Vec<CommandParameter>,
        ) -> CreateApplicationCommand {
            let mut appcommand = CreateApplicationCommand::default();
            appcommand.name(name);
            let default_description: String = "".to_string();

            for (attr_name, attr_value) in cmd_attrs {
                if attr_name == "description" {
                    appcommand.description(attr_value);
                }
            }
            for p in params {
                let mut opt = CreateApplicationCommandOption::default();
                let name = p.name;
                let attrs = &p.attrs;
                opt.name(name);
                lazy_static! {
                    static ref OPTIONAL_TYPE_REGEX: Regex = Regex::new(r"Option<(.+)>").unwrap();
                }
                let mut type_name = p.ty.as_str();
                opt.required(true);
                if let Some(m) = OPTIONAL_TYPE_REGEX.captures(p.ty.as_str()) {
                    let optional_inner = m.get(1).unwrap().as_str();
                    println!("Found Option<T>: {}", optional_inner);
                    type_name = optional_inner;
                    opt.required(false);
                }
                let t: CommandOptionType = match type_name {
                    "String" => CommandOptionType::String,
                    "i64" => CommandOptionType::Integer,
                    "User" => CommandOptionType::User,
                    _ => panic!("Invalid option type {}", p.ty),
                };
                opt.kind(t);
                opt.description(attrs.get("description").unwrap_or(&default_description));
                if let Some(max_length) = attrs.get("max_length") {
                    opt.max_length(max_length.parse::<u16>().unwrap());
                }
                if let Some(min_length) = attrs.get("min_length") {
                    opt.min_length(min_length.parse::<u16>().unwrap());
                }
                if let Some(min_value) = attrs.get("min_value") {
                    opt.min_number_value(min_value.parse::<f64>().unwrap());
                }
                if let Some(max_value) = attrs.get("max_value") {
                    opt.max_number_value(max_value.parse::<f64>().unwrap());
                }
                appcommand.add_option(opt);
            }
            appcommand
        }
        $block()
    };
}
pub(crate) use commands;
pub struct CommandParameter {
    pub ty: String,
    pub name: String,
    pub attrs: HashMap<String, String>,
}
macro_rules! command {
    (
        $([  $($cmd_attr_name:ident=$cmd_attr_value:expr)*  ])? $cmd_name: ident,
        $( $([  $($param_attr_name:ident=$param_attr_value:expr)*  ])?  $param_name:ident $param_type:ty,)*
        @$block: ident) => {
        {
            let mut cmd_attrs = HashMap::new();
            $(
                $(
                    cmd_attrs.insert(stringify!($cmd_attr_name).to_string(), $cmd_attr_value.to_string());
                )*
            )?
            let mut params = Vec::<CommandParameter>::new();
            $(
                let mut attrs = HashMap::new();
                $(
                    $(
                        attrs.insert(stringify!($param_attr_name).to_string(), $param_attr_value.to_string());
                    )*
                )?
                params.push(CommandParameter {
                    ty: stringify!($param_type).to_string(),
                    attrs:attrs,
                    name: stringify!($param_name).to_string()
                });
            )*

            let cmd = make(stringify!($cmd_name), cmd_attrs, params);
            fn $cmd_name (map: &CommandMap) -> CommandOutput {
                $block($(map.get::<$param_type>(stringify!($param_name)),)*)
            }
            (stringify!($cmd_name), $cmd_name, cmd)
        }
    }
}
pub(crate) use command;

pub type CommandRegistrant = (
    &'static str,
    fn(&CommandMap) -> CommandOutput,
    CreateApplicationCommand,
);

pub trait CommandValueCoercable {
    fn get_value(value: Option<CommandDataOptionValue>) -> Self;
}
impl CommandValueCoercable for String {
    fn get_value(value: Option<CommandDataOptionValue>) -> Self {
        if let CommandDataOptionValue::String(t) = value.unwrap() {
            return t;
        }
        panic!("")
    }
}
impl<T> CommandValueCoercable for Option<T>
where
    T: CommandValueCoercable,
{
    fn get_value(value: Option<CommandDataOptionValue>) -> Self {
        match value {
            None => None,
            Some(x) => Some(T::get_value(Some(x))),
        }
    }
}
impl CommandValueCoercable for i64 {
    fn get_value(value: Option<CommandDataOptionValue>) -> Self {
        if let CommandDataOptionValue::Integer(i) = value.unwrap() {
            return i;
        }
        panic!("")
    }
}
impl CommandValueCoercable for User {
    fn get_value(value: Option<CommandDataOptionValue>) -> Self {
        if let CommandDataOptionValue::User(u, ..) = value.unwrap() {
            return u;
        }
        panic!("");
    }
}
pub struct CommandMap {
    map: HashMap<String, Option<CommandDataOptionValue>>,
}
impl From<&CommandData> for CommandMap {
    fn from(value: &CommandData) -> Self {
        let mut map: HashMap<String, Option<CommandDataOptionValue>> = HashMap::new();
        for opt in &value.options {
            map.insert(opt.name.clone(), opt.resolved.clone());
        }
        CommandMap { map: map }
    }
}
impl CommandMap {
    pub fn get<T>(&self, name: &'static str) -> T
    where
        T: CommandValueCoercable,
    {
        let boxed = self.map.get(name).unwrap_or(&None).clone();
        return T::get_value(boxed);
    }
}

pub struct CommandRegistrar {
    pub commands:
        HashMap<&'static str, (fn(&CommandMap) -> CommandOutput, CreateApplicationCommand)>,
}
impl CommandRegistrar {
    pub(crate) fn register(&mut self, registrant: CommandRegistrant) {
        self.commands
            .insert(registrant.0, (registrant.1, registrant.2));
    }
    pub(crate) async fn handle(&self, ctx: Context, interaction: Interaction) {
        if let Interaction::ApplicationCommand(command) = interaction {
            let name = command.data.name.clone();
            info!("Handling {}", name);
            let start = SystemTime::now();

            match self.commands.get(name.as_str()) {
                Some(exec) => {
                    info!("Executing '{}'", &name);
                    let data = command.data.clone();
                    let command_ctx = CommandContext {
                        interaction: command,
                        command: (*exec).1.clone(),
                    };
                    let map = CommandMap::from(&data);
                    let m = ((*exec).0)(&map);
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
