macro_rules! command {
    (
        $([  $($cmd_attr_name:ident=$cmd_attr_value:expr)*  ])? $cmd_name: ident,
        $( $([  $($param_attr_name:ident=$param_attr_value:expr)*  ])?  $param_name:ident $param_type:ty,)*
        @$block: ident) => {
        {
            use crate::infra::command::CommandOutput;
            use crate::infra::execution::CommandContext;
            use crate::infra::make_command::make_command;
            use crate::infra::registry2::{CommandMap, CommandParameter};
            #[allow(dead_code, unused, unused_variables)]
            use std::collections::HashMap;

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

            fn $cmd_name (ctx: &CommandContext, map: &CommandMap) -> CommandOutput {
                $block(ctx, $(map.get::<$param_type>(stringify!($param_name)),)*)
            }
            make_command(stringify!($cmd_name), cmd_attrs, params, $cmd_name)
        }
    }
}
pub(crate) use command;
