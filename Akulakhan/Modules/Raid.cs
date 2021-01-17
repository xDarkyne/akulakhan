using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Akulakhan.Modules
{
    [Group("raid")]
    public class Raid : ModuleBase<SocketCommandContext>
    {
        [Command]
        public async Task DefaultRaidAsync()
        {
            //
        }
    }
}
