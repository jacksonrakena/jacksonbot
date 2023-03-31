use crate::infra::registry::{CommandInvokePtr, CommandParameter, CommandRegistration};
use lazy_static::lazy_static;
use regex::Regex;
use serenity::builder::{CreateApplicationCommand, CreateApplicationCommandOption};
use serenity::model::prelude::command::CommandOptionType;
use std::collections::HashMap;

/// Called by the command! macro with the compiled name, description, command attributes,
/// parameters, parameter attributes for each parameter, and the pointer to the generated
/// caller function, which calls the function provided by the developer.
///
/// Returns a CommandRegistration containing all the semantic information and final
/// manifest to be sent to Discord.
pub fn make_command(
    intended_name: &'static str,
    description: &'static str,
    cmd_attrs: HashMap<&'static str, String>,
    params: Vec<CommandParameter>,
    ptr: CommandInvokePtr,
) -> CommandRegistration {
    let mut command = CreateApplicationCommand::default();
    let mut real_name = intended_name.to_string();
    command.description(description);

    // parse command attributes
    for (attr_name, attr_value) in cmd_attrs {
        match attr_name {
            "name" => {
                real_name = attr_value;
            }
            rest => {
                panic!(
                    "unknown command attribute '{}' on command '{}'",
                    rest, real_name
                );
            }
        }
    }

    command.name(real_name.clone());

    // parse parameters
    for param in params {
        command.add_option(make_parameter(&real_name, param));
    }
    CommandRegistration {
        name: real_name,
        manifest: command,
        generated_invoke: ptr,
    }
}

fn make_parameter(command_name: &str, param: CommandParameter) -> CreateApplicationCommandOption {
    lazy_static! {
        static ref OPTIONAL_TYPE_REGEX: Regex = Regex::new(r"Option<(.+)>").unwrap();
    }

    let mut opt = CreateApplicationCommandOption::default();

    let parameter_name = param.name;
    opt.name(parameter_name.clone());
    opt.required(true);

    let attrs = &param.attrs;

    let mut type_name = param.ty;

    // Attempt to parse Option<T>
    if let Some(m) = OPTIONAL_TYPE_REGEX.captures(param.ty) {
        type_name = m.get(1).unwrap().as_str();
        opt.required(false);
    }
    opt.kind(match type_name {
        "String" => CommandOptionType::String,
        "i64" => CommandOptionType::Integer,
        "User" => CommandOptionType::User,
        "bool" => CommandOptionType::Boolean,
        "f64" => CommandOptionType::Number,
        _ => panic!(
            "Invalid type '{}' for parameter '{}' in command '{}'",
            param.ty, parameter_name, command_name
        ),
    });

    if attrs.contains_key("description") {
        opt.description(attrs.get("description").unwrap());
    }

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

    opt
}
