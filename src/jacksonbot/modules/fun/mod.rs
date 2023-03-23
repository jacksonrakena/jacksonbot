use serenity::model::application::command::CommandOptionType;
use serenity::model::prelude::interaction::application_command::CommandDataOptionValue;
use crate::{CommandExecContext, CommandResult};
use crate::CommandResult::Text;
use crate::jacksonbot::infra::module::{make_module, Module};
use rand::prelude::*;

pub fn fun_module() -> Module {
    make_module("fun", |reg| {
        reg.register_simple("roll", "Rolls some dice.", roll_dice);
        reg.register_custom(|attr| {
            attr.name("roll").description("Rolls a dice.").create_option(|opt| {
                opt.name("dice").description("The dice you want to roll").kind(CommandOptionType::Integer).required(true)
            });
        }, roll_dice);
    })
}

fn roll_dice(ctx: &mut CommandExecContext) {
    if let CommandDataOptionValue::Integer(i) = ctx.interaction.data.options.get(0).expect("").resolved.as_ref().expect("expected text") {
        let r: f32 = rand::thread_rng().gen();
        ctx.ok(Text(format!("rolling {}", (r*(*i as f32)).ceil())))
    }
}