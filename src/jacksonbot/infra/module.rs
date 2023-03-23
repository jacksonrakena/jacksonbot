use crate::CommandRegistry;

pub fn make_module<D: ToString>(name: D, registry_accessor: fn(&mut CommandRegistry)) -> Module {
    Module {
        registrant: registry_accessor
    }
}

pub struct Module {
    pub(crate) registrant: fn(&mut CommandRegistry)
}