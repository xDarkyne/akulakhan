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
        public CommandService _commands;
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

        // setup and start the bot
        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            _client.MessageReceived += OnMessageAsync;
            _client.Ready += OnReadyAsync;
            _client.UserJoined += OnUserJoinAsync;
            _client.Log += Log;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            await _client.LoginAsync(TokenType.Bot, config.token);
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        // handling what happens when the bot gets ready
        private async Task OnReadyAsync()
        {
            await _client.SetActivityAsync(new Game("dotNet", ActivityType.Playing));
        }

        // handling for incoming messages
        private async Task OnMessageAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            int pos = 0;

            // ignores messages that don't start with the command prefix or were sent by a bot
            if (!message.HasCharPrefix(config.prefix, ref pos) || message.Author.IsBot) return;

            var result = await _commands.ExecuteAsync(context, pos, _services);
            // handling the error if command execution fails
            if (!result.IsSuccess)
            {
                Console.WriteLine(result.Error + "|" + result.ErrorReason);
                await message.Channel.SendMessageAsync($"Ups, something went wrong. Use `{config.prefix}help` for a list of commands");
            }
        }

        // handling what happens when a new user joins
        private async Task OnUserJoinAsync(SocketGuildUser user)
        {
            var guild = user.Guild;
            var role = guild.GetRole(config.defaultRoleID);

            // check if the bot user has sufficient permission and if the role was found
            if (!guild.CurrentUser.GuildPermissions.Has(GuildPermission.ManageRoles) || role == null) return;

            // add the role to the user
            await user.AddRoleAsync(role);
        }

        // handling logs
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
