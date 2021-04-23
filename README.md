# 👨🏻‍🔧 The Overseer
A Discord-focused, multi-platform chat and administration bot for the communities you love.
Built on top of [Disqord](https://github.com/Quahu/Disqord) and [Qmmands](https://github.com/Quahu/Qmmands).
Mesh communication is powered by [Pixie](https://github.com/jacksonrakena/pixie).

<img src="https://i.imgur.com/DF1ZIs2.png" height="150" />

| Prefix | Developer | Runtime | Library | Version | Platform | Invite |
|-|-|-|-|-|-|-|
| // | [Abyssal](https://github.com/jacksonrakena) | JRE 1.8 | [Disqord](https://github.com/Quahu/Disqord)  | 18.1 | .NET | Not yet
  
### 👮‍ Requirements
- A [Discord bot application](https://discordapp.com/developers/applications/) with registered user and token (app -> Bot -> Add Bot)

### 🛠 Structure
The project is broken down into the following domains:     
**Core** 
- 💚 `com.abyssaldev.abyss` The core of Abyss. This project contains initialisation logic for the interaction controller and the gateway.
  
**Persistence**
- 📜 `com.abyssaldev.abyss.persistence` This package contains Abyss' persistence logic, including its database connections.

**Commands**  
- 🪐 `com.abyssaldev.abyss.commands.gateway` This package contains Abyss' default gateway (`/gw`) command set.
- 🤝 `com.abyssaldev.abyss.framework.interactions` This handles Abyss' interactions (also known as "slash commands"), handled over REST.  
  - `com.abyssaldev.abyss.commands.interactions` This package contains Abyss' default interactions set.
  
### 🖋 Copyright
Copyright (c) 2018-2021 Abyssal under the MIT License, available at [the LICENSE file.](LICENSE.md)  
