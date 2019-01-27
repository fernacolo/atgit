using System;
using System.Collections.Generic;
using System.IO;

namespace atgit
{
    internal class ActionBase
    {
        protected readonly Options _options;

        protected ActionBase(Options options)
        {
            _options = options;
        }

        protected void Verbose(Func<string> getMessage)
        {
            if (!_options.Verbose)
                return;
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(getMessage());
            Console.ResetColor();
        }

        protected void Info(Func<string> getMessage)
        {
            //if (_options.Quiet)
            //    return;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(getMessage());
            Console.ResetColor();
        }

        protected void Error(Func<string> getMessage)
        {
            //if (_options.Quiet)
            //    return;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(getMessage());
            Console.ResetColor();
        }

        protected IEnumerable<DirectoryInfo> FindGitRepositories()
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