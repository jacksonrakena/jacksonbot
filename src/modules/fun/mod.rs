use commands_lib::command::CommandOutput;
use commands_lib::command::CommandResponse::Text;
use commands_lib::execution::CommandContext;
use commands_lib::macros::command;
use rand::prelude::*;

use commands_lib::registry::CommandRegistrar;
use commands_lib::text;

pub fn fun_module(registry: &mut CommandRegistrar) {
    registry.register(command!(
        "roll", "Roll some dice.",
        [description="The dice you'd like to roll." max_value=60] dice: i64,
        @roll_dice));
}

fn roll_dice(_ctx: &CommandContext, dice_size: i64) -> CommandOutput {
    let r: f32 = thread_rng().gen();
    let res = (dice_size as f32) * r;

    text!(
        ":game_die: I rolled {} on a **{}**-sided die.",
        res.ceil(),
        dice_size
    )
}
