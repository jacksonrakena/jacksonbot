use commands_lib::command::{embed, err, CommandError, CommandOutput};
use commands_lib::execution::CommandContext;

pub(crate) fn get_hex(_ctx: &CommandContext, val: String) -> CommandOutput {
    match i32::from_str_radix(val.trim_start_matches("#"), 16) {
        Ok(i) => embed(|emb| {
            emb.title(format!("Color: #{}", val))
                .image(format!("https://singlecolorimage.com/get/{}/200x200", val))
                .color(i);
        }),
        Err(_) => err!("can't parse that color"),
    }
}
