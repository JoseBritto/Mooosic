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
    
    
}