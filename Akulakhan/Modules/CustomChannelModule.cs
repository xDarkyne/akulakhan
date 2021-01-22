using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Akulakhan.Modules
{
    [Group("custom"), Name("Custom Channels")]
    public class CustomChannelModule : ModuleBase<SocketCommandContext>
    {
        [Command("name"), Ratelimit(2, 10, Measure.Minutes)]
        public async Task SetNameAsync(string name)
        {
            var user = Context.User;

            foreach(var customChannel in Worker._customChannels)
            {
                // check if the user is within a custom channel
                if (customChannel.Channel.GetUserAsync(user.Id) != null)
                {
                    // check if the user is the channel's owner
                    if (customChannel.Owner != user) break;
                    // change the channel's name
                    await customChannel.Channel.ModifyAsync(x => x.Name = name);
                    break;
                }
            }
        }
    }
}
