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
    public class Echo : ModuleBase<ShardedCommandContext>
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

        //[Command("silentecho"), Alias("silentsay"), RequireContext(ContextType.DM), RequireOwner(ErrorMessage = "only hpenney2 can silently echo messages")]
        //public async Task SilentEchoAsync(ulong channelId = 0, [Remainder] string message = null)
        //{

        //    //if (Context.User.Id != 142664159048368128) { await ReplyAsync("only hpenney2 can silently echo messages"); return; }
        //    if (channelId == 0 || message == null) { await ReplyAsync("bruh you literally made me how did you mess that up"); return; };

        //    var channel = Context.Client.GetChannel(channelId) as IMessageChannel;

        //    await channel.SendMessageAsync(message);
        //}
    }
}