using System;
using System.Collections.Generic;
using System.IO;

namespace Arcadia.TeknoParrot {
    public static class RomScanner {
        public static List<string> Scan(string romsFolder) {
            var roms = new List<string>();
            foreach (var file in Directory.GetFiles(romsFolder, "*.exe", SearchOption.AllDirectories)) {
                roms.Add(file);
            }
            return roms;
        }
    }
}
