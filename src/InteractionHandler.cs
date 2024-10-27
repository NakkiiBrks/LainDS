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

    string banner = @"
██╗      █████╗ ██╗███╗   ██╗    ██████╗ ███████╗
██║     ██╔══██╗██║████╗  ██║    ██╔══██╗██╔════╝
██║     ███████║██║██╔██╗ ██║    ██║  ██║███████╗
██║     ██╔══██║██║██║╚██╗██║    ██║  ██║╚════██║
███████╗██║  ██║██║██║ ╚████║    ██████╔╝███████║
╚══════╝╚═╝  ╚═╝╚═╝╚═╝  ╚═══╝    ╚═════╝ ╚══════╝                                               
";

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
        Console.Write(banner);
        Console.WriteLine("╔═════════════════════╗");
        Console.WriteLine("║ Server started!");
        Console.WriteLine($"║ Logged as: {_client.CurrentUser.Username}");
        Console.WriteLine("╚═════════════════════╝");
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
                    default:
                        await context.Interaction.RespondAsync(text: "An error has ocurred when processing this command.", ephemeral: true);
                        break;

                    case InteractionCommandError.UnmetPrecondition:
                        await context.Interaction.RespondAsync(text: "You dont have enough permissions to execute this command.", ephemeral: true);
                        break;
                }
        }
        catch
        {
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }

    private async Task HandleInteractionExecute(ICommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        if (!result.IsSuccess)
            switch (result.Error)
            {
                default:
                    Console.WriteLine($"An error has ocurred when processing: {commandInfo.Name}. Reason: {result.ErrorReason}");
                    await context.Interaction.RespondAsync(text: $"An error has ocurred when processing: {commandInfo.Name}. Reason: {result.ErrorReason}", ephemeral: true);
                    break;

                case InteractionCommandError.UnmetPrecondition:
                    Console.WriteLine("You dont have enough permissions to execute this command.");
                    await context.Interaction.RespondAsync(text: "You dont have enough permissions to execute this command.", ephemeral: true);
                    break;

                case InteractionCommandError.UnknownCommand:
                    Console.WriteLine($"Unknow command: {commandInfo.Name}");
                    await context.Interaction.RespondAsync(text: $"Unknow command: {commandInfo.Name}", ephemeral: true);
                    break;

                case InteractionCommandError.Exception:
                    Console.WriteLine($"Exception while trying to run command: {commandInfo.Name}");
                    await context.Interaction.RespondAsync(text: $"Exception while trying to run command: {commandInfo.Name}", ephemeral: true);
                    break;
            }
    }
}