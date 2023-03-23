use crate::CommandRegistry;

pub fn make_module<D: ToString, F: Fn(&mut CommandRegistry) + Send + Sync + 'static>(name: D, registry_accessor: F) -> Module {
    Module {
        registrant: Box::new(registry_accessor)
    }
}

pub struct Module {
    pub(crate) registrant: Box<dyn Fn(&mut CommandRegistry) + Send + Sync + 'static>
}