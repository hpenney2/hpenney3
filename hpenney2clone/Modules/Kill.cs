using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Addons.Interactive;

namespace hpenney2clone.Modules
{
    public class Kill : InteractiveBase<ShardedCommandContext>
    {
        public InteractiveService IntServ { get; set; }
        [Command("kill", RunMode = RunMode.Async),Alias("exit"),RequireOwner(ErrorMessage = "only hpenney2 has a high enough power level to kill me")]
        [Summary("Kills the bot. Can only be ran by hpenney2.")]
        public async Task KillAsync()
        {
            //if (Context.User.Id != 142664159048368128) { await ReplyAsync("only hpenney2 has a high enough power level to kill me"); return; }
            await ReplyAsync("uh, you sure you want to kill me? you have 5 seconds to choose (y/n)");
            var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(5));
            if (response != null && response.Content.ToLower() == "y")
            {
                await ReplyAsync("<a:snap:636688480352600074> mr. hpenney2, i don't feel so good. i might show online for up to about 5 minutes after exiting.");
                Environment.Exit(0);
                return;
            }
            else
            {
                if (response != null && response.Content.ToLower() == "n")
                {
                    await ReplyAsync("thank you for sparing me uwu");
                }
                else
                    await ReplyAsync("you took too long, i get to live");
                return;
            }
        }
    }
}
