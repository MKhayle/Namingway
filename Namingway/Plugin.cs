using System;
using Dalamud.Plugin;

namespace Namingway {
    internal class Plugin : IDisposable {
        internal DalamudPluginInterface Interface { get; }

        internal Configuration Config { get; }
        internal Renamer Renamer { get; }
        internal PluginUi Ui { get; }
        private Commands Commands { get; }

        internal Plugin(DalamudPluginInterface pluginInterface) {
            this.Interface = pluginInterface;

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
