using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace RoboSphere.Discord
{
    public static class GitHelper
    {
        private static void Exec(string git, string args, out string? stdout, out string? stderr)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = git,
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = @"C:\Program Files\Git\cmd\git.exe",
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            stdout = null;
            stderr = null;
            if (process == null) return;
            stdout = process.StandardOutput.ReadToEnd();
            stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();
        }

        public static string? Diff(string git, string? path, long? start, long? end)
        {
            if (!Directory.Exists(git)) return null;
            if (path != null)
            {
                var fullPath = Path.GetFullPath(Path.Combine(git, path));
                if (!fullPath.StartsWith(Path.GetFullPath(git)) || !File.Exists(fullPath)) return null;
            }
            if (start == null || end == null)
            {
                string? commits;
                const string logCommand = "log -n 2 --pretty=format:\"%h\" --no-patch";
                if (!string.IsNullOrWhiteSpace(path))
                    Exec(git, $"{logCommand} \"{path}\"", out commits, out _);
                else
                    Exec(git, logCommand, out commits, out _);

                if (string.IsNullOrWhiteSpace(commits)) return null;
                var commitIds = commits.Split('\n');
                if (commitIds.Length < 2) return null;
                start ??= long.Parse(commitIds[0], NumberStyles.HexNumber);
                end ??= long.Parse(commitIds[1], NumberStyles.HexNumber);
            }

            string? diff;
            var diffCommand = $"diff --minimal -U0 {start:x}..{end:x}";
            if (!string.IsNullOrWhiteSpace(path))
                Exec(git, $"{diffCommand} \"{path}\"", out diff, out _);
            else
                Exec(git, diffCommand, out diff, out _);

            return diff;
        }


        public static void Add(string git, string path)
        {
            var fullPath = Path.GetFullPath(Path.Combine(git, path));
            if (!Directory.Exists(git) || !fullPath.StartsWith(Path.GetFullPath(git)) || !(File.Exists(path) || Directory.Exists(path))) return;
            Exec(git, $"add \"{path}\"", out _, out _);
        }

        public static void Commit(string git, string commitMsg)
        {
            if (!Directory.Exists(git)) return;
            if (commitMsg.Contains("\\") || commitMsg.Contains("\"")) return;
            Exec(git, $"commit -am \"{commitMsg}\"", out _, out _);
        }

        public static void Push(string git)
        {
            if (!Directory.Exists(git)) return;
            Exec(git, "push -u origin develop", out _, out _);
        }

        public static void Pull(string git)
        {
            if (!Directory.Exists(git)) return;
            Exec(git, "pull -u origin", out _, out _);
        }

        public static string? GetHash(string git)
        {
            if (!Directory.Exists(git)) return null;
            Exec(git, "rev-parse --verify HEAD", out var hash, out _);
            return hash?.Trim();
        }
    }
}
