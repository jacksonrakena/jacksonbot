use crate::jacksonbot::infra::command::CommandOutput;
use crate::jacksonbot::infra::module::{make_module, Module};
use crate::jacksonbot::modules::user::avatar::get_avatar;
use crate::jacksonbot::modules::user::hex::get_hex;
use crate::CommandContext;
use log::Level::Debug;
use rand::prelude::*;
use serenity::builder::CreateEmbedAuthor;
use serenity::model::application::command::CommandOptionType;
use serenity::model::prelude::interaction::application_command::CommandDataOptionValue;
use serenity::model::user::User;

mod avatar;
mod hex;

pub fn user_module() -> Module {
    make_module("user", |reg| {
        reg.register_custom(
            |attr| {
                attr.name("avatar")
                    .description("Grabs the avatar for a user.")
                    .create_option(|opt| {
                        opt.name("user")
                            .description("The user who you wish to get the avatar for.")
                            .kind(CommandOptionType::Integer)
                            .required(true)
                    });
            },
            get_avatar,
        );

        reg.register_custom(
            |attr| {
                attr.name("hex")
                    .description("Parses a color.")
                    .create_option(|col| {
                        col.name("color")
                            .description("A hex value, like #C21ABF.")
                            .required(true)
                            .kind(CommandOptionType::String)
                    });
            },
            get_hex,
        );
    })
}
