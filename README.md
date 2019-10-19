[![DBL Big](https://discordbots.org/api/widget/532099058941034498.svg)](https://discordbots.org/bot/532099058941034498)   
 
[![DBL Small](https://discordbots.org/api/widget/owner/532099058941034498.svg)](https://discordbots.org/bot/532099058941034498)
# Abyss

A **fully modular, expandable, open-source (for life)** Discord bot, written in C# using .NET Core and Discord.Net.
  
| Prefix | Developer | Language/Runtime | Library | Add To Server
|------------|---|---|---|---|
| a. | Abyssal | C# 8/.NET Core 3.0 | Discord.Net | [Authorize](https://discordapp.com/api/oauth2/authorize?client_id=532099058941034498&permissions=0&scope=bot)
  
### Features
- Spotify track and album lookup, via [AbyssalSpotify](http://github.com/abyssal/AbyssalSpotify).
- Bicubic resizing of emojis and custom images, both animated and not-animated.
- Live C# script evaluation. (owner-only)
- Dice rolling with custom expression support (e.g. `a.roll d20+d48+d10`).
- C a t commands.
- General purpose command set.
- Moderation tools, including an advanced customizable message purge.
- And much more.
  
### Requirements
- Docker Engine (17.06.0+) and Docker Compose (3.3+)
- A [Discord bot application](https://discordapp.com/developers/applications/) with registered user and token (app -> Bot -> Add Bot)
- `abyss.json` configuration file set out as below  
  
### Setup
Follow the Docker instructions that are [available here.](DOCKER.md)

### Configuration
An example Abyss configuration file can be found at [abyss.example.json](abyss.example.json), which should be renamed to `abyss.json` before running. This needs to be mounted in Abyss' data root, which is explained in [the Docker instructions.](DOCKER.md)

### Structure
The project is broken down into the following projects:     
**Platform core** 
- `Abyss` (library) The core of Abyss. This project contains the robust, fast, and safe architecture that sits at the heart of Abyss operation.
   
**Default command packs**
- `Abyss.Commands.Default` (library) The default command pack for Abyss, this contains all of the default commands.  
  
**Web host**  
- `Abyss.Hosts.Default` This is the Abyss web server, which wraps `Abyss` and `Abyss.Commands.Default` and provides web server functionality, as well as running the bot.  
  
### Copyright
Copyright (c) 2019 Abyssal under the MIT License, available at [the LICENSE file.](LICENSE.md)  