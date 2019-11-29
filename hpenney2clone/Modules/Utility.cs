using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace hpenney2clone.Modules
{
    public class Utility : ModuleBase<ShardedCommandContext>
    {
        static Random rnd = new Random();

        [Command("userinfo"), Alias("uinfo", "info")]
        [Summary("Gets information about a user in a nice embed.")]
        public async Task GetUInfoAsync(SocketGuildUser user = null)
        {
            // Makes a default value for the user the bot gets info for if the command was given no user.
            var userGet = user ?? Context.User as SocketGuildUser;

            // Creates new EmbedBuilder.
            var userInfo = new Discord.EmbedBuilder()
            {
                Title = $"Info about **{userGet.Username}**",
                ThumbnailUrl = userGet.GetAvatarUrl() ?? userGet.GetDefaultAvatarUrl(),
                Color = Discord.Color.Green
            };
            
            // Adds fields containing various types of information about the user.
            userInfo.AddField("Username & Discriminator", userGet.Username + "#" + userGet.Discriminator, true);
            userInfo.AddField("Nickname", userGet.Nickname ?? "None", true);
            userInfo.AddField("Account Created At", userGet.CreatedAt.DateTime.ToString(), true);
            userInfo.AddField("Joined Server At", userGet.JoinedAt.Value.DateTime.ToString(), true);
            if (userGet.Status != Discord.UserStatus.DoNotDisturb)
                userInfo.AddField("Status", userGet.Status.ToString(), true);
            else
                userInfo.AddField("Status", "Do Not Disturb", true);
            userInfo.AddField("Bot", userGet.IsBot ? "Yes" : "No", true);

            // Posts the embed to the channel.
            await ReplyAsync("", false, userInfo.Build());
        }

        [Command("pick", RunMode = RunMode.Async), Alias("idunno", "choose")]
        [Summary("Picks between at least two choices seperated by commas.")]
        public async Task PickAsync([Remainder] string choices)
        {
            char[] seperators = { ',' };
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
