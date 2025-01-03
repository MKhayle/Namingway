using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using ImGuiNET;
using Lumina.Excel.Sheets;
using Newtonsoft.Json;

namespace Namingway;

internal class PluginUi : IDisposable {
    private Plugin Plugin { get; }
    private Dictionary<uint, ActionIndirection> Indirections { get; } = new();
    private HashSet<uint> ZadnorActions { get; } = [];
    private HashSet<uint> EurekaActions { get; } = [];

    internal bool DrawSettings;

    private Pack? _pack;
    private bool _editing;
    private string _importJson = string.Empty;
    private Pack? _importPack;

    internal PluginUi(Plugin plugin) {
        this.Plugin = plugin;

        this.Plugin.Interface.UiBuilder.Draw += this.Draw;
        this.Plugin.Interface.UiBuilder.OpenConfigUi += this.OnOpenConfig;

        this.FilterActions();
        this.FilterStatuses();
    }

    public void Dispose() {
        this.Plugin.Interface.UiBuilder.OpenConfigUi -= this.OnOpenConfig;
        this.Plugin.Interface.UiBuilder.Draw -= this.Draw;
    }

    private void OnOpenConfig() {
        this.DrawSettings = true;
    }

    #region Drawing

    private void Draw() {
        if (!this.DrawSettings) {
            return;
        }

        ImGui.SetNextWindowSize(new Vector2(450, 400), ImGuiCond.FirstUseEver);
        if (!ImGui.Begin(Plugin.Name, ref this.DrawSettings, ImGuiWindowFlags.MenuBar)) {
            ImGui.End();
            return;
        }

        if (ImGui.BeginMenuBar()) {
            if (ImGui.BeginMenu("Pack")) {
                ImGui.PushID("pack-menu");

                if (ImGui.MenuItem("New")) {
                    this.Plugin.Config.CustomPacks.Add(new Pack("Untitled Pack") {
                        Id = Guid.NewGuid(),
                    });
                }

                if (ImGui.BeginMenu("Import")) {
                    ImGui.SetNextItemWidth(250);
                    if (ImGui.InputText("##import-json", ref this._importJson, 5120)) {
                        try {
                            this._importPack = JsonConvert.DeserializeObject<Pack>(this._importJson);
                        } catch (JsonException) {
                            this._importPack = null;
                        }
                    }

                    if (this._importPack != null) {
                        ImGui.TextUnformatted($"Name: {this._importPack.Name}");
                        ImGui.TextUnformatted($"Actions: {this._importPack.Actions.Count}");
                        ImGui.TextUnformatted($"Status effects: {this._importPack.Statuses.Count}");

                        var existing = this.Plugin.Config.FindEnabledPack(this._importPack.Id);
                        if (existing != null) {
                            ImGui.TextUnformatted($"A pack with this ID already exists: {existing.Name}.");
                        } else if (ImGui.Button("Import##import-button")) {
                            this.Plugin.Config.CustomPacks.Add(this._importPack);
                            this._importPack = null;
                            this._importJson = string.Empty;
                            this.Plugin.SaveConfig();
                        }
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Export")) {
                    foreach (var pack in this.Plugin.Config.CustomPacks) {
                        if (ImGui.MenuItem($"{pack.Name}##{pack.Id}")) {
                            ExportPack(pack);
                        }
                    }

                    ImGui.EndMenu();
                }

                ImGui.PopID();
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Options")) {
                ImGui.PushID("options-menu");

                var anyChanged = false;

                if (ImGui.MenuItem("Only show player actions", null, ref this.Plugin.Config.OnlyPlayerActions)) {
                    this.FilterActions();
                    anyChanged = true;
                }

                if (anyChanged) {
                    this.Plugin.SaveConfig();
                }

                ImGui.PopID();
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

            foreach (var (id, name) in pack.Actions) {
                var action = this.Plugin.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Action>()!.GetRow(id);
                if (action.RowId == 0) {
                    continue;
                }

                ImGui.TableNextColumn();
                this.DrawIcon(action.Icon, new Vector2(ImGui.GetTextLineHeightWithSpacing()));

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(GetActionName(action));

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(name);
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

            foreach (var (id, name) in pack.Statuses) {
                var status = this.Plugin.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Status>()!.GetRow(id);
                if (status.RowId == 0) {
                    continue;
                }

                ImGui.TableNextColumn();
                this.DrawRatioIcon(status.Icon);

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(status.Name.ToString());

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(name);
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

            if (Util.IconButton(FontAwesomeIcon.Trash, "pack")) {
                this._editing = false;
                this._pack = null;
                this.Plugin.Config.CustomPacks.Remove(pack);
                this.Plugin.Config.EnabledPacks.Remove(pack.Id);
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

            foreach (var (id, name) in pack.Actions) {
                var action = this.Plugin.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Action>()!.GetRow(id);
                if (action.RowId == 0) {
                    continue;
                }

                ImGui.TableNextColumn();
                this.DrawIcon(action.Icon, new Vector2(ImGui.GetTextLineHeightWithSpacing()));

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(GetActionName(action));

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(name);

                ImGui.TableNextColumn();
                if (Util.IconButton(FontAwesomeIcon.Trash, $"action-{id}")) {
                    remove = id;
                }
            }

            if (remove > 0) {
                pack.Actions.Remove(remove);
            }

            var editAction = this.Plugin.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Action>()!.GetRow(this._editActionId);
            ImGui.TableNextColumn();
            if (editAction.RowId != 0) {
                this.DrawIcon(editAction.Icon, new Vector2(ImGui.GetTextLineHeightWithSpacing()));
            }

            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(-1);
            if (ImGui.BeginCombo("##edit-action", editAction.Name.ToString() ?? string.Empty)) {
                ImGui.SetNextItemWidth(-1);
                if (ImGui.InputTextWithHint("##edit-action-search", "Search...", ref this._editActionSearch, 100)) {
                    this.FilterActions();
                    this._editActionId = 0;
                }

                ImGuiListClipperPtr clipper;
                unsafe {
                    clipper = ImGuiNative.ImGuiListClipper_ImGuiListClipper();
                }

                clipper.Begin(this.FilteredActions.Count);

                while (clipper.Step()) {
                    for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++) {
                        var action = this.FilteredActions[i];

                        var contained = pack.Actions.ContainsKey(action.RowId);
                        var flags = contained
                            ? ImGuiSelectableFlags.Disabled
                            : ImGuiSelectableFlags.None;

                        if (contained) {
                            unsafe {
                                ImGui.PushStyleColor(ImGuiCol.Text, *ImGui.GetStyleColorVec4(ImGuiCol.TextDisabled));
                            }
                        }

                        if (ImGui.Selectable($"##action-{action.Name}##{action.RowId}", false, flags)) {
                            this._editActionId = action.RowId;
                        }

                        ImGui.SameLine();

                        this.DrawIcon(action.Icon, new Vector2(ImGui.GetTextLineHeightWithSpacing()));
                        ImGui.SameLine();
                        ImGui.TextUnformatted(GetActionName(action));

                        if (contained) {
                            ImGui.PopStyleColor();
                        }
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGui.InputText("##edit-action-name", ref this._editActionName, 100);

            ImGui.TableNextColumn();
            if (Util.IconButton(FontAwesomeIcon.Plus, "action") && this._editActionId > 0 && this._editActionName.Length > 0) {
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

            foreach (var (id, name) in pack.Statuses) {
                var status = this.Plugin.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Status>()!.GetRow(id);
                if (status.RowId == 0) {
                    continue;
                }

                ImGui.TableNextColumn();
                this.DrawRatioIcon(status.Icon);

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(status.Name.ToString());

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(name);

                ImGui.TableNextColumn();
                if (Util.IconButton(FontAwesomeIcon.Trash, $"status-{id}")) {
                    remove = id;
                }
            }

            if (remove > 0) {
                pack.Statuses.Remove(remove);
            }

            var editStatus = this.Plugin.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Status>()!.GetRow(this._editStatusId);
            ImGui.TableNextColumn();
            if (editStatus.RowId != 0) {
                this.DrawRatioIcon(editStatus.Icon);
            }

            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(-1);
            if (ImGui.BeginCombo("##edit-status", editStatus.Name.ToString() ?? string.Empty)) {
                ImGui.SetNextItemWidth(-1);
                if (ImGui.InputTextWithHint("##edit-status-search", "Search...", ref this._editStatusSearch, 100)) {
                    this._editStatusId = 0;
                    this.FilterStatuses();
                }

                ImGuiListClipperPtr clipper;
                unsafe {
                    clipper = ImGuiNative.ImGuiListClipper_ImGuiListClipper();
                }

                clipper.Begin(this.FilteredStatuses.Count);
                while (clipper.Step()) {
                    for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++) {
                        var status = this.FilteredStatuses[i];

                        var contained = pack.Statuses.ContainsKey(status.RowId);
                        var flags = contained
                            ? ImGuiSelectableFlags.Disabled
                            : ImGuiSelectableFlags.None;

                        if (contained) {
                            unsafe {
                                ImGui.PushStyleColor(ImGuiCol.Text, *ImGui.GetStyleColorVec4(ImGuiCol.TextDisabled));
                            }
                        }

                        if (ImGui.Selectable($"##status-{status.Name}##{status.RowId}", false, flags)) {
                            this._editStatusId = status.RowId;
                        }

                        ImGui.SameLine();

                        this.DrawRatioIcon(status.Icon);
                        ImGui.SameLine();
                        ImGui.TextUnformatted($"{status.Name}");

                        if (contained) {
                            ImGui.PopStyleColor();
                        }
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGui.InputText("##edit-status-name", ref this._editStatusName, 100);

            ImGui.TableNextColumn();
            if (Util.IconButton(FontAwesomeIcon.Plus, "status") && this._editStatusId > 0 && this._editStatusName.Length > 0) {
                pack.Statuses[this._editStatusId] = this._editStatusName;
                this._editStatusId = 0;
                this._editStatusName = string.Empty;
            }

            ImGui.EndTable();
        }
    }

    #endregion Drawing

    private List<Lumina.Excel.Sheets.Action> FilteredActions { get; } = [];
    private List<Lumina.Excel.Sheets.Status> FilteredStatuses { get; } = [];

    private void FilterActions() {
        if (this.Indirections.Count == 0) {
            foreach (var indirection in this.Plugin.DataManager.GetExcelSheet<ActionIndirection>()!) {
                if (indirection.Name.RowId == 0) {
                    continue;
                }

                this.Indirections[indirection.Name.RowId] = indirection;
            }
        }

        if (this.ZadnorActions.Count == 0) {
            foreach (var myc in this.Plugin.DataManager.GetExcelSheet<MYCTemporaryItem>()!) {
                if (myc.Action.RowId == 0) {
                    continue;
                }

                this.ZadnorActions.Add(myc.Action.RowId);
            }
        }

        if (this.EurekaActions.Count == 0) {
            foreach (var eureka in this.Plugin.DataManager.GetExcelSheet<EurekaMagiaAction>()!) {
                if (eureka.Action.RowId == 0) {
                    continue;
                }

                this.EurekaActions.Add(eureka.Action.RowId);
            }
        }

        this.FilteredActions.Clear();

        foreach (var action in this.Plugin.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Action>()!) {
            if (this.Plugin.Config.OnlyPlayerActions && !action.IsPlayerAction) {
                var allow = this.Indirections.TryGetValue(action.RowId, out var indirection) && indirection.ClassJob.RowId != uint.MaxValue
                            || this.ZadnorActions.Contains(action.RowId)
                            || this.EurekaActions.Contains(action.RowId);

                if (!allow) {
                    continue;
                }
            }

            if (action.Icon == 0 || action.Name.ToString().Length == 0) {
                continue;
            }

            // ReSharper disable once ConstantConditionalAccessQualifier
            if (this.Plugin.Config.OnlyPlayerActions && (action.ClassJobCategory.ValueNullable?.Name.ToString().Length ?? 0) == 0) {
                continue;
            }

            if (this._editActionSearch.Length > 0 && !action.Name.ToString().ToLowerInvariant().Contains(this._editActionSearch.ToLowerInvariant())) {
                continue;
            }

            this.FilteredActions.Add(action);
        }
    }

    private void FilterStatuses() {
        this.FilteredStatuses.Clear();

        foreach (var status in this.Plugin.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Status>()!) {
            if (status.Icon == 0) {
                continue;
            }

            if (this._editStatusSearch.Length > 0 && !status.Name.ToString().ToLowerInvariant().Contains(this._editStatusSearch.ToLowerInvariant())) {
                continue;
            }

            this.FilteredStatuses.Add(status);
        }
    }

    private static void ExportPack(Pack pack) {
        var json = JsonConvert.SerializeObject(pack);
        ImGui.SetClipboardText(json);
    }

    private IDalamudTextureWrap? GetIcon(uint id) {
        return this.Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(id)).GetWrapOrDefault();
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

    private static string GetActionName(Lumina.Excel.Sheets.Action action) {
        return GetName(action.Name.ToString(), action.IsPvP);
    }

    private static string GetName(string name, bool pvp) {
        if (pvp) {
            name += " (PvP)";
        }

        return name;
    }
}
