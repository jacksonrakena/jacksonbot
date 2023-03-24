use crate::infra::command::CommandOutput;
use crate::infra::execution::CommandContext;
use serenity::builder::CreateEmbedAuthor;
use serenity::model::prelude::User;

pub(crate) fn get_avatar(ctx: &mut CommandContext) -> CommandOutput {
    let user = ctx.get_user(0).unwrap_or(&ctx.interaction.user);
    let avatar_url = make_avatar_url(user, ImageFormat::Png, Some(4096));
    let sizes = make_sizes(&user);
    let title = format!("{}#{}", user.name, user.discriminator);

    ctx.embed(|emb| {
        let mut author = CreateEmbedAuthor::default();
        author.name(&title).icon_url(&avatar_url);

        emb.set_author(author)
            .description(&sizes)
            .image(&avatar_url);
    })
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
/// Generates an avatar URL for a user. Will check for an empty avatar, and will instead provide the URL for that user's default.
fn make_avatar_url(user: &User, mut format: ImageFormat, size: Option<u16>) -> String {
    let size_str = match size {
        Some(t) => format!("?size={}", t),
        None => "".to_string(),
    };

    if let Some(hash) = &user.avatar {
        if hash.starts_with("a_") {
            format = ImageFormat::Gif;
        }
        if !hash.starts_with("a_") && format == ImageFormat::Gif {
            format = ImageFormat::Png
        }

        return format!(
            "https://cdn.discordapp.com/avatars/{}/{}.{}{}",
            user.id.as_u64(),
            hash,
            format!("{:?}", format).to_lowercase(),
            size_str
        );
    }

    format!(
        "https://cdn.discordapp.com/embed/avatars/{}.{}{}",
        user.discriminator % 5,
        format!("{:?}", format).to_lowercase(),
        size_str
    )
}
