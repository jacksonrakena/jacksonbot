use crate::jacksonbot::infra::module::{make_module, Module};
use crate::CommandContext;
use rand::prelude::*;
use serenity::model::application::command::CommandOptionType;
use serenity::model::prelude::interaction::application_command::CommandDataOptionValue;
use std::ptr::null;

pub fn fun_module() -> Module {
    make_module("fun", |reg| {
        reg.register_custom(
            |attr| {
                attr.name("roll")
                    .description("Rolls a dice.")
                    .create_option(|opt| {
                        opt.name("dice")
                            .description("The dice you want to roll")
                            .kind(CommandOptionType::Integer)
                            .required(true)
                    });
            },
            roll_dice,
        );
    })
}

fn roll_dice(ctx: &mut CommandContext) {
    let i = ctx.get_i64(0).unwrap();
    let r: f32 = thread_rng().gen();
    let res = (i as f32) * r;
    ctx.ok_embed(|emb| {
        emb.title("Dice roll")
            .description(format!("rolling {}", res.ceil()));
    });
}
