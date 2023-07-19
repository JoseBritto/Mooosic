using Lavalink4NET.Player;

namespace Mooosic.Util;

public readonly struct MoosicPlayerInfo
{
    public ulong? VcId
    {
        init;
        get;
    }
    public ulong GuildId { init; get; }
    
    public LavalinkTrack? CurrentTrack { init; get;}

    public PlayerState State { init; get; }

    public TrackPosition Position { init; get; }

}