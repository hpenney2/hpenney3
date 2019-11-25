using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace hpenney2clone
{
    public class Result : RuntimeResult
    {
        public Result(CommandError? error, string reason) : base(error, reason)
        {
        }

        public static Result FromError(string reason) =>
            new Result(CommandError.Unsuccessful, reason);
        public static Result FromStrangeError(string strangeReason) =>
            new Result(CommandError.Unsuccessful, $"A strange error has occured, please send this to the support channel in our Discord server (~support).\n`{strangeReason}`");
        public static Result FromSuccess(string reason = null) =>
            new Result(null, reason);
    }
}
