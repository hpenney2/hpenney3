using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Audio;
using YoutubeExplode;
using YoutubeExplode.Converter;
using System.Diagnostics;
using System.Collections.Generic;
using YoutubeExplode.Models.MediaStreams;

namespace hpenney2clone.Modules
{
    public class Voice : ModuleBase<ShardedCommandContext>
    {
        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }

        /*
                [Command("summon", RunMode = RunMode.Async), Alias("join", "getinhere"), RequireContext(ContextType.Guild)]
                [Summary("Joins your current voice channel.")]
                public async Task JoinAsync(uint time = 5)
                {
                    var channel = (Context.User as IGuildUser)?.VoiceChannel;
                    if (Context.Guild.CurrentUser.VoiceChannel != null) { await ReplyAsync("❌ i'm already in a voice channel"); return; }
                    if (channel == null) { await ReplyAsync("❌ you need to be in a voice channel to summon me"); return; }

                    await ReplyAsync($"✅ joining voice channel **{channel.Name}**");
                    var aClient = await channel.ConnectAsync();
                }

                [Command("disconnect"), Alias("dc", "leave"), RequireContext(ContextType.Guild)]
                [Summary("Makes the bot leave its current voice channel (only if you are also in it).")]
                public async Task LeaveAsync()
                {
                    var userChannel = (Context.User as IGuildUser)?.VoiceChannel;
                    var botChannel = Context.Guild.CurrentUser.VoiceChannel;
                    if (botChannel == null) { await ReplyAsync("❌ i'm not in a voice channel")}
                    if (userChannel != botChannel) { await ReplyAsync($"❌ you aren't in my voice channel (**{botChannel.Name}**)"); return; }
                }
        */

        //[Command("bruh", RunMode = RunMode.Async), Alias("soundeffect2")]
        //[Summary("Plays Bruh Sound Effect #2 in your current voice channel.")]


        public Dictionary<string, IAudioClient> aClients = new Dictionary<string, IAudioClient>();
        [Command("play", RunMode = RunMode.Async), Alias("music")]
        public async Task<RuntimeResult> MusicAsync([Remainder] string search)
        {
            var channel = (Context.User as IGuildUser)?.VoiceChannel;
            if (Context.Guild.CurrentUser.VoiceChannel != null) { return CustomResult.FromError("I'm already in a voice channel!"); }
            if (channel == null) { return CustomResult.FromError("You need to be in a voice channel to do that."); }

            var message = await ReplyAsync($"searching for `{search}` on YouTube...");

            var client = new YoutubeClient();
            var vidSearch = await client.SearchVideosAsync(search, 1) as List<YoutubeExplode.Models.Video>;
            var searchArray = vidSearch.ToArray();
            if (searchArray.Length <= 0)
                return CustomResult.FromError($"No videos could be found for the query `{search}`");
            if (searchArray[0].Duration > TimeSpan.FromHours(1))
                return CustomResult.FromError("Videos must be no longer than one hour be played.");

            await message.ModifyAsync(msg => msg.Content = $"Found video! (`{searchArray[0].Title}` uploaded by `{searchArray[0].Author}`)\nPreparing audio...");
            var info = await client.GetVideoMediaStreamInfosAsync(searchArray[0].Id);
            var converter = new YoutubeConverter(client);
            await converter.DownloadVideoAsync(info, $"song_{Context.Guild.Id}.opus", "opus");

            await message.ModifyAsync(msg => msg.Content = $"Joining channel and playing `{searchArray[0].Title}`");
            var aClient = await channel.ConnectAsync();
            aClients.Add(Context.Guild.Id.ToString(), aClient);

            using (var ffmpeg = CreateStream($"song_{Context.Guild.Id}.opus"))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = aClient.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await output.CopyToAsync(discord); }
                finally { await discord.FlushAsync(); }
            }

            await Context.Guild.CurrentUser.VoiceChannel.DisconnectAsync();
            return CustomResult.FromSuccess();
        }

        [Command("disconnect", RunMode = RunMode.Async), Alias("leave", "dc")]
        public async Task<RuntimeResult> DiscAsync()
        {
            var channel = (Context.User as IGuildUser)?.VoiceChannel;
            if (Context.Guild.CurrentUser.VoiceChannel == null) { return CustomResult.FromError("I'm not in a voice channel!"); }
            if (channel != Context.Guild.CurrentUser.VoiceChannel) { return CustomResult.FromError($"You need to be in my voice channel (`{Context.Guild.CurrentUser.VoiceChannel.Name}`) to do that."); }
        }
        
    }
}
