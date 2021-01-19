using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Akulakhan.Modules
{
    [Name("Help")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        public HelpModule(CommandService service)
        {
            _service = service;
        }

        [Command("help")]
        public async Task Help()
        {
            var embedBuilder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218), // maybe store colors in config file
                Title = "Help",
                Description = "Here's a list of all commands"
            };

            foreach (var module in _service.Modules)
            {
                string description = null;
                foreach (var command in module.Commands)
                {
                    var result = await command.CheckPreconditionsAsync(Context);
                    var param = command.Parameters.FirstOrDefault();
                    string paramString = (param != null) ? $"<{param}>" : "";
                    if (result.IsSuccess)
                        description += $"{config.prefix}{command.Aliases.First()} {paramString}\n";
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    embedBuilder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }

            await ReplyAsync("", false, embedBuilder.Build());
        }

        [Command("help")]
        public async Task Help(string command)
        {
            var result = _service.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                return;
            }

            char prefix = config.prefix;
            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
                Title = "Help",
                Description = $"Here are some commands like **{command}**"
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;

                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                              $"Summary: {cmd.Summary}";
                    x.IsInline = false;
                });
            }

            await ReplyAsync("", false, builder.Build());
        }
    }
}
