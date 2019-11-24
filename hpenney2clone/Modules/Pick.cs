using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Text.RegularExpressions;

namespace hpenney2clone.Modules
{
    public class Pick : ModuleBase<ShardedCommandContext>
    {
        static Random rnd = new Random();
        [Command("pick", RunMode = RunMode.Async), Alias("idunno", "choose")]
        [Summary("Picks between at least two choices seperated by commas.")]
        public async Task PickAsync([Remainder] string choices)
        {
            char[] seperators = {','};
            string[] choiceTable = choices.Split(separator: seperators, options: StringSplitOptions.RemoveEmptyEntries);

            if (choiceTable.Count() <= 1) { await ReplyAsync("you have to split at least two choices with a comma, like this,or like this"); return; }

            var message = await ReplyAsync("<a:ThinkRoll:636688496689414144>");

            await Task.Delay(2500);

            int rndChoice = rnd.Next(choiceTable.Count());
            string choice = choiceTable[rndChoice].Trim();
            await message.ModifyAsync(prop => prop.Content = choice);
        }
    }
}