using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Hooking;

namespace Namingway;

internal class Renamer : IDisposable {
    private static class Signatures {
        internal const string GetAbilitySheet = "E8 ?? ?? ?? ?? 80 FB 12";
        internal const string GetStatusSheet = "E8 ?? ?? ?? ?? 48 85 C0 74 96";
    }

    private delegate IntPtr GetAbilitySheetDelegate(uint abilityId);

    private delegate IntPtr GetStatusSheetDelegate(uint effectId);

    private NamingwayPlugin Plugin { get; }
    private Dictionary<uint, IntPtr> StatusSheets { get; } = new();

    private GetAbilitySheetDelegate? GetAbilitySheet { get; }

    // private Hook<GetAbilitySheetDelegate>? GetAbilitySheetHook { get; }
    private Hook<GetStatusSheetDelegate>? GetStatusSheetHook { get; }

    internal Renamer(NamingwayPlugin plugin) {
        this.Plugin = plugin;

        

        if (Service.SigScanner.TryScanText(Signatures.GetAbilitySheet, out var abilityPtr)) {
            this.GetAbilitySheet = Marshal.GetDelegateForFunctionPointer<GetAbilitySheetDelegate>(abilityPtr);
        }

        // if (this.Plugin.Interface.TargetModuleScanner.TryScanText(Signatures.GetAbilitySheet, out var abilityPtr)) {
        //     this.GetAbilitySheetHook = new Hook<GetAbilitySheetDelegate>(abilityPtr, this.GetAbilitySheetDetour);
        //     this.GetAbilitySheetHook.Enable();
        // }

        if (Service.SigScanner.TryScanText(Signatures.GetStatusSheet, out var statusPtr)) {
            this.GetStatusSheetHook = Service.GameInteropProvider.HookFromAddress<GetStatusSheetDelegate>(statusPtr, this.GetStatusSheetDetour);
            this.GetStatusSheetHook.Enable();
        }
    }

    public void Dispose() {
        // this.GetAbilitySheetHook?.Dispose();
        this.GetStatusSheetHook?.Dispose();

        foreach (var ptr in this.StatusSheets.Values) {
            Marshal.FreeHGlobal(ptr);
        }
    }

    internal void RestoreAbility(uint abilityId) {
        var name = Service.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Action>()!.GetRow(abilityId).Name;
        if (name.IsEmpty) {
			name = Service.DataManager.GetExcelSheet<Lumina.Excel.Sheets.CraftAction>()!.GetRow(abilityId).Name;
		}
        if (name.IsEmpty) {
            return;
        }

        this.RenameAbility(abilityId, name.ToString());
    }

    internal void RenameAbility(uint abilityId, string name) {
        if (this.GetAbilitySheet == null) {
            return;
        }

        var data = this.GetAbilitySheet(abilityId);
        if (data == IntPtr.Zero) {
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(name);
        var offset = Marshal.ReadByte(data);
        Marshal.Copy(bytes, 0, data + offset, bytes.Length);
        Marshal.WriteByte(data + offset + bytes.Length, 0);
    }

    //
    // private IntPtr GetAbilitySheetDetour(uint abilityId) {
    //     var data = this.GetAbilitySheetHook!.Original(abilityId);
    //
    //     if (data != IntPtr.Zero && this.Plugin.Config.ActiveActions.TryGetValue(abilityId, out var name)) {
    //         var bytes = Encoding.UTF8.GetBytes(name);
    //         var offset = Marshal.ReadByte(data);
    //         Marshal.Copy(bytes, 0, data + offset, bytes.Length);
    //         Marshal.WriteByte(data + offset + bytes.Length, 0);
    //     }
    //
    //     return data;
    // }

    private IntPtr GetStatusSheetDetour(uint statusId) {
        var data = this.GetStatusSheetHook!.Original(statusId);

        try {
            return this.GetStatusSheetDetourInner(statusId, data);
        } catch (Exception ex) {
            Service.Log.Error(ex, "Exception in GetStatusSheetDetour");
        }

        return data;
    }

    private IntPtr GetStatusSheetDetourInner(uint statusId, IntPtr data) {
        const int nameOffset = 0;
        const int descOffset = 4;
        const int icon = 8;

        if (this.Plugin.Config.ActiveStatuses.TryGetValue(statusId, out var name)) {
            if (this.StatusSheets.TryGetValue(statusId, out var cached)) {
                return cached + nameOffset;
            }

            var raw = new byte[1024];
            Marshal.Copy(data - nameOffset, raw, 0, raw.Length);
            var nameBytes = Encoding.UTF8.GetBytes(name);
            var oldName = Util.ReadRawBytes(data + raw[nameOffset]);
            var descBytes = Util.ReadRawBytes(data + raw[descOffset] + 4);
            var oldPost = raw[nameOffset] + oldName.Length + 1 + descBytes.Length + 1;
            var newPost = raw[nameOffset] + nameBytes.Length + 1 + descBytes.Length + 1;

            var newData = new byte[1536];

            // copy over header
            for (var i = 0; i < nameOffset + raw[nameOffset]; i++) {
                newData[i] = raw[i];
            }

            newData[nameOffset] = raw[nameOffset];
            newData[descOffset] = (byte) (nameOffset + newData[nameOffset] + nameBytes.Length + 1 - 4);

            // copy icon
            for (var i = 0; i < 4; i++) {
                newData[icon + i] = raw[icon + i];
            }

            // copy name
            for (var i = 0; i < nameBytes.Length; i++) {
                newData[nameOffset + newData[nameOffset] + i] = nameBytes[i];
            }

            // copy description
            for (var i = 0; i < descBytes.Length; i++) {
                newData[descOffset + newData[descOffset] + i] = descBytes[i];
            }

            // copy post-description info
            for (var i = 0; i < raw.Length - oldPost; i++) {
                newData[newPost + i] = raw[oldPost + i];
            }

            var newSheet = Marshal.AllocHGlobal(newData.Length);
            Marshal.Copy(newData, 0, newSheet, newData.Length);

            this.StatusSheets[statusId] = newSheet;
            return newSheet + nameOffset;
        }

        return data;
    }
}
