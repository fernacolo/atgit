using System;
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
    }
}