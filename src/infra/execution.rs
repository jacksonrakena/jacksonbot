use crate::infra::command::CommandResult::{Embed, Text};
use crate::infra::command::{CommandError, CommandExecutable, CommandOutput, CommandResult};
use crate::infra::registry::CommandRegistry;
use serenity::builder::CreateEmbed;
use serenity::model::prelude::interaction::application_command::{
    ApplicationCommandInteraction, CommandDataOption, CommandDataOptionValue,
};
use serenity::model::prelude::PartialMember;
use serenity::model::prelude::User;

pub struct CommandContext<'a> {
    pub(crate) interaction: ApplicationCommandInteraction,
    pub(crate) command: &'a CommandExecutable,
}
impl<'a> CommandContext<'a> {
    fn arguments(&self) -> &Vec<CommandDataOption> {
        &self.interaction.data.options
    }

    pub(crate) fn get_i64(&self, index: usize) -> Option<i64> {
        self.get_optional(index).and_then(|val| {
            if let CommandDataOptionValue::Integer(i) = val {
                Some(*i)
            } else {
                None
            }
        })
    }

    pub(crate) fn get_string(&self, index: usize) -> Option<&String> {
        self.get_optional(index).and_then(|val| {
            if let CommandDataOptionValue::String(i) = val {
                Some(i)
            } else {
                None
            }
        })
    }

    pub(crate) fn get_user(&self, index: usize) -> Option<&User> {
        self.get_optional(index).and_then(|val| {
            if let CommandDataOptionValue::User(u, m) = val {
                Some(u)
            } else {
                None
            }
        })
    }

    pub(crate) fn get_member(&self, index: usize) -> Option<&PartialMember> {
        return match self.get_optional(index) {
            Some(CommandDataOptionValue::User(user, member)) => {
                if member.is_none() {
                    return None;
                }
                return Some(member.as_ref().unwrap());
            }
            None => None,
            _ => panic!("requested member for parameter {}", index),
        };
    }

    pub(crate) fn get_optional(&self, index: usize) -> Option<&CommandDataOptionValue> {
        let data = self.interaction.data.options.get(index);
        if data.is_none() || data.unwrap().resolved.is_none() {
            return Option::None;
        }
        return data.unwrap().resolved.as_ref();
    }

    pub(crate) fn get_required(&self, index: usize) -> &CommandDataOptionValue {
        return self
            .interaction
            .data
            .options
            .get(index)
            .expect("expected variable")
            .resolved
            .as_ref()
            .unwrap();
    }

    pub(crate) fn text<D: ToString>(text: D) -> CommandOutput {
        Ok(Text(text.to_string()))
    }

    pub(crate) fn embed<F>(&self, editor: F) -> CommandOutput
    where
        F: Fn(&mut CreateEmbed),
    {
        let mut embed = CreateEmbed::default();
        (editor)(&mut embed);
        Ok(Embed(embed))
    }

    pub(crate) fn err<F: ToString>(&self, message: F) -> CommandOutput {
        let err = CommandError::new(message);
        Err(err)
    }
}
