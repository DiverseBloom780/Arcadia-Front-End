using System;
using System.Diagnostics;
using System.IO;
using Arcadia.Core.Models;

namespace Arcadia.Launchers
{
    public class LayraPS4Integration
    {
        private readonly string _layraPath;

        public LayraPS4Integration(string layraPath)
        {
            _layraPath = layraPath;
        }

        public bool IsInstalled()
        {
            return File.Exists(_layraPath);
        }

        public void LaunchGame(Game game)
        {
            if (!IsInstalled())
            {
                throw new FileNotFoundException("LayraPS4 executable not found.", _layraPath);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = _layraPath,
                Arguments = $"\"{game.ExecutablePath}\"",
                WorkingDirectory = Path.GetDirectoryName(_layraPath),
                UseShellExecute = false
            };

            Process.Start(startInfo);
        }
    }
}
