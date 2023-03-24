use crate::jacksonbot::infra::command::{CommandError, CommandOutput};
use crate::jacksonbot::infra::execution::CommandContext;
use serenity::builder::CreateEmbed;

pub(crate) fn get_hex(ctx: &mut CommandContext) -> CommandOutput {
    let val = ctx.get_string(0).unwrap().trim_start_matches("#");

    match i32::from_str_radix(val, 16) {
        Ok(i) => ctx.embed(|emb| {
            emb.title(format!("Color: #{}", val))
                .image(format!("https://singlecolorimage.com/get/{}/200x200", val))
                .color(i);
        }),
        Err(why) => ctx.err("can't parse that color"),
    }
}
