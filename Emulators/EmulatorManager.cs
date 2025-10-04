using System.Diagnostics;
using System.IO;
using System.Net;

namespace Arcadia.Emulators {
    public static class EmulatorManager {
        public static void DownloadAndUnpack(string url, string targetDir) {
            string zipPath = Path.Combine(targetDir, "temp.zip");
            new WebClient().DownloadFile(url, zipPath);
            System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, targetDir, true);
            File.Delete(zipPath);
        }

        public static void Launch(string exePath, string args = "") {
            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = exePath,
                    Arguments = args,
                    UseShellExecute = false
                }
            };
            process.Start();
        }
    }
}
