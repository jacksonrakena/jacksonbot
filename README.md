[![DBL Big](https://discordbots.org/api/widget/532099058941034498.svg)](https://discordbots.org/bot/532099058941034498)   
 
# 🎀 Abyss
A **fully modular, expandable, open-source (for life)** Discord bot, written in C# using .NET Core.
  
| Prefix     | Developer | Language/Runtime   | Library                                    | Add To Server  | List links |
|------------|-----------|--------------------|--------------------------------------------|----------------| ---------- |
| a.         | 🎀 Abyssal | .NET Core 3.0 | [Disqord](https://github.com/Quahu/Disqord)| [Authorize](https://discordapp.com/api/oauth2/authorize?client_id=532099058941034498&permissions=0&scope=bot) | [dbots](https://discord.bots.gg/bots/532099058941034498), [top.gg](https://top.gg/bot/532099058941034498), [DBL](https://discordbotlist.com/bots/532099058941034498), [dboats](https://discord.boats/bot/532099058941034498), [bod](https://bots.ondiscord.xyz/bots/532099058941034498)
  
### 🎉 Features
**Core framework**
- In-depth command support, making it super simple to create your own hosts and command sets.
- Super powerful commands system, with custom results and expandable command packs.
- ASP.NET-style result system, including Ok, Bad Request, React With Thumbs Up, and Reply With Thumbs Up.
- Advanced response formatting.
- Positional and UNIX-style parameter parsing.

**Included commands pack**
- Spotify track and album lookup, via [AbyssalSpotify](http://github.com/abyssal/AbyssalSpotify).
- Bicubic resizing of emojis and custom images, both animated and not-animated.
- Live C# and Handlebars script evaluation. (owner-only)
- Dice rolling with DND-style expressions (e.g. `a.roll d20+d48+d10`).
- Moderation tools, including an advanced UNIX-style customizable message purge.
- A general purpose command set.
- Much more inside!
  
### 👮‍ Requirements
- Docker Engine (17.06.0+) and Docker Compose (3.3+)
- A [Discord bot application](https://discordapp.com/developers/applications/) with registered user and token (app -> Bot -> Add Bot)
- `abyss.json` configuration file, as described in **⚙ Configuration**
  
### 🔧 Setup
Follow the Docker instructions that are [available here.](DOCKER.md)

### ⚙ Configuration
An example Abyss configuration file can be found at [abyss.example.json](abyss.example.json), which should be renamed to `abyss.json` before running. This needs to be placed in the Abyss data root, which is explained in [the Docker instructions.](DOCKER.md)

### 🛠 Structure
The project is broken down into the following projects:     
**Platform core** 
- 🎀 `Abyss` (library) The core of Abyss. This project contains the message receiver, bot host, argument parsers, checks, contexts, results, and type parsers required to make Abyss command packs.
   
**Command packs**
- 🎫 `Abyss.Commands.Default` (library) The default command pack for Abyss. This contains all of the default commands, as well as the Spotify integration support.
  
**Hosts**  
- 🪐 `Abyss.Hosts.Default` (app) This is the default Abyss host, which wraps `Abyss` and `Abyss.Commands.Default` and runs the bot. The host injects services required by the `Abyss.Commands.Default` pack, and imports it through `DefaultPackLoader`.  
  
### 🖋 Copyright
Copyright (c) 2019 Abyssal under the MIT License, available at [the LICENSE file.](LICENSE.md)  
