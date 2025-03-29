using System;
using Dalamud.Game.Command;

namespace Namingway;

internal class Commands : IDisposable {
    private NamingwayPlugin Plugin { get; }

    internal Commands(NamingwayPlugin plugin) {
            this.Plugin = plugin;

            Service.CommandManager.AddHandler("/namingway", new CommandInfo(this.OnCommand) {
                HelpMessage = "Opens the Namingway interface",
            });
        }

    public void Dispose() {
            Service.CommandManager.RemoveHandler("/namingway");
        }

    private void OnCommand(string command, string arguments) {
            this.Plugin.Ui.DrawSettings ^= true;
        }
}