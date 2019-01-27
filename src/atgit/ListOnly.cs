using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace atgit
{
    internal class ListOnly : ActionBase
    {
        public ListOnly(Options options) : base(options)
        {
        }

        public int Execute()
        {
            var repos = FindGitRepositories();
            foreach (var repo in repos)
                Console.WriteLine(repo.FullName);

            return 0;
        }
    }
}