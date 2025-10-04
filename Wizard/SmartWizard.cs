using System;
using System.Collections.Generic;

namespace Arcadia.Wizard {
    public class SmartWizard {
        private readonly Dictionary<string, Action<string[]>> commands;

        public SmartWizard() {
            commands = new() {
                { "setup", Setup },
                { "repair", Repair },
                { "config", Configure },
                { "fetch", FetchMedia }
            };
        }

        public void Run(string input) {
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

            var cmd = parts[0].ToLower();
            if (commands.TryGetValue(cmd, out var action)) {
                action(parts[1..]);
            } else {
                Console.WriteLine($"Unknown command: {cmd}");
            }
        }

        private void Setup(string[] args) => Console.WriteLine("Running setup wizard...");
        private void Repair(string[] args) => Console.WriteLine("Repairing configs...");
        private void Configure(string[] args) => Console.WriteLine("Configuring emulator...");
        private void FetchMedia(string[] args) => Console.WriteLine("Fetching missing artwork...");
    }
}
