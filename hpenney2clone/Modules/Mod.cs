/*using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Interactive;

namespace hpenney2clone.Modules
{
    public class Mod : ModuleBase<ShardedCommandContext>
    {
        [Group("mod"), RequireContext(ContextType.Guild)]
        public class ModModule : InteractiveBase<ShardedCommandContext>
        {
            [Command("kick", RunMode = RunMode.Async), RequireUserPermission(Discord.GuildPermission.KickMembers), RequireBotPermission(Discord.GuildPermission.KickMembers)]
            public async Task KickAsync(SocketGuildUser user = null, [Remainder] string reason = null)
            {
                if (user == null) { await ReplyAsync("i need a user to kick dumb dumb"); return; }
                
                if (reason == null) { await ReplyAsync($"f for {user.Mention}, they have been kicked from the server. no reason given by {Context.User.Username}."); await user.KickAsync(); return; }
                else { await ReplyAsync($"f for {user.Mention}, they have been kicked from the server. reason given by **{Context.User.Username}**: **{reason}**"); await user.KickAsync(reason); return; }
            }

            [Command("ban", RunMode = RunMode.Async), RequireUserPermission(Discord.GuildPermission.BanMembers), RequireBotPermission(Discord.GuildPermission.BanMembers)]
            public async Task BanAsync(SocketGuildUser user = null, [Remainder] string reason = null)
            {
                if (user == null) { await ReplyAsync("i need a user to ban dumb dumb"); return; }

                await ReplyAsync($"ay chief, you sure you wanna ban {user.Username}? (10 seconds to choose, y/n)");
                var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(10));

                if (response != null && response.Content.ToLower() == "y")
                {
                    if (reason == null)
                    {
                        await ReplyAsync($"<a:snap:635296176022749190> mega f to {user.Mention}, they have been banned from {Context.Guild.Name}. no reason given by **{Context.User.Username}**.");
                        var dmUser = await user.GetOrCreateDMChannelAsync();
                        await dmUser.SendMessageAsync($"uh oh, looks like you just got banned from {Context.Guild.Name} by {Context.User.Username}. no reason given.");
                        await BanAsync(user);
                        return;
                    }
                    else
                    {
                        await ReplyAsync($"<a:snap:635296176022749190> mega f to {user.Mention}, they have been banned from {Context.Guild.Name}. reason given by **{Context.User.Username}**: {reason}");
                        var dmUser = await user.GetOrCreateDMChannelAsync();
                        await dmUser.SendMessageAsync($"uh oh, looks like you just got banned from {Context.Guild.Name} by {Context.User.Username}. reason: {reason}");
                        await BanAsync(user, reason);
                        return;
                    }
                }
                else
                {
                    if (response != null && response.Content.ToLower() == "n")
                    {
                        await ReplyAsync($"{user.Username} has been spared");
                        return;
                    }
                    else
                        await ReplyAsync($"⌚ you took too long so I spared {user.Username}");
                    return;
                }
            }
        }
    }
}
*/