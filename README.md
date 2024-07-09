# Mooosic
A discord music bot. 
> [!WARNING]
> No longer maintained due to frequent breakages from youtube side.

# Building
This project requires .Net 7 SDK to be built from source.
Once installed use `dotnet build` in the project directory to produce a binary.
More information on compiling can found here for dotnet [build](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-build), [publish](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-build) and [run](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-run) commands.

# Configuration

The binary requires a configuration file named config.json in its working directory to function.

Here is a sample config file:
```
{
  "discord_token": "YOUR_TOKEN_HERE",
  "admin_ids": ["ADMIN_1", "ADMIN_2"],
  "lavalink": {
    "password": "youshallnotpass"
 },
  "spotify": {
    "secret": "YOUR_SPOTIFY_SECRET",
    "client_id": "YOUR_SPOTIFY_CLIENT_ID"
  }
}
```

Create an application on [discord's  developer platform](https://discord.com/developers/applications) and go to the bot tab to get your token.

> DONOT SHARE ANY TOKENS WITH ANYONE!

Get spotify creds by creting an app [here](https://developer.spotify.com/dashboard)

> You can remove the spotify section if needed but the bot will not accept spotify links

Admin ids are the discord user ids of the users that have full control over the bot. Only these users can invoke the refresh commands.


This bot requires lavalink setup and running. TODO: Document this more...

# Usage
Use @bot_name refresh global to register all the slash commands in all guilds. The bot will react with a thumbs up if the command was typed correctly.
Now wait for upto 5 mins to see all the slash commands available in all the guilds the bot is in.

Use @bot_name refresh guild to do this only for the current guild. This is usually instant and you will see all the commands in only the current guild.

> NOTE:
> It isn't usually reccommended to use the refresh guild command as this will create duplicate entries if the refresh global command was run before or afterwards.

