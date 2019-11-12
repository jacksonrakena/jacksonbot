[![DBL Big](https://discordbots.org/api/widget/532099058941034498.svg)](https://discordbots.org/bot/532099058941034498)   
 
# 🎀 Abyss
A **useful, open-source (for life)** Discord bot, written in C# using .NET Core.
  
| Prefix     | Developer | Language/Runtime   | Library                                    | Add To Server  | List links |
|------------|-----------|--------------------|--------------------------------------------|----------------| ---------- |
| a.         | 🎀 Abyssal | .NET Core 3.0 | [Disqord](https://github.com/Quahu/Disqord)| [Authorize](https://discordapp.com/api/oauth2/authorize?client_id=532099058941034498&permissions=0&scope=bot) | [dbots](https://discord.bots.gg/bots/532099058941034498), [top.gg](https://top.gg/bot/532099058941034498), [DBL](https://discordbotlist.com/bots/532099058941034498), [dboats](https://discord.boats/bot/532099058941034498), [bod](https://bots.ondiscord.xyz/bots/532099058941034498)
  
### 🎉 Features
- Spotify track and album lookup, via [AbyssalSpotify](http://github.com/abyssal/AbyssalSpotify).
- Bicubic resizing of emojis and custom images, both animated and not-animated.
- Live C# and Handlebars script evaluation. (owner-only, JS support soon!)
- Dice rolling with DND-style expressions (e.g. `a.roll d20+d48+d10`).
- Moderation tools, including hackbans, and an advanced UNIX-style customizable message purge.
- A general purpose command set.
- (Beta) In-depth, feature-complete action log system recording every single thing to happen in your server.
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
- 🎀 `Abyss` (C# Console Application) The core of Abyss. This project contains the message receiver, bot host, argument parsers, checks, contexts, results, and the command set.  
  
### 🖋 Copyright
Copyright (c) 2019 Abyssal under the MIT License, available at [the LICENSE file.](LICENSE.md)  
