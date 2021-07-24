using System;
using System.Collections.Generic;
using Dalamud.Game;

namespace Namingway {
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
    }
}
