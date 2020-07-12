using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Fortnite_API;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FortniteAPI_Bot
{
	class Program
    {
        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                DefaultRetryMode = RetryMode.AlwaysRetry,
                AlwaysDownloadUsers = true,
                RateLimitPrecision = RateLimitPrecision.Millisecond,
                ExclusiveBulkDelete = true
            });

            _commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async
            });

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(new FortniteApi())
                .BuildServiceProvider();

            _client.Log += _client_Log;
            _client.Ready += _client_Ready;

            await RegisterCommandsAsync();

            dynamic data = JObject.Parse(File.ReadAllText("../../../../tokens.json"));

            await _client.LoginAsync(TokenType.Bot, data.discord_token);

            await _client.StartAsync();

            await Task.Delay(-1);

        }

        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        private Task _client_Ready()
        {
            Console.WriteLine($"FortniteAPI-Bot ready as {_client.CurrentUser.Id}.");
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage; if (message == null) return;
            var context = new SocketCommandContext(_client, message);
            if (message.Author.IsBot) return;
            if (context.IsPrivate) return;

            int argPos = 0;
            if (message.HasStringPrefix(".", ref argPos))
            {
                if (!context.Guild.CurrentUser.GetPermissions((IGuildChannel)context.Channel).EmbedLinks)
                {
                    await context.Channel.SendMessageAsync("Failed to execute command as: `Embed Links` permission is missing.");
                    return;
                }

                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            }
        }
    }
}