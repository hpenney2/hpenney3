﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MongoDB.Bson;
using MongoDB.Driver;

namespace hpenney2clone.Modules
{
    public class Jackbox : ModuleBase<ShardedCommandContext>
    {
        MongoClient mDBClient = new MongoClient("mongodb+srv://quipcord-user:quiphpenneyquipcord3@quipcord-cahg5.mongodb.net/test?retryWrites=true&w=majority");
        [Command("quipcord", RunMode = RunMode.Async), Alias("quiplash", "quip", "qc", "ql"), RequireOwner(ErrorMessage = "Quipcord isn't available at this time and can only be accessed by hpenney2.")]
        [Summary("Quiplash recreated in Discord.")]
        public async Task Quiplash(string action = null, string code = null)
        {

            string[] actions = { "create", "start", "join" };
            //string[] allowedChars = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            Regex alphaNum = new Regex("^[a-zA-Z0-9]*$");

            if (action == null) { await ReplyAsync("you need to use this command with an action, available actions are **create**, **join**, and **start**."); return; }
            foreach (string validAction in actions)
            {
                if (action != validAction) continue;

                //var quipDB = mgDBClient.GetDatabase("quip");
                //var games = quipDB.GetCollection<BsonDocument>("games");

                if (action.ToLower() == "create")
                {
                    if (code == null) { await ReplyAsync("you need to give me a code so other people can join the game!"); return; }
                    if (!alphaNum.IsMatch(code))
                    {
                        await ReplyAsync("your game code must be alphanumeric. (only letter and numbers)");
                        return;
                    }

                    var usersDB = mDBClient.GetDatabase("users").GetCollection<BsonDocument>(Context.User.Id.ToString());

                    //if (System.IO.File.Exists($@"C:\Users\hunte\Desktop\PennBot\hpenney2's clone\hpenney2clone\hpenney2clone\quipcord\{code}.txt")) { await ReplyAsync("a game with that code already exists!"); return; }
                    var gamesForGuild = mDBClient.GetDatabase("games").GetCollection<BsonDocument>(Context.Guild.Id.ToString());
                    var filter = new BsonDocument()
                    {
                        {"gameid", code}
                    };
                    var findGame = gamesForGuild.FindSync(filter);
                    //adding this here so I don't forget, MAKE SURE TO ADD CODE TO ALLOW ONE GAME A PLAYER! this includes for creating games and joining. (maybe not joining?)
                    //also make one game per channel but that might be a little bit overboard
                    if (findGame.SingleOrDefault() != null)
                    {
                        await ReplyAsync("a game with that code already exists for this server! please contact hpenney2 if you think this is in error.");
                        return;
                    }
                    else
                    {
                        System.IO.File.AppendAllText($@"C:\Users\hunte\Desktop\PennBot\hpenney2's clone\hpenney2clone\hpenney2clone\quipcord\{code}.txt", "0");
                        var preGameEmbed = new EmbedBuilder()
                        {
                            Title = "Quipcord Pre-Game",
                            Description = $"Join with **~quipcord join {code}**"
                        };
                        preGameEmbed
                            .WithFooter("Players: 1");


                    }
                }

                if (action.ToLower() == "join")
                {

                }

                if (action.ToLower() == "start")
                {

                }

                else
                {
                    await ReplyAsync($"{action} is not a valid action, available actions are **create**, **join**, and **start**.");
                    return;
                }
            }
        }
    }
}
