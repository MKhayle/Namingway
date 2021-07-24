using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using ImGuiScene;

namespace Namingway {
    internal class PluginUi : IDisposable {
        private Plugin Plugin { get; }
        private Dictionary<uint, TextureWrap> Icons { get; } = new();

        internal bool DrawSettings;

        private Pack? _pack;
        private bool _editing;

        internal PluginUi(Plugin plugin) {
            this.Plugin = plugin;

            this.Plugin.Interface.UiBuilder.OnBuildUi += this.Draw;
            this.Plugin.Interface.UiBuilder.OnOpenConfigUi += this.OnOpenConfig;
        }

        public void Dispose() {
            this.Plugin.Interface.UiBuilder.OnOpenConfigUi -= this.OnOpenConfig;
            this.Plugin.Interface.UiBuilder.OnBuildUi -= this.Draw;

            foreach (var icon in this.Icons.Values) {
                icon.Dispose();
            }
        }

        private void OnOpenConfig(object sender, EventArgs e) {
            this.DrawSettings = true;
        }

        private void Draw() {
            if (!this.DrawSettings) {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(450, 400), ImGuiCond.FirstUseEver);
            if (!ImGui.Begin(DalamudPlugin.PluginName, ref this.DrawSettings, ImGuiWindowFlags.MenuBar)) {
                ImGui.End();
                return;
            }

            if (ImGui.BeginMenuBar()) {
                if (ImGui.BeginMenu("Pack")) {
                    if (ImGui.MenuItem("Add")) {
                        this.Plugin.Config.CustomPacks.Add(new Pack("Untitled Pack") {
                            Id = Guid.NewGuid(),
                        });
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }

            if (ImGui.BeginChild("packs", new Vector2(ImGui.GetContentRegionAvail().X * 0.25f, -1))) {
                void DrawPacks(IEnumerable<Pack> packs) {
                    foreach (var pack in packs) {
                        var enabled = this.Plugin.Config.EnabledPacks.Contains(pack.Id);
                        if (!enabled) {
                            unsafe {
                                var disabled = *ImGui.GetStyleColorVec4(ImGuiCol.TextDisabled);
                                ImGui.PushStyleColor(ImGuiCol.Text, disabled);
                            }
                        }

                        if (ImGui.Selectable($"{pack.Name}##{pack.Id}", this._pack?.Id == pack.Id)) {
                            if (this._editing) {
                                this.Plugin.SaveConfig();
                            }

                            this._editing = false;
                            this._editPackName = pack.Name;
                            this._pack = pack;
                        }

                        if (!enabled) {
                            ImGui.PopStyleColor();
                        }
                    }
                }

                if (ImGui.TreeNode("Default")) {
                    DrawPacks(DefaultPacks.All);
                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Custom")) {
                    DrawPacks(this.Plugin.Config.CustomPacks);
                    ImGui.TreePop();
                }

                ImGui.EndChild();
            }

            ImGui.SameLine();

            if (ImGui.BeginChild("pack-view", new Vector2(-1, -1))) {
                if (this._pack != null) {
                    if (this._editing) {
                        this.DrawEditPackView(this._pack);
                    } else {
                        this.DrawPackView(this._pack);
                    }
                }

                ImGui.EndChild();
            }

            ImGui.End();
        }

        private void DrawPackView(Pack pack) {
            var custom = DefaultPacks.All.All(p => p.Id != pack.Id);
            var enabled = this.Plugin.Config.EnabledPacks.Contains(pack.Id);
            if (ImGui.Checkbox($"{pack.Name}##enable-{pack.Id}", ref enabled)) {
                if (enabled) {
                    this.Plugin.Config.EnabledPacks.Add(pack.Id);
                    pack.Enable(this.Plugin.Renamer);
                } else {
                    this.Plugin.Config.EnabledPacks.Remove(pack.Id);
                    pack.Disable(this.Plugin.Renamer);
                }

                this.Plugin.Config.UpdateActive();
                this.Plugin.SaveConfig();
            }

            if (custom) {
                ImGui.SameLine();

                ImGui.Checkbox("Edit mode", ref this._editing);
            }

            ImGui.Separator();

            ImGui.TextUnformatted("Actions");

            if (ImGui.BeginTable("actions", 3, ImGuiTableFlags.BordersInnerH)) {
                ImGui.TableSetupColumn("##icon", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Old name");
                ImGui.TableSetupColumn("New name");

                ImGui.TableHeadersRow();

                foreach (var entry in pack.Actions) {
                    var action = this.Plugin.Interface.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().GetRow(entry.Key);
                    if (action == null) {
                        continue;
                    }

                    ImGui.TableNextColumn();
                    this.DrawIcon(action.Icon, new Vector2(ImGui.GetTextLineHeightWithSpacing()));

                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(action.Name.ToString());

                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(entry.Value);
                }

                ImGui.EndTable();
            }

            ImGui.Separator();

            ImGui.TextUnformatted("Status effects");

            if (ImGui.BeginTable("statuses", 3, ImGuiTableFlags.BordersInnerH)) {
                ImGui.TableSetupColumn("##icon", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Old name");
                ImGui.TableSetupColumn("New name");

                ImGui.TableHeadersRow();

                foreach (var entry in pack.Statuses) {
                    var status = this.Plugin.Interface.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Status>().GetRow(entry.Key);
                    if (status == null) {
                        continue;
                    }

                    ImGui.TableNextColumn();
                    this.DrawRatioIcon(status.Icon);

                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(status.Name.ToString());

                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(entry.Value);
                }

                ImGui.EndTable();
            }
        }

        private string _editPackName = string.Empty;

        private uint _editActionId;
        private string _editActionSearch = string.Empty;
        private string _editActionName = string.Empty;

        private uint _editStatusId;
        private string _editStatusSearch = string.Empty;
        private string _editStatusName = string.Empty;

        private void DrawEditPackView(Pack pack) {
            var custom = DefaultPacks.All.All(p => p.Id != pack.Id);
            if (ImGui.InputText("##edit-pack-name", ref this._editPackName, 100)) {
                pack.Name = this._editPackName;
            }

            if (custom) {
                ImGui.SameLine();

                if (ImGui.Checkbox("Edit mode", ref this._editing)) {
                    this.Plugin.SaveConfig();
                }

                ImGui.SameLine();

                if (ImGui.Button("Delete##pack")) {
                    this._editing = false;
                    this._pack = null;
                    this.Plugin.Config.CustomPacks.Remove(pack);
                    this.Plugin.SaveConfig();
                    return;
                }
            }

            ImGui.Separator();

            ImGui.TextUnformatted("Actions");

            if (ImGui.BeginTable("actions", 4, ImGuiTableFlags.BordersInnerH)) {
                ImGui.TableSetupColumn("##icon", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Old name");
                ImGui.TableSetupColumn("New name");
                ImGui.TableSetupColumn("##button", ImGuiTableColumnFlags.WidthFixed);

                ImGui.TableHeadersRow();

                var remove = 0u;

                foreach (var entry in pack.Actions) {
                    var action = this.Plugin.Interface.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().GetRow(entry.Key);
                    if (action == null) {
                        continue;
                    }

                    ImGui.TableNextColumn();
                    this.DrawIcon(action.Icon, new Vector2(ImGui.GetTextLineHeightWithSpacing()));

                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(action.Name.ToString());

                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(entry.Value);

                    ImGui.TableNextColumn();
                    if (ImGui.Button($"Delete##action-{entry.Key}")) {
                        remove = entry.Key;
                    }
                }

                if (remove > 0) {
                    pack.Actions.Remove(remove);
                }

                var editAction = this._editActionId == 0
                    ? null
                    : this.Plugin.Interface.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().GetRow(this._editActionId);
                ImGui.TableNextColumn();
                if (editAction != null) {
                    this.DrawIcon(editAction.Icon, new Vector2(ImGui.GetTextLineHeightWithSpacing()));
                }

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(-1);
                if (ImGui.BeginCombo("##edit-action", editAction?.Name?.ToString() ?? string.Empty)) {
                    if (ImGui.InputText("##edit-action-search", ref this._editActionSearch, 100)) {
                        this._editActionId = 0;
                    }

                    foreach (var action in this.Plugin.Interface.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()) {
                        if (!action.IsPlayerAction) {
                            continue;
                        }

                        if (pack.Actions.ContainsKey(action.RowId)) {
                            continue;
                        }

                        if (this._editActionSearch.Length > 0 && !action.Name.ToString().ToLowerInvariant().Contains(this._editActionSearch.ToLowerInvariant())) {
                            continue;
                        }

                        if (ImGui.Selectable($"##action-{action.Name}##{action.RowId}")) {
                            this._editActionId = action.RowId;
                        }

                        ImGui.SameLine();

                        this.DrawIcon(action.Icon, new Vector2(ImGui.GetTextLineHeightWithSpacing()));
                        ImGui.SameLine();
                        ImGui.TextUnformatted($"{action.Name}");
                    }

                    ImGui.EndCombo();
                }

                ImGui.TableNextColumn();
                ImGui.InputText("##edit-action-name", ref this._editActionName, 100);

                ImGui.TableNextColumn();
                if (ImGui.Button("Add##action") && this._editActionId > 0 && this._editActionName.Length > 0) {
                    pack.Actions[this._editActionId] = this._editActionName;
                    this._editActionId = 0;
                    this._editActionName = string.Empty;
                }

                ImGui.EndTable();
            }

            ImGui.Separator();

            ImGui.TextUnformatted("Status effects");

            if (ImGui.BeginTable("statuses", 4, ImGuiTableFlags.BordersInnerH)) {
                ImGui.TableSetupColumn("##icon", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Old name");
                ImGui.TableSetupColumn("New name");
                ImGui.TableSetupColumn("##button", ImGuiTableColumnFlags.WidthFixed);

                ImGui.TableHeadersRow();

                var remove = 0u;

                foreach (var entry in pack.Statuses) {
                    var status = this.Plugin.Interface.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Status>().GetRow(entry.Key);
                    if (status == null) {
                        continue;
                    }

                    ImGui.TableNextColumn();
                    this.DrawRatioIcon(status.Icon);

                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(status.Name.ToString());

                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(entry.Value);

                    ImGui.TableNextColumn();
                    if (ImGui.Button($"Delete##status-{entry.Key}")) {
                        remove = entry.Key;
                    }
                }

                if (remove > 0) {
                    pack.Statuses.Remove(remove);
                }

                var editStatus = this._editStatusId == 0
                    ? null
                    : this.Plugin.Interface.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Status>().GetRow(this._editStatusId);
                ImGui.TableNextColumn();
                if (editStatus != null) {
                    this.DrawRatioIcon(editStatus.Icon);
                }

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(-1);
                if (ImGui.BeginCombo("##edit-status", editStatus?.Name?.ToString() ?? string.Empty)) {
                    if (ImGui.InputText("##edit-status-search", ref this._editStatusSearch, 100)) {
                        this._editStatusId = 0;
                    }

                    foreach (var status in this.Plugin.Interface.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Status>()) {
                        if (status.RowId == 0) {
                            continue;
                        }

                        if (pack.Statuses.ContainsKey(status.RowId)) {
                            continue;
                        }

                        if (this._editStatusSearch.Length > 0 && !status.Name.ToString().ToLowerInvariant().Contains(this._editStatusSearch.ToLowerInvariant())) {
                            continue;
                        }

                        if (ImGui.Selectable($"##status-{status.Name}##{status.RowId}")) {
                            this._editStatusId = status.RowId;
                        }

                        ImGui.SameLine();

                        this.DrawRatioIcon(status.Icon);
                        ImGui.SameLine();
                        ImGui.TextUnformatted($"{status.Name}");
                    }

                    ImGui.EndCombo();
                }

                ImGui.TableNextColumn();
                ImGui.InputText("##edit-status-name", ref this._editStatusName, 100);

                ImGui.TableNextColumn();
                if (ImGui.Button("Add##status") && this._editStatusId > 0 && this._editStatusName.Length > 0) {
                    pack.Statuses[this._editStatusId] = this._editStatusName;
                    this._editStatusId = 0;
                    this._editStatusName = string.Empty;
                }

                ImGui.EndTable();
            }
        }

        private TextureWrap? GetIcon(uint id) {
            if (this.Icons.TryGetValue(id, out var wrap)) {
                return wrap;
            }

            try {
                wrap = this.Plugin.Interface.Data.GetImGuiTextureIcon(this.Plugin.Interface.ClientState.ClientLanguage, (int) id);
            } catch (NullReferenceException) {
                return null;
            }

            this.Icons[id] = wrap;

            return wrap;
        }

        private void DrawIcon(uint id, Vector2 size = default) {
            var icon = this.GetIcon(id);
            if (icon == null) {
                return;
            }

            if (size == default) {
                size = new Vector2(icon.Width, icon.Height);
            }

            ImGui.Image(icon.ImGuiHandle, size);
        }

        private void DrawRatioIcon(uint id) {
            var icon = this.GetIcon(id);
            if (icon == null) {
                return;
            }

            var ratio = ImGui.GetTextLineHeightWithSpacing() / icon.Height;
            var size = new Vector2(icon.Width * ratio, icon.Height * ratio);
            ImGui.Image(icon.ImGuiHandle, size);
        }
    }
}
