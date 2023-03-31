use crate::infra::command::CommandOutput;
use crate::infra::command::CommandResult::Text;
use rand::prelude::*;
use std::fs::File;

use crate::infra::registry2::{command, commands, CommandRegistrar};

pub fn fun_module(registry: &mut CommandRegistrar) {
    commands!({
        registry.register(command!(
            [description="Roll some dice."] roll,
            [description="The dice you'd like to roll." max_value=60] dice i64,
            @roll_dice));
    });
}

fn roll_dice(dice_size: i64) -> CommandOutput {
    let r: f32 = thread_rng().gen();
    let res = (dice_size as f32) * r;

    Ok(Text(format!(
        ":game_die: I rolled {} on a **{}**-sided die.",
        res.ceil(),
        dice_size
    )))
}
