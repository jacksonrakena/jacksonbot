use crate::infra::command::CommandOutput;
use crate::infra::command::CommandResult::Text;
use crate::infra::execution::CommandContext;
use crate::infra::macros::command;
use rand::prelude::*;

use crate::infra::registry::CommandRegistrar;

pub fn fun_module(registry: &mut CommandRegistrar) {
    registry.register(command!(
        "roll", "Roll some dice.",
        [description="The dice you'd like to roll." max_value=60] dice i64,
        @roll_dice));
}

fn roll_dice(_ctx: &CommandContext, dice_size: i64) -> CommandOutput {
    let r: f32 = thread_rng().gen();
    let res = (dice_size as f32) * r;

    Ok(Text(format!(
        ":game_die: I rolled {} on a **{}**-sided die.",
        res.ceil(),
        dice_size
    )))
}
