using System.Diagnostics;
using System.IO;

namespace albumica
{
    public static class TestVideo
    {
        public static void GetMeta(string location)
        {
            var fi = new FileInfo(location);

            var psi = new ProcessStartInfo("ffprobe", $"-v quiet -show_format -print_format json -i {fi.FullName}");
            psi.RedirectStandardOutput = true;
            var process = Process.Start(psi);
            process!.WaitForExit();

            var result = process.StandardOutput.ReadToEnd();
        }
    }
}