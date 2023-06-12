using Discord.Interactions;
using Discord.WebSocket;

namespace Mooosic.Commands;

public class Ping : InteractionModuleBase
{
    private readonly DiscordSocketClient _client;

    public Ping(DiscordSocketClient client)
    {
        _client = client;
    }
    
    [SlashCommand("ping", "Pings the bot")]
    public async Task PingAsync()
    {
        await RespondAsync($"Pong 🏓 That was {_client.Latency} ms");
    }
}