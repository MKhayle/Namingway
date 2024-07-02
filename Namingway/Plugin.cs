using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace Namingway {
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Plugin : IDalamudPlugin {
        internal static string Name => "Namingway";

        [PluginService]
        internal static IPluginLog Log { get; private set; } = null!;

        [PluginService]
        internal IDalamudPluginInterface Interface { get; init; } = null!;

        [PluginService]
        internal IClientState ClientState { get; init; } = null!;

        [PluginService]
        internal ICommandManager CommandManager { get; init; } = null!;

        [PluginService]
        internal IDataManager DataManager { get; init; } = null!;

        [PluginService]
        internal ISigScanner SigScanner { get; init; } = null!;

        [PluginService]
        internal ITextureProvider TextureProvider { get; init; } = null!;

        [PluginService]
        internal IGameInteropProvider GameInteropProvider { get; init; } = null!;

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
