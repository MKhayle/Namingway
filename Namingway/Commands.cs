using System;
using Dalamud.Game.Command;

namespace Namingway {
    internal class Commands : IDisposable {
        private Plugin Plugin { get; }

        internal Commands(Plugin plugin) {
            this.Plugin = plugin;

            this.Plugin.Interface.CommandManager.AddHandler("/namingway", new CommandInfo(this.OnCommand) {
                HelpMessage = "Opens the Namingway interface",
            });
        }

        public void Dispose() {
            this.Plugin.Interface.CommandManager.RemoveHandler("/namingway");
        }

        private void OnCommand(string command, string arguments) {
            this.Plugin.Ui.DrawSettings ^= true;
        }
    }
}
