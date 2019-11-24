using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace hpenney2clone.Modules
{
    public class Shard : ModuleBase<ShardedCommandContext>
    {
        [Command("whatshard"), Alias("shard"), RequireContext(ContextType.Guild)]
        [Summary("Checks the shard of the server ID given, or the current server if no server ID is specified.")]
        public async Task GetShardAsync(ulong serverId = 0)
        {


            if (serverId == 0)
            {
                var shardNum = (Context.Guild.Id >> 22) % 5;
                await ReplyAsync("this server is running on **shard " + shardNum + "**.\nif you want to get the shard number for a specific server, say the server's ID with the command");
                return;
            }
            if (serverId.ToString().Length < 18 || serverId.ToString().Length > 18)
            {
                await ReplyAsync("that is not a valid server ID as it is more or less than 18 characters, server IDs should be exactly 18 characters");
                return;
            }

            var shard = (serverId >> 22) % 5;
            await ReplyAsync($"server id **{serverId.ToString()}** is running on **shard {shard.ToString()}** if the bot is in that server");

        }
    }
}
