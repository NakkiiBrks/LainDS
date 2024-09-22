using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace InteractionFramework;

public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _handler;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;

    public InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services, IConfiguration configuration)
    {
        _client = client;
        _handler = handler;
        _services = services;
        _configuration = configuration;
    }

    public async Task InitializeAsync()
    {
        _client.Ready += ReadyAsync;
        _handler.Log += LogAsync;

        await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        _client.InteractionCreated += HandleInteraction;
        _handler.InteractionExecuted += HandleInteractionExecute;
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }

    private async Task ReadyAsync()
    {
        await _handler.RegisterCommandsGloballyAsync();
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(_client, interaction);

            var result = await _handler.ExecuteCommandAsync(context, _services);

            if (!result.IsSuccess)
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        await context.Interaction.RespondAsync(text: "You dont have enough permissions to execute this command.");
                        break;
                    default:
                        await context.Interaction.RespondAsync(text: "An error has ocurred when processing this command.");
                        break;
                }
        }
        catch
        {
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }

    private Task HandleInteractionExecute(ICommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        if (!result.IsSuccess)
            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    Console.WriteLine($"Not enough permissions to execute: {commandInfo.Name}");
                    break;
                case InteractionCommandError.UnknownCommand:
                    Console.WriteLine($"Unknow command: {commandInfo.Name}.");
                    break;
                case InteractionCommandError.Exception:
                    Console.WriteLine($"Exception while trying to run command: {commandInfo.Name}.");
                    break;
                default:
                    Console.WriteLine($"An error has ocurred when processing: {commandInfo.Name}. Reason: {result.ErrorReason}");
                    break;
            }

        return Task.CompletedTask;
    }
}