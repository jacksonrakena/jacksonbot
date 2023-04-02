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
pub trait ParameterValue {
    /// Returns a `Self` value from a wrapped `Option<CommandDataOptionValue>`.
    /// This method should panic if `Self` is not `Option<T>` and `value` is `None`.
    fn get_value(value: Option<CommandDataOptionValue>) -> Self;
}

fn parameter_value_guard<T>(name: &'static str) -> T {
    panic!(
        "Expected a {}, received something else. Commands could possibly be out-of-date.",
        name
    )
}

impl<T> ParameterValue for Option<T>
where
    T: ParameterValue,
{
    fn get_value(value: Option<CommandDataOptionValue>) -> Self {
        match value {
            None => None,
            Some(x) => Some(T::get_value(Some(x))),
        }
    }
}

impl ParameterValue for String {
    fn get_value(value: Option<CommandDataOptionValue>) -> Self {
        match value {
            Some(CommandDataOptionValue::String(t)) => t,
            _ => parameter_value_guard("String"),
        }
    }
}

impl ParameterValue for i64 {
    fn get_value(value: Option<CommandDataOptionValue>) -> Self {
        match value {
            Some(CommandDataOptionValue::Integer(t)) => t,
            _ => parameter_value_guard("i64"),
        }
    }
}

impl ParameterValue for User {
    fn get_value(value: Option<CommandDataOptionValue>) -> Self {
        match value {
            Some(CommandDataOptionValue::User(t, ..)) => t,
            _ => parameter_value_guard("User"),
        }
    }
}

impl ParameterValue for bool {
    fn get_value(value: Option<CommandDataOptionValue>) -> Self {
        match value {
            Some(CommandDataOptionValue::Boolean(t, ..)) => t,
            _ => parameter_value_guard("bool"),
        }
    }
}
