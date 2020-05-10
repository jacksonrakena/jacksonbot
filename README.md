# 🎀 Abyss
A niche little Discord bot, written in C# using .NET Core.
  
| Prefix     | Developer | Runtime   | Library                                    |
|------------|-----------|--------------------|--------------------------------------------|
| $$         | [🎀 Abyssal](https://github.com/abyssal) | .NET | [Disqord](https://github.com/Quahu/Disqord) | 
  
### 🎉 Features
- Spotify track and album lookup, via [SpotiNET](https://github.com/abyssal/SpotiNET).
- Bicubic resizing of emojis and custom images, both animated and not-animated.
- Live C# script evaluation. (owner-only)
- Dice rolling with DND-style expressions (e.g. `a.roll d20+d48+d10`).
- Customizable runtime options
  
### 👮‍ Requirements
- A [Discord bot application](https://discordapp.com/developers/applications/) with registered user and token (app -> Bot -> Add Bot)
- `appsettings.json` configuration file, as described in **⚙ Configuration**
  
### 🔧 Setup
You can use the default runtime (`Abyss.Runtimes.Default`), or create your own and reference `Abyss.Core`.

### ⚙ Configuration
An example Abyss configuration file can be found at [appsettings.example.json](appsettings.example.json), which should be renamed to `appsettings.json` before running. This needs to be placed in the Abyss base directory.

### 🛠 Structure
The project is broken down into the following projects:     
**Platform core** 
- 🎀 `Abyss` (library) The core of Abyss. This project contains the message receiver, bot host, argument parsers, checks, contexts, and commands.

**Web host**  
- 🪐 `Abyss.Runtimes.Default` (app) This is the Abyss basic runtime, which wraps `Abyss.Core` and runs the bot.  
  
### 🖋 Copyright
Copyright (c) 2018-2020 Abyssal under the MIT License, available at [the LICENSE file.](LICENSE.md)  
