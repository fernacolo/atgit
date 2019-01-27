using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace atgit
{
    internal class Runner : ActionBase
    {
        public Runner(Options options) : base(options)
        {
        }

        public int Execute()
        {
            var repos = FindGitRepositories();
            foreach (var repo in repos)
            {
                var result = RunCommand(repo);
                if (result != 0)
                {
                    Error(() => $"Exit code: {result}.");
                    if (!_options.Force)
                    {
                        Info(() => $"Aborted. Use -f to keep running on errors.");
                        return result;
                    }
                }
                
                Console.WriteLine();
            }

            return 0;
        }

        private int RunCommand(DirectoryInfo repo)
        {
            var si = new ProcessStartInfo();
            si.UseShellExecute = false;
            si.WorkingDirectory = repo.FullName;
            si.FileName = _options.Command;
            si.Arguments = _options.CommandArguments;
            Info(() => repo.FullName);
            var process = Process.Start(si);
            process.WaitForExit();
            return process.ExitCode;
        }

        private string GetGitArguments()
        {
            var result = _options.Command;
            if (_options.CommandArguments != null)
                result += " " + _options.CommandArguments;
            return result;
        }

        private IEnumerable<DirectoryInfo> FindGitRepositories()
        {
            var root = _options.RootDirectory;
            Verbose(() => $"Searching GIT repositories at {root.FullName}...");

            var pending = new Queue<DirectoryInfo>();
            foreach (var subdir in root.GetDirectories())
                pending.Enqueue(subdir);

            var found = new List<DirectoryInfo>();

            while (pending.Count > 0)
            {
                var subdir = pending.Dequeue();

                Verbose(() => $"Checking if {subdir.FullName} is Git repository...");
                var gitDir = new DirectoryInfo(Path.Combine(subdir.FullName, ".git"));
                if (gitDir.Exists)
                {
                    Verbose(() => $"Found Git repository: {subdir.FullName}.");
                    found.Add(subdir);
                    continue;
                }

                foreach (var subsubdir in subdir.GetDirectories())
                    pending.Enqueue(subsubdir);
            }

            return found;
        }
    }
}