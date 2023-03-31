<div align="center">
  <img width="100" height="100" src="https://d.lu.je/avatar/679925967153922055.png" />
  <h1>Jacksonbot </h1>
A general-purpose Discord bot with some cool features. <br />
Built with <a href="https://github.com/serenity-rs/serenity">Serenity</a> and Rust. <br /> 

<br />
<br />

| Developer                                          | Library                                             | Version | Platform |
|----------------------------------------------------|-----------------------------------------------------|---------|----------|
| [@jacksonrakena](https://github.com/jacksonrakena) | [Serenity](https://github.com/serenity-rs/serenity) | 21      | Rust     |


<br />
</div>

### Features & Roadmap
Very few commands. This is a technical test for Jacksonbot version 21.


### Infrastructure
Commands are defined with the following syntax:
```rust
pub fn fun_module(registry: &mut CommandRegistrar) {
    commands!({
        registry.register(command!(
            [description="Roll some dice."] roll,
            [description="The dice you'd like to roll." max_value=60] dice i64,
            @roll_dice));
    });
}
```
where `roll_dice` is the name of a function with signature `roll_dice(&ctx: CommandContext, dice: i64) -> CommandOutput`.

Jacksonbot's macro system will automatically re-interpret the signature provided to `commmand!` and 
rewrite the command to provide those values at runtime, as well as passing the attributes `[description=, max_value=]` to 
Discord when registering commands.

A command can take an optional argument, simply by changing the type to `Option<T>`.
### Requirements
*Who am I kidding? You're not gonna run this anyway.*
- A [Discord bot application](https://discordapp.com/developers/applications/) with registered user and token (app -> Bot -> Add Bot)
- Rust (compiler version supporting edition 2021 or newer)
- PostgreSQL (tested with 13.1)
    - Modify the connection string in `jacksonbot.appsettings.example.json` to your database

You can use the `tools/jacksonbot.service` file to run Jacksonbot as a `systemd` service.

Copyright &copy; 2015-2022 jacksonrakena under the MIT License, available at [the LICENSE file.](LICENSE.md)  