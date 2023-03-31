use serenity::model::prelude::interaction::application_command::CommandDataOptionValue;
use serenity::model::prelude::User;

pub trait CommandValueCoercable {
    fn get_value(value: Option<CommandDataOptionValue>) -> Self;
}

impl<T> CommandValueCoercable for Option<T>
where
    T: CommandValueCoercable,
{
    fn get_value(value: Option<CommandDataOptionValue>) -> Self {
        match value {
            None => None,
            Some(x) => Some(T::get_value(Some(x))),
        }
    }
}

impl CommandValueCoercable for String {
    fn get_value(value: Option<CommandDataOptionValue>) -> Self {
        if let CommandDataOptionValue::String(t) = value.unwrap() {
            return t;
        }
        panic!("")
    }
}

impl CommandValueCoercable for i64 {
    fn get_value(value: Option<CommandDataOptionValue>) -> Self {
        if let CommandDataOptionValue::Integer(i) = value.unwrap() {
            return i;
        }
        panic!("")
    }
}

impl CommandValueCoercable for User {
    fn get_value(value: Option<CommandDataOptionValue>) -> Self {
        if let CommandDataOptionValue::User(u, ..) = value.unwrap() {
            return u;
        }
        panic!("");
    }
}

impl CommandValueCoercable for bool {
    fn get_value(value: Option<CommandDataOptionValue>) -> Self {
        if let CommandDataOptionValue::Boolean(b) = value.unwrap() {
            return b;
        }
        panic!("");
    }
}
