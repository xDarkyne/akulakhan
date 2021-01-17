using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

/* File for testing, delete later */

namespace Akulakhan.Modules
{
    [Group("basic")] // <-- quasi top level command
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("ping")] // <-- subcommand
        public async Task Ping()
        {
            await Context.Channel.SendMessageAsync("Pong!");
        }

        [Command("pong")]
        public async Task Pong()
        {
            await Context.Channel.SendMessageAsync("Ping!");
        }

        [Group("very")] // <-- untergruppe als subcommand
        public class Very : ModuleBase<SocketCommandContext>
        {
            [Command("super")] // <-- subcommand des subcommands
            public async Task Super()
            {
                await Context.Channel.SendMessageAsync("Super!");
            }
        }
    }
}
