# 💚 Abyss
A Discord bot, capable of both interactions (slash-commands) and gateway commands (with [JDA](https://github.com/DV8FromTheWorld/JDA))
  
| Prefix     | Developer | Runtime   | Library                                    | Version | Platform |
|------------|-----------|--------------------|--------------------------------------------|-|-|
| /          | [Abyssal](https://github.com/abyssal) | JRE 1.8 | Custom & [JDA](https://github.com/DV8FromTheWorld/JDA) | 16.1 | Gradle |
  
### 👮‍ Requirements
- A [Discord bot application](https://discordapp.com/developers/applications/) with registered user and token (app -> Bot -> Add Bot)
    - Interactions enabled, pointing to Abyss' `/discord/interactions` endpoint (configurable)
- A reverse proxy or middleman to apply SSL from 443 to Abyss' configurable port (Nginx recommended - [example config](example_nginx_server_conf.nginx))
- `appconfig.json` configuration file

### 🛠 Structure
The project is broken down into the following domains:     
**Core** 
- 💚 `com.abyssaldev.abyss` The core of Abyss. This project contains initialisation logic for the interaction controller and the gateway.
  
**Persistence**
- 📜 `com.abyssaldev.abyss.persistence` This package contains Abyss' persistence logic, including its database connections.

**Gateway**  
- 🪐 `com.abyssaldev.abyss.gateway` This handles Abyss' behaviour over the WebSocket gateway, including voice and traditional commands.
- 🔊 `com.abyssaldev.abyss.gateway.voice` This contains Abyss' experimental music playing logic.
  
**Interactions & Commands**
- 🤝 `com.abyssaldev.abyss.interactions` This handles Abyss' interactions (also known as "slash commands"), handled over REST.  
- ⚙ `com.abyssaldev.abyss.interactions.framework` This package contains the logic for processing interactions and interactive commands.
- 🎫 `com.abyssaldev.abyss.interactions.commands` This package contains Abyss' stock commands and their logic.  
- 🧼 `com.abyssaldev.abyss.interactions.http` This package contains the Ktor routes and facilities to process Discord POST requests.
  
### 🖋 Copyright
Copyright (c) 2018-2020 Abyssal under the MIT License, available at [the LICENSE file.](LICENSE.md)  
