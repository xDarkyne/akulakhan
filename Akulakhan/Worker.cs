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
using System.Collections.Generic;

namespace Akulakhan
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private DiscordSocketClient _client;
        public CommandService _commands;
        private IServiceProvider _services;
        public static List<CustomChannel> _customChannels;

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
            _commands = new CommandService(new CommandServiceConfig { DefaultRunMode = RunMode.Async });
            _customChannels = new List<CustomChannel>();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            _client.MessageReceived += OnMessageAsync;
            _client.Ready += OnReadyAsync;
            _client.UserJoined += OnUserJoinAsync;
            _client.UserVoiceStateUpdated += OnUserVoiceStateUpdatedAsync;
            _client.Log += Log;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            await _client.LoginAsync(TokenType.Bot, config.token);
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        #region Event: OnReadyAsync | Triggered: When the bot gets ready
        private async Task OnReadyAsync()
        {
            await _client.SetActivityAsync(new Game("dotNet", ActivityType.Playing));
        }
        #endregion

        #region Event: OnMessageAsync | Triggered: When a user or a bot sent a message
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
        #endregion

        #region Event: OnUserVoiceStateUpdatedAsync | Triggered: When a user joins or leaves a voice channel
        private async Task OnUserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            // check if the user joined a channel
            if (newState.VoiceChannel != null)
            {
                var guild = newState.VoiceChannel.Guild;
                var guildUser = guild.GetUser(user.Id);

                // check if the channel is the Join2Create channel
                if (newState.VoiceChannel.Id == config.j2cChannelID)
                {
                    // create a new channel within the voice channels category
                    var channel = await guild.CreateVoiceChannelAsync($"{guildUser.Username}'s channel", x => x.CategoryId = config.voiceCategoryID);
                    // add the channel and the user as the owner to the list of custom channels
                    _customChannels.Add(new CustomChannel(guildUser, channel));
                    // move the user to the newly created channel
                    await guildUser.ModifyAsync(x => x.Channel = channel);
                }
            }

            // check if the user left a channel and if the channel is empty now
            if (oldState.VoiceChannel != null && oldState.VoiceChannel.Users.Count == 0)
            {
                var channel = oldState.VoiceChannel;
                foreach(var customChannel in _customChannels)
                {
                    // check if the channel the user left is a custom channel
                    if (customChannel.Channel.Id == channel.Id)
                    {
                        // remove the custom channel from the list and delete the voice channel
                        _customChannels.Remove(customChannel);
                        await customChannel.Channel.DeleteAsync();
                        break;
                    }
                }
            }
        }
        #endregion

        #region Event: OnUserJoinAsync | Triggered: When a user joins the server
        private async Task OnUserJoinAsync(SocketGuildUser user)
        {
            var guild = user.Guild;
            var role = guild.GetRole(config.defaultRoleID);

            // check if the bot user has sufficient permission and if the role was found
            if (!guild.CurrentUser.GuildPermissions.Has(GuildPermission.ManageRoles) || role == null) return;

            // add the role to the user
            await user.AddRoleAsync(role);
        }
        #endregion

        #region Log Function
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
    #endregion

    #region custom structs
    /// <summary>
    /// Wrapper for custom channels. Holds an Owner (SocketGuildUser) and a Channel (IVoiceChannel)
    /// </summary>
    public struct CustomChannel
    {
        public SocketGuildUser Owner;
        public IVoiceChannel Channel;

        public CustomChannel(SocketGuildUser owner, IVoiceChannel channel)
        {
            Owner = owner;
            Channel = channel;
        }
    }
    #endregion
}
