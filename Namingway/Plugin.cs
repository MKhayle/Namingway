using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace Namingway {
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Plugin : IDalamudPlugin {
        public string Name => "Namingway";

        [PluginService]
        internal DalamudPluginInterface Interface { get; init; } = null!;

        [PluginService]
        internal ClientState ClientState { get; init; } = null!;

        [PluginService]
        internal CommandManager CommandManager { get; init; } = null!;

        [PluginService]
        internal DataManager DataManager { get; init; } = null!;

        [PluginService]
        internal SigScanner SigScanner { get; init; } = null!;

        internal Configuration Config { get; }
        internal Renamer Renamer { get; }
        internal PluginUi Ui { get; }
        private Commands Commands { get; }

        public Plugin() {
            this.Config = this.Interface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Config.UpdateActive();

            this.Renamer = new Renamer(this);
            this.Ui = new PluginUi(this);
            this.Commands = new Commands(this);

            foreach (var pack in this.Config.FindEnabledPacks()) {
                pack.Enable(this.Renamer);
            }
        }

        public void Dispose() {
            foreach (var pack in this.Config.FindEnabledPacks()) {
                pack.Disable(this.Renamer);
            }

            this.Commands.Dispose();
            this.Ui.Dispose();
            this.Renamer.Dispose();
        }

        internal void SaveConfig() {
            this.Interface.SavePluginConfig(this.Config);
        }
    }
}
