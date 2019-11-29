using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace hpenney2clone.Modules
{
    public class Fun : ModuleBase<ShardedCommandContext>
    {
        [Command("echo")]
        [Alias("say")]
        [Summary("Echoes a message.")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task EchoAsync([Remainder] string echoMsg = null)
        {
            if (echoMsg == null) { await ReplyAsync("i need a message to echo dumb dumb"); return; }
            await Context.Message.DeleteAsync();
            await ReplyAsync(echoMsg);
        }
    }
}