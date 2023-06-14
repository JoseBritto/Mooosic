using Config.Net;

namespace Mooosic;

public interface ISettings
{
    [Option(Alias = "discord_token")]
    public string DiscordToken { get; }

    
    [Option(Alias = "admin_ids", DefaultValue = new string[]{})]
    public string[] AdminIds { get; }

    [Option(Alias = "log_file", DefaultValue = "./Logs/mooosic_.log")]
    public string LogFile { get; }

    [Option(Alias = "lavalink")] 
    public ILavalinkSetting Lavalink { get; }


}

public interface ILavalinkSetting
{
    private const string RESTURI_DEFAULT = "http://localhost:2333/";
    private const string WSURI_DEFAULT = "ws://localhost:2333/";
    private const string PASSWD_DEFAULT = "youshallnotpass";
    [Option(Alias = "rest_uri", DefaultValue = RESTURI_DEFAULT)]
    public string RestUri { get; }

    [Option(Alias = "websocket_uri", DefaultValue = WSURI_DEFAULT)]
    public string WebsocketUri { get; }

    [Option(Alias = "password", DefaultValue = PASSWD_DEFAULT)]
    public string Password { get; }
    

}