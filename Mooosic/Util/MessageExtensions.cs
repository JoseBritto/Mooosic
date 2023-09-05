using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Mooosic.Util;

public static class MessageExtensions
{
    public static async Task DisableAsync(this RestUserMessage message)
    {
        var components = message.Components
            .Select(x => x as IMessageComponent)?.ToList();
        if (components is null || components.Count == 0)
            return;
        var list = new List<ActionRowBuilder>();
        foreach (var component in components)
        {
            // There will be at least one action row (I hope!)
            if (component.Type == ComponentType.ActionRow)
            {
                var row = component as ActionRowComponent;
                var newRow = new ActionRowBuilder();
                foreach (var comp in row!.Components)
                {
                    AddDisabledComponentToList(comp, newRow);
                }
                list.Add(newRow);
            }
        }
        await message.ModifyAsync(x =>
        {
            x.Components = new ComponentBuilder().WithRows(list).Build();
            x.Embeds = message.Embeds.ToArray();
            x.Content = message.Content;
        });
        return;

        
        
        void AddDisabledComponentToList(IMessageComponent component, ActionRowBuilder row)
        {
            switch (component.Type)
            {
                case ComponentType.Button:
                {
                    var button = component as ButtonComponent;
                    row.AddComponent(button!.ToBuilder().WithDisabled(true).Build());
                    break;
                }
                
                case ComponentType.SelectMenu or ComponentType.RoleSelect or ComponentType.ChannelSelect or ComponentType.MentionableSelect:
                {
                    var selectMenu = component as SelectMenuComponent;
                    row.AddComponent(selectMenu!.ToBuilder().WithDisabled(true).Build());
                    break;
                }

                case ComponentType.TextInput:
                {
                    // This is not supported!
                    break;
                }
            }
        }
    }
}