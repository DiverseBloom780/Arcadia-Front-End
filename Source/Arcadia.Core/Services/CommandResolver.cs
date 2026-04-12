using System.Collections.Generic;
using Arcadia.Core.Models;

namespace Arcadia.Core.Helpers
{
    public static class CommandResolver
    {
        public static string Resolve(string template, Game game, Emulator emulator)
        {
            var result = template;

            // Standard variable replacement
            result = result.Replace("{RomPath}", $"\"{game.RomPath}\"");
            result = result.Replace("{EmulatorPath}", $"\"{emulator.ExecutablePath}\"");
            result = result.Replace("{Title}", game.Title);

            // Custom emulator variables
            foreach (var variable in emulator.CommandLineVariables)
            {
                result = result.Replace($"{{{variable.Key}}}", variable.Value);
            }

            return result;
        }
    }
}