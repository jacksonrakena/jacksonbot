use serenity::model::prelude::interaction::application_command::CommandDataOptionValue;
use serenity::model::prelude::User;

/// This trait is implemented for all valid command parameter types.
/// It creates a Self from an `Option<CommandDataOptionValue>`.
///
/// Note that the option is technically guaranteed to be Some(T)
/// in all instances except where Self is `Option<T>`, in which case it could
/// be None if the parameter is optional and was not supplied by the user.
///
/// All other types (i32, String, etc) are guaranteed to be Some(T) because they are not
/// optional.
pub trait FromCommandParameterValue {
    /// Returns a `Self` value from a wrapped `Option<CommandDataOptionValue>`.
    /// This method should panic if `Self` is not `Option<T>` and `value` is `None`.
    fn from_command_parameter_value(value: Option<CommandDataOptionValue>) -> Self;
}

fn parameter_value_guard<T>(name: &'static str) -> T {
    panic!(
        "Expected a {}, received something else. Commands could possibly be out-of-date.",
        name
    )
}

impl<T> FromCommandParameterValue for Option<T>
where
    T: FromCommandParameterValue,
{
    fn from_command_parameter_value(value: Option<CommandDataOptionValue>) -> Self {
        match value {
            None => None,
            Some(x) => Some(T::from_command_parameter_value(Some(x))),
        }
    }
}

impl FromCommandParameterValue for String {
    fn from_command_parameter_value(value: Option<CommandDataOptionValue>) -> Self {
        match value {
            Some(CommandDataOptionValue::String(t)) => t,
            _ => parameter_value_guard("String"),
        }
    }
}

impl FromCommandParameterValue for i64 {
    fn from_command_parameter_value(value: Option<CommandDataOptionValue>) -> Self {
        match value {
            Some(CommandDataOptionValue::Integer(t)) => t,
            _ => parameter_value_guard("i64"),
        }
    }
}

impl FromCommandParameterValue for User {
    fn from_command_parameter_value(value: Option<CommandDataOptionValue>) -> Self {
        match value {
            Some(CommandDataOptionValue::User(t, ..)) => t,
            _ => parameter_value_guard("User"),
        }
    }
}

impl FromCommandParameterValue for bool {
    fn from_command_parameter_value(value: Option<CommandDataOptionValue>) -> Self {
        match value {
            Some(CommandDataOptionValue::Boolean(t, ..)) => t,
            _ => parameter_value_guard("bool"),
        }
    }
}
