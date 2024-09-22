using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace InteractionFramework;

public class Program
{
    private static IConfiguration _configuration;
    private static IServiceProvider _services;

    // Intents config
    private static readonly DiscordSocketConfig _socketConfig = new()
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
        AlwaysDownloadUsers = true,
    };

    private static readonly InteractionServiceConfig _interactionServiceConfig = new()
    {
        LocalizationManager = new ResxLocalizationManager("InteractionFramework.Resources.CommandLocales", Assembly.GetEntryAssembly(),
        new CultureInfo("en-US"), new CultureInfo("ru"))
    };

    // App login
    public static async Task Main(string[] args)
    {
        _configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables(prefix: "?")
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        _services = new ServiceCollection()
            .AddSingleton(_configuration)
            .AddSingleton(_socketConfig)
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(), _interactionServiceConfig))
            .AddSingleton<InteractionHandler>()
            .AddSingleton<DiscordSocketClient>()
            .BuildServiceProvider();

        var client = _services.GetRequiredService<DiscordSocketClient>();

        client.Log += LogAsync;

        // Here we can initialize the service that will register and execute our commands
        await _services.GetRequiredService<InteractionHandler>()
            .InitializeAsync();

        var token = Environment.GetEnvironmentVariable("TOKEN");
        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        await client.SetCustomStatusAsync("Always connected.");

        await Task.Delay(Timeout.Infinite);
    }

    // Logs
    private static Task LogAsync(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}