using Discord;

namespace Mooosic.Util;

public class MoosicTrackContext
{
    public string? RealSongName { get; set; }
    public string? RealSongUrl { get; set; }
    public string SearchTerm { init; get; }
    
    //TODO: Implement this
    public IUser? RequestedBy { get; set; }

}