using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace hpenney2clone
{
    public class CustomResult : RuntimeResult
    {
        public CustomResult(CommandError? error, string reason) : base(error, reason)
        {
        }

        public static CustomResult FromError(string reason) =>
            new CustomResult(CommandError.Unsuccessful, reason);
        public static CustomResult FromSuccess(string reason = null) =>
            new CustomResult(null, reason);
    }
}
