using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Hooking;

namespace Namingway;

internal class Renamer : IDisposable {
    private static class Signatures {
        internal const string GetAbilitySheet = "E8 ?? ?? ?? ?? F6 40 3E 10";
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
        const int descOffset = 4;
        const int minHeaderSize = 12; // name offset, description offset, and icon

        // the game returns null for missing rows; never dereference it
        if (data == IntPtr.Zero || !this.Plugin.Config.ActiveStatuses.TryGetValue(statusId, out var name)) {
            return data;
        }

        if (this.StatusSheets.TryGetValue(statusId, out var cached)) {
            return cached;
        }

        // the name offset doubles as the header size, since the name string
        // immediately follows the fixed columns
        var headerSize = Marshal.ReadByte(data);
        if (headerSize < minHeaderSize) {
            return data;
        }

        var header = new byte[headerSize];
        Marshal.Copy(data, header, 0, headerSize);

        var nameBytes = Encoding.UTF8.GetBytes(name);
        var descBytes = Util.ReadRawBytes(data + header[descOffset] + descOffset);

        // the description offset is a single byte, so truncate names that
        // would push it past 255
        var maxNameLength = byte.MaxValue + descOffset - headerSize - 1;
        if (nameBytes.Length > maxNameLength) {
            Array.Resize(ref nameBytes, maxNameLength);
        }

        // header, then name and description, each null-terminated
        var newData = new byte[headerSize + nameBytes.Length + 1 + descBytes.Length + 1];
        Array.Copy(header, newData, headerSize);
        newData[descOffset] = (byte) (headerSize + nameBytes.Length + 1 - descOffset);
        Array.Copy(nameBytes, 0, newData, headerSize, nameBytes.Length);
        Array.Copy(descBytes, 0, newData, headerSize + nameBytes.Length + 1, descBytes.Length);

        var newSheet = Marshal.AllocHGlobal(newData.Length);
        Marshal.Copy(newData, 0, newSheet, newData.Length);

        this.StatusSheets[statusId] = newSheet;
        return newSheet;
    }
}
