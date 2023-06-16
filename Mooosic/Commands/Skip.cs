using Discord;
using Discord.Interactions;

namespace Mooosic.Commands;

public class Skip : InteractionModuleBase
{
    private readonly VoiceControls _controls;

    public Skip(VoiceControls controls)
    {
        _controls = controls;
    }
    
    [RequireContext(ContextType.Guild)]
    [SlashCommand("skip", "Skips the current song")]
    public async Task SkipAsync()
    {
        await DeferAsync();
        var result = await _controls.SkipAsync(Context.User as IGuildUser);

        await FollowupAsync(result.Response ?? result.Exception?.Message ?? "null", ephemeral: result.WasSuccess == false);
    }

    [RequireContext(ContextType.Guild)]
    [SlashCommand("play-pause", "Pauses or plays the current track")]
    public async Task PlayPasueAsync()
    {
        await DeferAsync();
        
        var result = await _controls.PauseOrResumeAsync(Context.User as IGuildUser);

        await FollowupAsync(result.Response ?? result.Exception?.Message ?? "null", ephemeral: result.WasSuccess == false);

    }
}