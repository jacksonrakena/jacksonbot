use crate::infra::module::{make_module, Module};

pub fn profile_module() -> Module {
    make_module("profile", |reg| {})
}
