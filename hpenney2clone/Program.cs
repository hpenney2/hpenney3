using System;
using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Addons.Interactive;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Discord;

namespace hpenney2clone
{
    public class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordShardedClient _client;
        private CommandService _commands;
        //public LiteDatabase _prefixDb;

        public async Task MainAsync()
        {
            _client = new DiscordShardedClient(new DiscordSocketConfig { TotalShards = 5 });
            _services = BuildServiceProvider();
            _commands = new CommandService(new CommandServiceConfig { LogLevel = Discord.LogSeverity.Info, CaseSensitiveCommands = false, DefaultRunMode = RunMode.Async});
            _client.Log += Log;
            _commands.Log += Log;
            var betaMode = false;
            string tokenType;
            if (betaMode) tokenType = "token"; else tokenType = "beta_token";
            //if (betaMode) tokenName = "betatoken.txt"; else tokenName = "token.txt";
            //var runDirectory = Directory.GetCurrentDirectory();
            //var tokenPath = Path.GetFullPath(Path.Combine(runDirectory, @"..\..\"));
            var token = Environment.GetEnvironmentVariable(tokenType);
            await _client.LoginAsync(Discord.TokenType.Bot, token);
            await _client.StartAsync();
            await GetCommandsAsync();
            _client.ShardReady += SetStatus;
            _client.JoinedGuild += SetStatusGuild;
            _client.JoinedGuild += GuildJoinLog;
            _client.LeftGuild += SetStatusGuild;
            _client.LeftGuild += GuildLeaveLog;

            await Task.Delay(-1);
        }

        public IServiceProvider BuildServiceProvider() => new ServiceCollection()
            .AddSingleton(new InteractiveService(_client))
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
            await Log(new LogMessage(LogSeverity.Info, "Command", $"A command executed successfully. Message: [{context.Message.Content}] | Guild: [{context.Guild.Name} ({context.Guild.Id})]"));
        }

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


        private async Task SetStatusGuild(SocketGuild guild)
        {
            var gCount = _client.Guilds.Count;

            foreach (DiscordSocketClient shard in _client.Shards)
            {
                await shard.SetActivityAsync(new Discord.Game($"hpenney2 | ~help for commands | Shard {shard.ShardId} | {gCount} servers |"/*"hpenney2 in " + _client.Guilds.Count + " servers"*/, Discord.ActivityType.Watching));
            }
        }

        private async Task SetStatus(DiscordSocketClient shard)
        {
            var gCount = _client.Guilds.Count;
            await shard.SetActivityAsync(new Discord.Game($"hpenney2 | ~help for commands | Shard {shard.ShardId} | {gCount} servers |", Discord.ActivityType.Watching));
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
