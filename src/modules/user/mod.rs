use crate::modules::user::avatar::get_avatar;
use crate::modules::user::hex::get_hex;
use commands_lib::macros::command;
use commands_lib::registry::CommandRegistrar;
use serenity::model::user::User;

mod avatar;
mod hex;

pub fn user_module(registry: &mut CommandRegistrar) {
    registry.register(command!(
        "avatar", "Shows an avatar for a user.",
        [description="The user you wish to get the avatar for."] user: Option<User>, @get_avatar));
    registry.register(command!(
            "hex", "Shows a color from a hex value.",
            [description="A hex value, like #C21ABF."] color: String, @get_hex));
}
