# Abyss
[![forthebadge](https://forthebadge.com/images/badges/built-with-love.svg)](https://forthebadge.com)

**A Discord bot.** 
  
  
  
### Requirements
- .NET Core 2.2 SDK for building (or Runtime for a pre-compiled version)
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
        "Token": "Discord bot user token"
      },
      "Spotify": {
        "ClientId": "Spotify client ID",
        "ClientSecret": "Spotify client secret"
      }
    }
  }

```
The bot will rotate through each Activity provided under the Startup.Activity property every minute. Available Activity types are Playing, Streaming, Listening, and Watching.  
  

### Copyright
Copyright (c) 2019 abyssal512 under the MIT License, available at [the LICENSE file.](LICENSE.md)
