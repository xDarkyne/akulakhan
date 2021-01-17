using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Akulakhan
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Run(MainAsync, stoppingToken);
            }
        }

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            _client.MessageReceived += OnMessage;
            _client.Ready += OnReady;
            _client.Log += Log;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            await _client.LoginAsync(TokenType.Bot, config.token);
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        private async Task OnReady()
        {
            await _client.SetActivityAsync(new Game("dotNet", ActivityType.Playing));
        }

        private async Task OnMessage(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            int pos = 0;

            if (!message.HasCharPrefix(config.prefix, ref pos) || message.Author.IsBot) return;

            var result = await _commands.ExecuteAsync(context, pos, _services);
            if (!result.IsSuccess) Console.WriteLine(result.Error + "|" + result.ErrorReason);
        }

        public Task Log(LogMessage msg)
        {
            try
            {
                Console.WriteLine(msg.ToString());
                _logger.LogDebug(msg.ToString());
                return Task.CompletedTask;
            }
            catch { return null; }
        }
    }
}
