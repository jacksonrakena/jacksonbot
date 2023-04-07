use commands_lib::execution::CommandContext;
use commands_lib::execution::CommandOutput;
use commands_lib::execution::CommandResponse::Embed;
use serenity::builder::{CreateEmbed, CreateEmbedAuthor};
use serenity::model::prelude::User;
use std::fmt::{Display, Formatter};

pub(crate) fn get_avatar(ctx: &CommandContext, user: Option<User>) -> CommandOutput {
    let u = user.as_ref().unwrap_or(&ctx.interaction.user);
    let avatar_url = make_avatar_url(u, ImageFormat::Png, Some(4096));
    let sizes = make_sizes(u);
    let title = format!("{}#{}", u.name, u.discriminator);

    let mut emb = CreateEmbed::default();

    let mut author = CreateEmbedAuthor::default();
    author.name(&title).icon_url(&avatar_url);

    emb.set_author(author)
        .description(&sizes)
        .image(&avatar_url);

    Ok(Embed(emb))
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

#[derive(Debug, PartialEq, Copy, Clone)]
enum ImageFormat {
    Png,
    Jpeg,
    WebP,
    Gif,
}

impl Display for ImageFormat {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", format!("{:?}", self).to_lowercase())
    }
}

/// Generates an avatar URL for a user. Will check for an empty avatar, and will instead provide the URL for that user's default.
fn make_avatar_url(user: &User, mut format: ImageFormat, size: Option<u16>) -> String {
    let size_str = size.map_or("".to_string(), |size| format!("?size={}", size));

    match &user.avatar {
        Some(hash) => {
            if hash.starts_with("a_") {
                format = ImageFormat::Gif;
            }
            if !hash.starts_with("a_") && format == ImageFormat::Gif {
                format = ImageFormat::Png
            }

            format!(
                "https://cdn.discordapp.com/avatars/{}/{}.{}{}",
                user.id.as_u64(),
                hash,
                format,
                size_str
            )
        }
        None => {
            format!(
                "https://cdn.discordapp.com/embed/avatars/{}.{}{}",
                user.discriminator % 5,
                format,
                size_str
            )
        }
    }
}
