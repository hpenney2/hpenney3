using System;
using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Addons.Interactive;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using System.Collections.Generic;
using System.Linq;
using Discord.Audio;
using System.Threading;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace hpenney2clone
{
    public class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordShardedClient _client;
        private CommandService _commands;
        private bool betaMode;
        //public LiteDatabase _prefixDb;

        public async Task MainAsync()
        {
            _client = new DiscordShardedClient(new DiscordSocketConfig { TotalShards = 5 });
            _services = BuildServiceProvider();
            _commands = new CommandService(new CommandServiceConfig { LogLevel = Discord.LogSeverity.Info, CaseSensitiveCommands = false, DefaultRunMode = RunMode.Async});
            _client.Log += Log;
            _commands.Log += Log;
            betaMode = false;
            string tokenType;
            if (!betaMode) tokenType = "token"; else tokenType = "beta_token";
            //if (betaMode) tokenName = "betatoken.txt"; else tokenName = "token.txt";
            //var runDirectory = Directory.GetCurrentDirectory();
            //var tokenPath = Path.GetFullPath(Path.Combine(runDirectory, @"..\..\"));
            var token = Environment.GetEnvironmentVariable(tokenType);
            await _client.LoginAsync(Discord.TokenType.Bot, token);
            await _client.StartAsync();
            await GetCommandsAsync();
            _client.ShardReady += SetStatus;
            //_client.JoinedGuild += SetStatusGuild;
            _client.JoinedGuild += GuildJoinLog;
            //_client.LeftGuild += SetStatusGuild;
            _client.LeftGuild += GuildLeaveLog;

            await Task.Delay(-1);
        }

        public IServiceProvider BuildServiceProvider() => new ServiceCollection()
            .AddSingleton(new InteractiveService(_client))
            //.AddSingleton(new AudioService())
            .BuildServiceProvider();

        private IServiceProvider _services;

        private async Task GetCommandsAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _commands.CommandExecuted += OnCommandExecutedAsync;
            _client.MessageReceived += HandleCommandAsync;
        }

        public async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!string.IsNullOrWhiteSpace(result?.ErrorReason) && result.Error != CommandError.UnknownCommand /*&& result.Error != CommandError.UnmetPrecondition && result.Error != CommandError.ParseFailed && result.Error != CommandError.BadArgCount*/)
            {
                var errorEmbed = new Discord.EmbedBuilder()
                {
                    Title = "Error",
                    Description = result.ErrorReason,
                    Color = Discord.Color.Red
                };
                errorEmbed.WithCurrentTimestamp();
                errorEmbed.WithFooter("If you think this is a bug, please report it in the #support channel in our server (~support).");
                await context.Channel.SendMessageAsync("", embed: errorEmbed.Build());
                await Log(new LogMessage(LogSeverity.Error, "Command", $"A command errored. Message: [{context.Message.Content}] | Error reason: [{result.ErrorReason}] | Guild: [{context.Guild.Name} ({context.Guild.Id})]"));
                return;
            }

            var commandName = command.IsSpecified ? command.Value.Name : "Unknown Command";
            await Log(new LogMessage(LogSeverity.Info, "Command", $"A command executed successfully. Message: [{context.Message.Content}] | Command: [{commandName}] | Guild: [{context.Guild.Name} ({context.Guild.Id})]"));
        }

        // Anyone who makes contributions can be added to the tester list here.
        ulong[] betaAllowedIds = { 142664159048368128, 336980669471391754 };
        private async Task HandleCommandAsync(SocketMessage arg)
        {
            // Don't process the command if it was a system message
            var message = arg as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasStringPrefix("~", ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            // Create a WebSocket-based command context based on the message
            var context = new ShardedCommandContext(_client, message);
            if (betaMode && !betaAllowedIds.Contains(context.User.Id))
            {
                await context.Channel.SendMessageAsync("Only a few people can use the test/beta version of hpenney3 right now, sorry.");
                return;
            }

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            var result = await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _services);

            /*if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
            {
                var errorEmbed = new Discord.EmbedBuilder()
                {
                    Title = "Error",
                    Description = result.ErrorReason,
                    Color = Discord.Color.Red
                };
                errorEmbed.WithCurrentTimestamp();
                errorEmbed.WithFooter("If you think this is a bug, please report it in the #support channel in our server (~support).");
                await context.Channel.SendMessageAsync("", embed: errorEmbed.Build());
                await Log(new LogMessage(LogSeverity.Error, "Command", $"A command errored. Message: [{message.Content}] | Error reason: [{result.ErrorReason}]"));
            }*/
        }


        /*private async Task SetStatusGuild(SocketGuild guild)
        {
            /*var gCount = _client.Guilds.Count;

            foreach (DiscordSocketClient shard in _client.Shards)
            {
                await shard.SetActivityAsync(new Game($"hpenney2 | ~help for commands | Shard {shard.ShardId} | {gCount} servers |"/*"hpenney2 in " + _client.Guilds.Count + " servers"*//*, Discord.ActivityType.Watching));
            }
        }*/

        private async Task SetStatus(DiscordSocketClient shard)
        {
            Task.Run(() => SetStatusATree(shard));
        }

        private async Task SetStatusATree(DiscordSocketClient shard)
        {
            var gCount = _client.Guilds.Count;
            var client = new RestClient("https://teamtrees.p.rapidapi.com/status");
            var request = new RestRequest(Method.GET);
            request.AddHeader("x-rapidapi-host", "teamtrees.p.rapidapi.com");
            request.AddHeader("x-rapidapi-key", "55e790cf59msh2137850e17331e7p1fd990jsn63303f8604dc");
            SocketGuildChannel channel;
            if (shard.ShardId == 2)
                channel = shard.GetGuild(636687065429442579).GetChannel(657667707134804008);
            else
                channel = null;
            while (true)
            {
                await shard.SetActivityAsync(new Game($"hpenney2 | ~help for commands | Shard {shard.ShardId} | {gCount} servers |", Discord.ActivityType.Watching));
                if (shard.ShardId == 2)
                {
                    string ftreeCount = null;
                    IRestResponse response = client.Execute(request);
                    Dictionary<string, string> trees;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var jsonObj = JObject.Parse(response.Content);
                        trees = jsonObj.ToObject<Dictionary<string, string>>();
                        trees.TryGetValue("trees", out ftreeCount);
                        ftreeCount = $"{Convert.ToUInt64(ftreeCount):n0}";
                    }
                    else
                        ftreeCount = "(error getting tree count)";
                    await channel.ModifyAsync(c => c.Name = $"🌲 {ftreeCount}");
                }
                await Task.Delay(TimeSpan.FromSeconds(15));
            }
        }

        private async Task GuildJoinLog(SocketGuild guild)
        {
            var logChannel = _client.GetChannel(647167484616769566) as SocketTextChannel;

            var embed = new Discord.EmbedBuilder()
            {
                Title = "📥 Joined guild 📥",
                Color = Discord.Color.Green
            };
            embed.AddField("Name", guild.Name, true);
            embed.AddField("ID", guild.Id, true);
            embed.WithFooter($"Now in {_client.Guilds.Count.ToString()} guilds.");
            embed.WithCurrentTimestamp();

            await logChannel.SendMessageAsync("", embed: embed.Build());
            //await logChannel.SendMessageAsync($"📥 Joined guild {guild.Name} ({guild.Id}). Now in {_client.Guilds.Count.ToString()} guilds.");
        }

        private async Task GuildLeaveLog(SocketGuild guild)
        {
            var logChannel = _client.GetChannel(647167484616769566) as SocketTextChannel;

            var embed = new Discord.EmbedBuilder()
            {
                Title = "📤 Left guild 📤",
                Color = Discord.Color.Red
            };
            embed.AddField("Name", guild.Name, true);
            embed.AddField("ID", guild.Id, true);
            embed.WithFooter($"Now in {_client.Guilds.Count.ToString()} guilds.");
            embed.WithCurrentTimestamp();

            //await logChannel.SendMessageAsync($"📤 Left guild \"{guild.Name}\" (`{guild.Id}`). Now in {_client.Guilds.Count.ToString()} guilds.");
            await logChannel.SendMessageAsync("", embed: embed.Build());
        }

        private Task Log(Discord.LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
