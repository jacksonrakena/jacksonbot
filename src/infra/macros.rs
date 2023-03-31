macro_rules! command {
    (
        $([  $($cmd_attr_name:ident=$cmd_attr_value:expr)*  ])? $cmd_name: literal,
        $description: literal,
        $( $([  $($param_attr_name:ident=$param_attr_value:expr)*  ])?  $param_name:ident: $param_type:ty,)*
        @$block: ident) => {
        {
            use crate::infra::command::CommandOutput;
            use crate::infra::execution::CommandContext;
            use crate::infra::make_command::make_command;
            use crate::infra::registry::{CommandMap, CommandParameter};
            use std::collections::HashMap;

            // This ignores when cmd_attrs is empty (no attributes)
            #[allow(unused_mut)]

            // Compile command-level attributes
            let mut cmd_attrs = HashMap::new();
            $(
                $(
                    cmd_attrs.insert(stringify!($cmd_attr_name), $cmd_attr_value.to_string()); // buh?
                )*
            )?

            // Compile parameters
            let mut params = Vec::<CommandParameter>::new();
            $(
                // Parameter-level attributes
                let mut attrs = HashMap::new();
                $(
                    $(
                        attrs.insert(stringify!($param_attr_name), $param_attr_value.to_string());
                    )*
                )?
                params.push(CommandParameter {
                    ty: stringify!($param_type),
                    attrs: attrs,
                    name: stringify!($param_name)
                });
            )*

            fn invoke (ctx: &CommandContext, map: &CommandMap) -> CommandOutput {
                $block(ctx, $(map.get::<$param_type>(stringify!($param_name)),)*)
            }
            make_command($cmd_name, $description, cmd_attrs, params, invoke)
        }
    }
}
pub(crate) use command;
