using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Akulakhan.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task Help()
        {
            List<CommandInfo> commands = Worker._commands.Commands.ToList();
            var embedBuilder = new EmbedBuilder();

            foreach (CommandInfo command in commands)
            {
                // get command information
                string embedFieldText = command.Summary ?? "No description available";
                embedBuilder.AddField(command.Name.ToLower(), embedFieldText);
            }

            await ReplyAsync("Here's a list of my commands and their description", false, embedBuilder.Build());
        }
    }
}
