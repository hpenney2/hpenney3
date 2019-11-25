using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace hpenney2clone.Modules
{
    public class Information : ModuleBase<ShardedCommandContext>
    {
        private readonly CommandService _service;

        public Information(CommandService service)
        {
            _service = service;
        }

        [Command("help"), Alias("commands", "cmds")]
        [Summary("Displays all commands.")]
        public async Task HelpAsync()
        {
            //string CurrentContext;
            string prefix = "~";
            //if (Context.IsPrivate) { CurrentContext = "DM"; } else { CurrentContext = "server/guild"; }
            var builder = new EmbedBuilder()
            {
                Color = new Color(42, 141, 222),
                Title = "Commands",
                Description = "Here's a list of available commands.\n Required arguments: []\nOptional arguments: <>",
            };
            builder.WithFooter($"For more information about a command (such as aliases), do {prefix}help <command>.");

            foreach (var module in _service.Modules)
            {
                //if (module.Name.ToLower() == "help") { continue; }
                string description = null;
                foreach (var cmd in module.Commands)
                {

                    description += $"{prefix}{cmd.Name}";

                    if (cmd.Parameters.Count > 0)
                    {
                        description += " ";

                        foreach (var param in cmd.Parameters)
                        {
                            if (param.IsOptional)
                                description += $"<{param.Name}>";
                            else
                                description += $"[{param.Name}]";
                            description += " ";
                        }
                    }

                    description += "\n";

                    if (cmd.Summary != null)
                        description += cmd.Summary + "\n\n";
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }

            await ReplyAsync("", false, builder.Build());
        }

        [Command("help"), Alias("commands", "cmds")]
        [Summary("Gets information about a command, like aliases and parameters.")]
        public async Task<RuntimeResult> HelpAsync(string command)
        {
            var result = _service.Search(command.ToLower());

            if (!result.IsSuccess)
            {
                return Result.FromError($"Couldn't find a command named **{command}**.");
            }

            string prefix = "~";
            var embed = new EmbedBuilder
            {
                Color = new Color(42, 141, 222),
                Title = "Command Information",
                Description = $"Information about the command \"**{command}**\":"
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;
                var aliasesList = cmd.Aliases.ToList();

                if (aliasesList.Any())
                {
                    aliasesList.RemoveAt(0);
                    List<string> aliasesListPrefixes = aliasesList.Select(alias => $"~{alias}").ToList();
                    aliasesList = aliasesListPrefixes;
                }
                else
                    aliasesList.Add("(None)");

                var aliases = aliasesList.ToArray();

                var paramList = cmd.Parameters.ToList();
                string parameters;

                if (paramList.Any())
                {
                    parameters = string.Join(", ", cmd.Parameters.Select(p => $"{p.Name} ({p.Type.Name})"));
                }
                else
                    parameters = "(None)";

                embed.AddField(x =>
                {
                    x.Name = prefix + cmd.Name;
                    x.Value = $"Aliases: {string.Join(", ", aliases)}\n" +
                              $"Parameters: {parameters}\n" +
                              $"Description: {cmd.Summary}";
                    x.IsInline = false;
                });
            }

            await ReplyAsync("", embed: embed.Build());
            return Result.FromSuccess();
        }
    }
}