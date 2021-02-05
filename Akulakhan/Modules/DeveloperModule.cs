using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Akulakhan.Modules
{
    [Name("Developer commands")]
    public class DeveloperModule : ModuleBase<SocketCommandContext>
    {
        [Command("dev")]
        public async Task DevAsync()
        {
            SocketGuild guild = Context.Guild;
            SocketGuildUser user = guild.GetUser(Context.Message.Author.Id);

            var role = guild.Roles.First(x => x.Id == config.devRoleID);
            if (user.Roles.Contains(role) == false)
            {
                await user.AddRoleAsync(role);
                await ReplyAsync($"Gave {user.Mention} the {role.Name} role. Enjoy your new abilities!");
            }
            else
            {
                await user.RemoveRoleAsync(role);
                await ReplyAsync($"Took {role.Name} role from {user.Mention}.");
            }
        }
    }
}
