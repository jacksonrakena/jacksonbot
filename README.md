[![Discord](https://img.shields.io/discord/598437365891203072.svg?style=plastic)](https://discord.gg/RsRps9M)
# Abyss Bot Platform

A **fully modular, expandable, open-source (for life)** Discord bot and platform, written in C# using .NET Core and Discord.Net.  
You can add the public instance, running on the default addon set, [here.](https://discordapp.com/api/oauth2/authorize?client_id=532099058941034498&permissions=0&scope=bot)  
You can join the support server [here.](https://discord.gg/RsRps9M)
  
### Features
- Complete modularity including custom event hooks, custom commands (using the existing command system), and full expandability, through .NET's powerful assembly loading system   
- Spotify track and album lookup (can also read from the current song you're listening to), powered by [AbyssalSpotify](http://github.com/abyssal512/AbyssalSpotify)  
![Spotify features](https://jessica.is-pretty.cool/2Xtz3D8.png)
- Resizing (bicubic) of emojis and custom images, both animated and not-animated  
![Bigmoji](https://jessica.is-pretty.cool/6GxxvCX.png)
- Live C# script evaluation  
![Script eval](https://jessica.is-pretty.cool/yqDJqbU.png)
- Dice rolling with custom expression support (e.g. `a.roll d20+d48+d10`)  
![Dice roll](https://jessica.is-pretty.cool/5j9rGtr.png)
- C a t commands  
![Meow](https://jessica.is-pretty.cool/A5r2nws.png)
- General purpose command set

  
### Requirements
- .NET Core 3.0 Preview 7 SDK for building (or Runtime for a pre-compiled version)
- A Discord bot application with registered user and token
- `Abyss.json` configuration file set out as below

### Example config file
Here's an example Abyss configuration file, taken from my main public instance.
```json
{
    "Name": "Abyss",
    "CommandPrefix": "a.",
    "Startup": {
      "Activity": [
        {
          "Type": "Watching",
          "Message": "you <3"
        }
      ]
      },
    "Connections": {
      "Discord": {
        "Token": "Discord bot user token",
        "SupportServer": "An invite to the bot's home base. Optional.",
        "SupportServerId": "The ID of your support server. Optional."
      },
      "Spotify": {
        "ClientId": "Spotify client ID",
        "ClientSecret": "Spotify client secret"
      }
    },
    "Notifications": {
      "Ready": 598437593721602068,
      "ServerMembershipChange": 598437593721602068,
      "Feedback": 600565543010828288
    }
  }

```
This produces a result that looks like this:   
![Abyss: Watching you](https://i.imgur.com/TkX7Eat.png)  
The bot will rotate through each Activity provided under the Startup.Activity property every minute. Available Activity types are Playing, Streaming, Listening, and Watching. (These match to the [enum ActivityType](https://docs.stillu.cc/api/Discord.ActivityType.html) in Discord.Net)  
  
### Modularity
Abyss has fully modular runtime assembly support. Here's a quick guide on doing so:
1) Clone this repository to your computer.
2) Create a new .NET Core 3.0 Library (**not .NET Standard 2.0**) and add your local copy of `Abyss.Core` as a dependency.
3) Create your modules and commands as you like, using `AbyssModuleBase`. Feel free to look at Abyss' included commands for help.
4) Build `Abyss.Console` (or whatever frontend you are using) in your preferred configuration.
5) Build your assembly, and copy the assembly file (something like `MyCommandAssembly.dll`) into the `CustomAssemblies` folder. This will be in `src/Abyss.Console/bin/<your_release_configuration>/netcoreapp3.0/CustomAssemblies`.
6) Start Abyss with `dotnet run --project src/Abyss.Console/Abyss.Console.csproj -c <Your_Configuration>`, and check that the `MessageProcessor` loaded your assembly. It should look like this:
![Assembly loading](https://i.imgur.com/PZqeY7s.png)
7) Enjoy your custom modules! They should be available in the help command.
8) If there are any issues with this process, feel free to hop in the [support server](https://discord.gg/RsRps9M) for help.

### Contributing
The project is broken down into the following projects: 
- `Abyss.Core` (library) The core of Abyss. This project contains the robust, fast, and safe architecture that sits at the heart of Abyss operation. It also contains all of the default commands that come with every instance.
- `Abyss.ExampleCustomAssembly` (library) This is an example project, which shows how you can make your own projects to expand and add new functionality and commands to Abyss.
- `Abyss.Console` This is an executable which wraps `Abyss.Core`, and pipes output to the console. This executable does not support web functionality.
- `Abyss.Web.Shared` This library contains data classes and code that is shared between `Abyss.Web.Client` and `Abyss.Web.Server`.
- `Abyss.Web.Client` This contains all the Blazor clientside code for using the Abyss administration panel.
- `Abyss.Web.Server` An alternative to `Abyss.Console`, this executable starts up the Abyss administration panel on `[Any IP]:2003` and serves clients the Blazor code in `Abyss.Web.Client`. This web server contains many useful administration tools.

### Copyright
Copyright (c) 2019 abyssal512 under the MIT License, available at [the LICENSE file.](LICENSE.md)
