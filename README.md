<div align="center">
  <h1>Abyss </h1>
A Discord chat and administration bot for the communities you love. <br />
Built on top of <a href="https://github.com/Quahu/Disqord">Disqord</a> and <a href="https://github.com/Quahu/Qmmands">Qmmands</a>. <br /> 

<br />
<br />
  
| Prefix | Developer |  Library | Version | Platform |
|-|-|-|-|-|
| a. | [Abyssal](https://github.com/jacksonrakena) | [Disqord](https://github.com/Quahu/Disqord)  | 19 | .NET 5 |

<br />
</div>

### Features & Roadmap
- Fully integrated accounts system and economy, with fun ways to earn and spend currency
  - Auditable transaction and exchange records, detailing where money is going
- Several minigames including modern Blackjack, trivia questions and slot machines
  - Statistics recorded for all games, allowing users to track progress and compare with other users
  - More games and activities coming soon
- DnD-style dice rolling & RNG, allowing users to roll custom dice (i.e. `d20+d48`)
- Profile system with custom colours, descriptions, and earnable badges
- (Coming soon) Experience system, with points gained through sending messages and engaging with servers
- (Coming soon) [Web portal](https://bot.abyssal.gg) with control panel and statistics
- (Coming soon) Powerful moderation functions, including massbans, warns, autoban, and customisable punishment thresholds 

  
### Requirements
*Who am I kidding? You're not gonna run this anyway.*
- A [Discord bot application](https://discordapp.com/developers/applications/) with registered user and token (app -> Bot -> Add Bot)
- .NET 5.0 Runtime (SDK for building) or newer
- PostgreSQL (tested with 13.1)
    - Username `abyss`, password `abyss123`, empty `abyss` database (databases are migrated automatically) [configurable]

You can use the `tools/abyss.service` file to run Abyss as a `systemd` service. 

Copyright &copy; 2015-2021 Abyssal under the MIT License, available at [the LICENSE file.](LICENSE.md)  
