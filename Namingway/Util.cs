using System;
using System.Collections.Generic;
using Dalamud.Game;
using Dalamud.Interface;
using Dalamud.Bindings.ImGui;

namespace Namingway;

internal static class Util {
    internal static bool TryScanText(this SigScanner scanner, string sig, out IntPtr result) {
            result = IntPtr.Zero;
            try {
                result = scanner.ScanText(sig);
                return true;
            } catch (KeyNotFoundException) {
                return false;
            }
        }

    internal static unsafe byte[] ReadRawBytes(IntPtr ptr) {
            var bytes = new List<byte>();

            var bytePtr = (byte*) ptr;
            while (*bytePtr != 0) {
                bytes.Add(*bytePtr);
                bytePtr += 1;
            }

            return bytes.ToArray();
        }

    internal static bool IconButton(FontAwesomeIcon icon, string? id = null) {
            var label = icon.ToIconString();
            if (id != null) {
                label += $"##{id}";
            }

            ImGui.PushFont(UiBuilder.IconFont);

            var ret = ImGui.Button(label);

            ImGui.PopFont();

            return ret;
        }
}