using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using static Mooosic.Constants.CustomIds;

namespace Mooosic.Buttons;

public class PlaybackControl : InteractionModuleBase
{
    private readonly VoiceControls _controls;

    public PlaybackControl(VoiceControls controls)
    {
        _controls = controls;
    }

    [ComponentInteraction($"{PAUSE_RESUME}")]
    [RequireContext(ContextType.Guild)]
    public async Task PlayPause()
    {
        await DeferAsync();
        
        var result = await _controls.PauseOrResumeAsync(Context.User as IGuildUser);

        if(result.WasSuccess == false)
            await FollowupAsync(result.Response ?? result.Exception?.Message ?? "null", ephemeral: true);

        var interaction = Context.Interaction as SocketMessageComponent;
        
        var message = interaction?.Message;
        var originalComponents = message?.Components?.ToList();

        if (interaction is null || message is null || originalComponents is null)
        {
            await FollowupAsync("Something went wrong!", ephemeral: true);
            return;
        }
        
    
        var newComponents = new List<ActionRowBuilder>();

        foreach (var row in originalComponents)
        {
            var newRow = new ActionRowBuilder();
            newComponents.Add(newRow);
            foreach (var component in row.Components)
            {
                if(component.CustomId == PAUSE_RESUME)
                {
                    var button = component as ButtonComponent;
                    var emoji = result.Paused ? new Emoji("▶️") : new Emoji("⏸️");
                    
                    newRow.AddComponent(button!.ToBuilder()
                        .WithEmote(emoji)
                        .Build());
                }
                else
                {
                    newRow.AddComponent(component);
                }
            }
        }


        await interaction.Message.ModifyAsync(x =>
        {
            x.Content = message.Content;
            x.Components = new ComponentBuilder().WithRows(newComponents).Build();
        });
    }
}