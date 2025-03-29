using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Configuration;
using Dalamud.Plugin;
using Newtonsoft.Json;

namespace Namingway;


public class Configuration : IPluginConfiguration {
    [JsonIgnore] private IDalamudPluginInterface pluginInterface;
    public int Version { get; set; } = 1;

    public bool OnlyPlayerActions = true;

    public HashSet<Guid> EnabledPacks { get; set; } = [];
    public List<Pack> CustomPacks { get; set; } = [];

    [JsonIgnore]
    internal Dictionary<uint, string> ActiveActions { get; set; } = new();
    [JsonIgnore]
    internal Dictionary<uint, string> ActiveStatuses { get; set; } = new();

    internal void UpdateActive() {
            var packs = this.EnabledPacks
                .Select(this.FindEnabledPack)
                .Where(pack => pack != null)
                .ToList();

            this.ActiveActions = packs
                .SelectMany(pack => pack!.Actions)
                .Aggregate(new Dictionary<uint, string>(), (dict, entry) => {
                    dict[entry.Key] = entry.Value;
                    return dict;
                });

            this.ActiveStatuses = packs
                .SelectMany(pack => pack!.Statuses)
                .Aggregate(new Dictionary<uint, string>(), (dict, entry) => {
                    dict[entry.Key] = entry.Value;
                    return dict;
                });
        }

    internal Pack? FindEnabledPack(Guid id) {
            return DefaultPacks.All.FirstOrDefault(pack => pack.Id == id)
                   ?? this.CustomPacks.FirstOrDefault(pack => pack.Id == id);
        }

    internal IEnumerable<Pack> FindEnabledPacks() {
            foreach (var id in this.EnabledPacks) {
                var pack = this.FindEnabledPack(id);
                if (pack != null) {
                    yield return pack;
                }
            }
        }

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
    }
    internal void SaveConfig()
    {
        this.pluginInterface.SavePluginConfig(this);
    }
}

[Serializable]
public class Pack {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Dictionary<uint, string> Actions { get; set; } = new();
    public Dictionary<uint, string> Statuses { get; set; } = new();

    public Pack(string name) {
            this.Name = name;
        }

    internal void Enable(Renamer renamer) {
            foreach (var entry in this.Actions) {
                renamer.RenameAbility(entry.Key, entry.Value);
            }
        }

    internal void Disable(Renamer renamer) {
            foreach (var id in this.Actions.Keys) {
                renamer.RestoreAbility(id);
            }
        }
}