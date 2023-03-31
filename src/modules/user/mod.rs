use crate::infra::macros::command;
use crate::infra::registry2::CommandRegistrar;
use crate::modules::user::avatar::get_avatar;
use crate::modules::user::hex::get_hex;
use serenity::model::user::User;

mod avatar;
mod hex;

pub fn user_module(registry: &mut CommandRegistrar) {
    registry.register(command!(
        [description="Shows an avatar for a user."] avatar, 
        [description="The user you wish to get the avatar for."] user Option<User>, @get_avatar));
    registry.register(command!(
            [description="Shows a color from a hex value."] hex, 
            [description="A hex value, like #C21ABF."] color String, @get_hex));
}
