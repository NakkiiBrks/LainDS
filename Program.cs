using Discord;
using Discord.WebSocket;


public class Program
{
#pragma warning disable CS8618 // O campo não anulável precisa conter um valor não nulo ao sair do construtor. Considere adicionar o modificador "obrigatório" ou declarar como anulável.
    private static DiscordSocketClient _client;
#pragma warning restore CS8618 

    public static async Task Main()
    {
        _client = new DiscordSocketClient();

        _client.Log += Log;

        var token = Environment.GetEnvironmentVariable("TOKEN");

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}