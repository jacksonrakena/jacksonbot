use serenity::builder::CreateEmbed;
use serenity::model::prelude::interaction::application_command::{ApplicationCommandInteraction, CommandDataOption, CommandDataOptionValue};
use serenity::model::prelude::User;
use serenity::model::prelude::PartialMember;
use crate::jacksonbot::infra::command::CommandResult;
use crate::jacksonbot::infra::command::CommandResult::{Embed, Text};
use crate::jacksonbot::infra::registry::CommandRegistry;

pub struct CommandContext {
    pub(crate) interaction: ApplicationCommandInteraction,
    pub(crate) response: Option<CommandResult>
}
impl CommandContext {
    fn arguments(&self) -> &Vec<CommandDataOption> {
        &self.interaction.data.options
    }

    pub(crate) fn get_i64(&self, index: usize) -> &i64 {
        let val = self.get_required(index);
        if let CommandDataOptionValue::Integer(i) = val { return i; }
        panic!("requested i64 for parameter {}, got: {:#?}", index, val);
    }

    pub(crate) fn get_string(&self, index: usize) -> &String {
        let val = self.get_required(index);
        if let CommandDataOptionValue::String(s) = val { return s; }
        panic!("requested i64 for parameter {}, got: {:#?}", index, val);
    }

    pub(crate) fn get_user(&self, index: usize) -> Option<&User> {
        return match self.get_optional(index) {
            Some(CommandDataOptionValue::User(user, member)) => {
                return Some(user);
            },
            None => None,
            _ => panic!("requested user for parameter {}", index)
        };
    }

    pub(crate) fn get_member(&self, index: usize) -> Option<&PartialMember> {
        return match self.get_optional(index) {
            Some(CommandDataOptionValue::User(user, member)) => {
                if member.is_none() { return None; }
                return Some(member.as_ref().unwrap());
            },
            None => None,
            _ => panic!("requested member for parameter {}", index)
        };
    }

    pub(crate) fn get_optional(&self, index: usize) -> Option<&CommandDataOptionValue> {
        let data = self.interaction.data.options.get(index);
        if data.is_none() || data.unwrap().resolved.is_none() { return Option::None; }
        return data.unwrap().resolved.as_ref();
    }

    pub(crate) fn get_required(&self, index: usize) -> &CommandDataOptionValue {
        return self.interaction.data.options.get(index).expect("expected variable").resolved.as_ref().unwrap();
    }
}

impl CommandContext {
    pub(crate) fn ok_str<D: ToString>(&mut self, text: D) {
        self.response = Some(Text(text.to_string()))
    }
    pub(crate) fn ok_embed<F>(&mut self, editor: F) where F: Fn(&mut CreateEmbed) {
        let mut embed = CreateEmbed::default();
        (editor)(&mut embed);
        self.response = Some(Embed(embed))
    }
}