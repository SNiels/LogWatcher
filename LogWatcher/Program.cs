using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LogWatcher
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            //pass in "path/to/log.*.txt
            var path = args[0];
            var dir = Path.GetDirectoryName(path);
            var fileFilter = Path.GetFileName(path);
            TailFile(GetMostRecentLogFile(dir, fileFilter));
            Watch(dir, fileFilter);

            do
            {
                process = processFactory?.Invoke();
                process?.Start();
                process?.WaitForExit();
            } while (process != null);
        }

        private static string GetMostRecentLogFile(string dir, string fileFilter)
        {
            var directory = new DirectoryInfo(dir);
            var file = directory.GetFiles(fileFilter)
                .OrderByDescending(f => f.LastWriteTime)
                .First();
            return file.FullName;
        }

        private static FileSystemWatcher watcher;
        private static Func<Process> processFactory;
        private static Process process;

        private static void Watch(string dir, string fileFilter)
        {
            watcher = new FileSystemWatcher
            {
                Path = dir,
                Filter = fileFilter
            };
            watcher.Created += FileCreated;
            watcher.EnableRaisingEvents = true;
        }

        private static void FileCreated(object sender, FileSystemEventArgs e) => TailFile(e.FullPath);

        private static void TailFile(string file)
        {
            processFactory = () =>
            {
                var process = new Process {StartInfo = new ProcessStartInfo("powershell.exe", $"type -wait '{file}'")};
                processFactory = null;
                return process;
            };
            process?.Kill();
        }
    }
}
