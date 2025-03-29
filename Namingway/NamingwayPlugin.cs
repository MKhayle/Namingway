using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace Namingway;

// ReSharper disable once ClassNeverInstantiated.Global
public class NamingwayPlugin : IDalamudPlugin {
    internal static string Name => "Namingway";
    public static NamingwayPlugin? Plugin { get; private set; }

    internal Configuration Config { get; }
    internal Renamer Renamer { get; }
    internal PluginUi Ui { get; }
    private Commands Commands { get; }

    public NamingwayPlugin(IDalamudPluginInterface pluginInterface) {
        Plugin = this;
        pluginInterface.Create<Service>();

        this.Config = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        this.Config.Initialize(pluginInterface);

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


}
