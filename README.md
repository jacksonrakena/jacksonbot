<div align="center">
  <img width="100" height="100" src="https://d.lu.je/avatar/679925967153922055" />
  <h1>Jacksonbot </h1>
A general-purpose Discord bot with some cool features. <br />
Built on top of <a href="https://github.com/Quahu/Disqord">Disqord</a> and <a href="https://github.com/Quahu/Qmmands">Qmmands</a>. <br /> 

<br />
<br />
  
| Developer                                          |Library| Version | Platform |
|----------------------------------------------------|-----|---------|----------|
| [@jacksonrakena](https://github.com/jacksonrakena) |[Disqord](https://github.com/Quahu/Disqord)| 20      | .NET 7   |


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
- (Coming soon) Powerful moderation functions, including massbans, warns, autoban, and customisable punishment thresholds 

  
### Requirements
*Who am I kidding? You're not gonna run this anyway.*
- A [Discord bot application](https://discordapp.com/developers/applications/) with registered user and token (app -> Bot -> Add Bot)
- .NET 5.0 Runtime (SDK for building) or newer
- PostgreSQL (tested with 13.1)
    - Modify the connection string in `jacksonbot.appsettings.example.json` to your database

You can use the `tools/jacksonbot.service` file to run Jacksonbot as a `systemd` service. 

Copyright &copy; 2015-2022 jacksonrakena under the MIT License, available at [the LICENSE file.](LICENSE.md)  
