using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Extensions;
using Lavalink4NET.InactivityTracking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
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

    public class AppSettings
    {
        public string StatusMessage { get; set; }
    }

    public class LavalinkSettings
    {
        public string BaseAddress { get; set; }
        public string Passphrase { get; set; }
    }

    // App login
    public static async Task Main(string[] args)
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables(prefix: "?")
            .AddJsonFile("src/appsettings.json", true, true)
            .Build();

        var lavaLinkSettings = _configuration.GetSection("Lavalink").Get<LavalinkSettings>();

        _services = new ServiceCollection()
            .AddSingleton(_configuration)
            .AddSingleton(_socketConfig)
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<InteractionHandler>()
            .AddSingleton<DiscordSocketClient>()
            .AddLavalink()
            .AddLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Trace))
            .ConfigureLavalink(config =>
            {
                config.BaseAddress = new Uri(lavaLinkSettings?.BaseAddress ?? "http://localhost:2333");
                config.Passphrase = lavaLinkSettings?.Passphrase ?? "notadefaultpass";
                config.ReadyTimeout = TimeSpan.FromSeconds(3);
                config.ResumptionOptions = new LavalinkSessionResumptionOptions(TimeSpan.FromSeconds(60));
            })
            .BuildServiceProvider();

        // Lavalink
        var audioService = _services.GetRequiredService<IAudioService>();
        var inactivityTrackingService = _services.GetService<IInactivityTrackingService>();

        var appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
        var client = _services.GetRequiredService<DiscordSocketClient>();
        var token = Environment.GetEnvironmentVariable("TOKEN");
        client.Log += LogAsync;

        await _services.GetRequiredService<InteractionHandler>()
            .InitializeAsync();
        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();
        await audioService.StartAsync();

        // Custom presence
        if (!string.IsNullOrWhiteSpace(appSettings?.StatusMessage))
        {
            await client.SetCustomStatusAsync(appSettings.StatusMessage);
        }

        await Task.Delay(Timeout.Infinite).ConfigureAwait(false);
    }

    // Logs
    private static Task LogAsync(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}