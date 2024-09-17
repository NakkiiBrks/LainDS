using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace LainDS
{
    public class Program
    {
        public static DiscordSocketClient? client;

        public static async Task Main(string[] args)
        {
            client = new DiscordSocketClient();
            client.Log += Log;
            client.Ready += Client_Ready;
            client.SlashCommandExecuted += SlashCommandHandler;


            var token = Environment.GetEnvironmentVariable("TOKEN");
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            await Task.Delay(-1);
        }

        public static async Task Client_Ready()
        {
            var globalCommand = new SlashCommandBuilder();
            globalCommand.WithName("idiot");
            globalCommand.WithDescription("Terry Davis quote");

            try
            {
                await client.CreateGlobalApplicationCommandAsync(globalCommand.Build());
            }
            catch (HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

                Console.WriteLine(json);
            }
        }

        private static async Task SlashCommandHandler(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case "idiot":
                    await IdiotTerryDavisQuote(command);
                    break;
            }
        }

        private static async Task IdiotTerryDavisQuote(SocketSlashCommand command)
        {
            await command.RespondAsync("An idiot admires complexity, a genius admires simplicity, " +
                "a physicist tries to make it simple, for an idiot anything the more complicated it is the more he will admire it, " +
                "if you make something so clusterfucked he can't understand it he's gonna think you're a god cause you made it so complicated nobody can understand it. " +
                "That's how they write journals in Academics, they try to make it so complicated people think you're a genius.");
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public class LoggingService
        {
            LoggingService(DiscordSocketClient client, CommandService command)
            {
                client.Log += LogAsync;
                command.Log += LogAsync;
            }

            private Task LogAsync(LogMessage message)
            {
                if (message.Exception is CommandException cmdException)
                {
                    Console.WriteLine($"[Command/{message.Severity}] {cmdException.Command.Aliases[0]}" +
                        $" failed to execute in {cmdException.Context.Channel}.");
                    Console.WriteLine(cmdException);
                }
                else
                {
                    Console.WriteLine($"[General/{message.Severity}] {message}");
                }

                return Task.CompletedTask;
            }
        }
    }
}