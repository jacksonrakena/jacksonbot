use log::Level::Debug;
use serenity::model::application::command::CommandOptionType;
use serenity::model::prelude::interaction::application_command::CommandDataOptionValue;
use crate::{CommandContext};
use crate::jacksonbot::infra::module::{make_module, Module};
use rand::prelude::*;
use serenity::builder::CreateEmbedAuthor;
use serenity::model::user::User;

pub fn user_module() -> Module {
    make_module("user", |reg| {
        reg.register_custom(|attr| {
            attr.name("avatar").description("Grabs the avatar for a user.").create_option(|opt| {
                opt.name("user").description("The user who you wish to get the avatar for.").kind(CommandOptionType::Integer).required(true)
            });
        }, get_avatar);
    })
}

fn get_avatar(ctx: &mut CommandContext) {
    let user = match ctx.get_user(0) {
        Some(u) => u,
        None => &ctx.interaction.user
    };
    let avatar_url = make_avatar_url(user, ImageFormat::Png, Some(4096));
    let sizes = make_sizes(&user);
    let title = format!("{}#{}", user.name, user.discriminator);

    ctx.ok_embed(|emb|{
        let mut author = CreateEmbedAuthor::default();
        author.name(&title).icon_url(&avatar_url);

        emb
            .set_author(author)
            .description(&sizes)
            .image(&avatar_url);
    });
}
fn make_sizes(user: &User) -> String {
    format!("**Sizes:** [128]({}) | [256]({}) | [1024]({}) | [2048]({}) \n **Formats:** [PNG]({}) | [JPG]({}) | [WEBP]({}) | [GIF]({})",
            make_avatar_url(user, ImageFormat::Png, Some(128)),
            make_avatar_url(user, ImageFormat::Png, Some(256)),
            make_avatar_url(user, ImageFormat::Png, Some(1024)),
            make_avatar_url(user, ImageFormat::Png, Some(2048)),

            make_avatar_url(user, ImageFormat::Png, Some(1024)),
            make_avatar_url(user, ImageFormat::Jpeg, Some(1024)),
            make_avatar_url(user, ImageFormat::WebP, Some(1024)),
            make_avatar_url(user, ImageFormat::Gif, Some(1024)),
    )
}

#[derive(Debug, PartialEq)]
enum ImageFormat{
    Png,
    Jpeg,
    WebP,
    Gif
}
/// Generates an avatar URL for a user. Will check for an empty avatar, and will instead provide the URL for that user's default.
fn make_avatar_url(user: &User, format: ImageFormat, size: Option<u16>) -> String{
    let size_str = match size {
        Some(t) => format!("?size={}", t),
        None => "".to_string()
    };

    if let Some(hash) = &user.avatar {
        let mut fmt = format;

        if hash.starts_with("a_") { fmt = ImageFormat::Gif; }
        if !hash.starts_with("a_") && fmt == ImageFormat::Gif { fmt = ImageFormat::Png }

        return format!("https://cdn.discordapp.com/avatars/{}/{}.{}{}", user.id.as_u64(), hash, format!("{:?}", fmt).to_lowercase(), size_str);
    }
    return format!("https://cdn.discordapp.com/embed/avatars/{}.{}{}", user.discriminator % 5, format!("{:?}", format).to_lowercase(), size_str)
}
