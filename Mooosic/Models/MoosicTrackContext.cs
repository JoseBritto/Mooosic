using Discord;
using Mooosic.Models;

namespace Mooosic.Util;

public class MoosicTrackContext
{
    public string? RealSongName { get; set; }
    public string? RealSongUrl { get; set; }
    public string SearchTerm { init; get; }
    
    public IGuildUser? RequestedBy { get; set; }
    
    public string? OriginalCoverArtUrl { get; set; }
    
    public MusicSource FrontEndSource { init; get; }

}