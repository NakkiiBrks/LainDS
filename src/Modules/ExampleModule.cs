using Discord;
using Discord.Interactions;
using InteractionFramework.Attributes;
using System;
using System.Threading.Tasks;

namespace InteractionFramework.src.Modules;

public class Administration : InteractionModuleBase<SocketInteractionContext>
{
    public InteractionService Commands { get; set; }

    private InteractionHandler _handler;

    public Administration(InteractionHandler handler)
    {
        _handler = handler;
    }

    [SlashCommand("terry-quote", "Terry Davis quote")]
    public async Task TerryQuote()
        => await RespondAsync("An idiot admires complexity, a genius admires simplicity, " +
                "a physicist tries to make it simple, for an idiot anything the more complicated it is the more he will admire it, " +
                "if you make something so clusterfucked he can't understand it he's gonna think you're a god cause you made it so complicated nobody can understand it. " +
                "That's how they write journals in Academics, they try to make it so complicated people think you're a genius.");


    [SlashCommand("ping", "Pings the bot and returns its latency.")]
    public async Task GreetUserAsync()
        => await RespondAsync(text: $":ping_pong: It took me {Context.Client.Latency}ms to respond to you!", ephemeral: true);

    [SlashCommand("delete_message", "Deletes a DM message")]
    public async Task DeleteMessageAsync(string messageId)
    {
        var message = await Context.Channel.GetMessageAsync(ulong.Parse(messageId));

        if (message != null)
        {
            await message.DeleteAsync();
            await RespondAsync("Message deleted.", ephemeral: true);
        }
        else
        {
            await RespondAsync("I didnt find the message, or it wasnt sent by me", ephemeral: true);
        }
    }
}