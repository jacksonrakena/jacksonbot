[![DBL Big](https://discordbots.org/api/widget/532099058941034498.svg)](https://discordbots.org/bot/532099058941034498)   
 
[![DBL Small](https://discordbots.org/api/widget/owner/532099058941034498.svg)](https://discordbots.org/bot/532099058941034498)
# Abyss

A **fully modular, expandable, open-source (for life)** Discord bot, written in C# using .NET Core and Discord.Net.
  
| Prefix | Developer | Language/Runtime | Library | Add To Server
|------------|---|---|---|---|
| a. | Abyssal | C#/.NET Core 3.0 | Discord.Net | [Authorize](https://discordapp.com/api/oauth2/authorize?client_id=532099058941034498&permissions=0&scope=bot)
  
### Features
- Spotify track and album lookup (can also read from the current song you're listening to), powered by [AbyssalSpotify](http://github.com/abyssal/AbyssalSpotify).
- Resizing (bicubic) of emojis and custom images, both animated and not-animated.
- Live C# script evaluation.
- Dice rolling with custom expression support (e.g. `a.roll d20+d48+d10`).
- C a t commands.
- General purpose command set.
- And much more.

  
### Requirements
- It is heavily recommended to run Abyss on a Docker daemon running with Linux containers. Instructions for running with Docker are [here](DOCKER.md). If you don't want to run on Docker, you're on your own.
- .NET Core 3.0 SDK for building (or Runtime for a pre-compiled version)
- A Discord bot application with registered user and token
- `Abyss.json` configuration file set out as below

### Configuration
An example Abyss configuration file can be found at [abyss.example.json](https://github.com/abyssal/Abyss/blob/master/abyss.example.json), which should be renamed to `abyss.json` before running. This needs to be mounted in Abyss' content root, which is explained in [the Docker instructions.](DOCKER.md)

### Contributing
The project is broken down into the following projects:     
**Platform core** 
- `Abyss.Core` (library) The core of Abyss. This project contains the robust, fast, and safe architecture that sits at the heart of Abyss operation.
   
**Default command packs**
- `Abyss.Commands.Default` (library) The default command pack for Abyss, this contains all of the default commands.  
  
**Web host**  
- `Abyss.Hosts.Default` This is the Abyss web server, which wraps `Abyss.Core` and provides web server functionality, as well as running the bot.  
  
### Copyright
Copyright (c) 2019 Abyssal under the MIT License, available at [the LICENSE file.](LICENSE.md)  
  
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fabyssal512%2FAbyss.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2Fabyssal512%2FAbyss?ref=badge_large)
