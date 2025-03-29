﻿using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Namingway
{
    internal class Service
    {
        [PluginService] internal static IPluginLog Log { get; private set; } = null!;
        [PluginService] internal static IDalamudPluginInterface Interface { get; private set; } = null!;
        [PluginService] internal static IClientState ClientState { get; private set; } = null!;
        [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
        [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;
        [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
        [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    }
}
