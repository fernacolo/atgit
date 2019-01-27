using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace atgit
{
    internal sealed class Options
    {
        public static void ShowIntro()
        {
            Console.WriteLine("atgit 0.8.1");
            Console.WriteLine("Visit the project home at https://github.com/fernacolo/atgit/");
            Console.WriteLine();
        }

        public static void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine();
            Console.WriteLine("    atgit [options] [-- <command>]");
            Console.WriteLine();
            Console.WriteLine("Executes <command> on all Git repositories found in the tree of current directory.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine();
            Console.WriteLine("    -f, --force          Force execution on all repositories (ignores non-zero exit codes).");
            Console.WriteLine("    --add-to-path        Adds itself to HKEY_CURRENT_USER\\Environment\\Path (Windows only).");
            Console.WriteLine("    -v, --verbose        Enable verbose logging.");
        }

        public Options(string[] args)
        {
            Action = OptionsAction.None;

            var argsScanner = ((IEnumerable<string>) args).GetEnumerator();
            while (argsScanner.MoveNext())
            {
                var curArg = argsScanner.Current;
                if (curArg == "--")
                {
                    if (IsAmbiguityError(curArg))
                        return;

                    if (!argsScanner.MoveNext())
                    {
                        Action = OptionsAction.ShowHelp;
                        ErrorMessage = "A command was not specified after \"--\".";
                        ResultCode = ResultCodes.InvalidArguments;
                        return;
                    }

                    Action = OptionsAction.RunCommand;
                    RootDirectory = new DirectoryInfo(new DirectoryInfo(".").FullName);
                    Command = argsScanner.Current;
                    CommandArguments = Flush(argsScanner);
                    return;
                }

                if (curArg.StartsWith("--"))
                {
                    ParseOptionArg(curArg, argsScanner);
                    continue;
                }

                if (curArg.StartsWith("-"))
                {
                    ParseMultiOptionsArg(curArg, argsScanner);
                    continue;
                }

                SetInvalidArgument(curArg);
                return;
            }

            if (argsScanner.MoveNext())
                throw new InvalidOperationException();

            if (Action == OptionsAction.None)
            {
                ShowIntro();
                Action = OptionsAction.ShowHelp;
            }
        }

        private bool IsAmbiguityError(string curArg)
        {
            if (Action == OptionsAction.None)
                return false;

            Action = OptionsAction.ShowHelp;
            ErrorMessage = $"Ambiguity detected: \"{curArg}\".";
            ResultCode = ResultCodes.InvalidArguments;
            return true;
        }


        private void SetInvalidArgument(string curArg)
        {
            Action = OptionsAction.ShowHelp;
            ErrorMessage = $"Unrecognized option: \"{curArg}\".";
            ResultCode = ResultCodes.InvalidArguments;
        }

        private void ParseOptionArg(string curArg, IEnumerator<string> argsScanner)
        {
            if (curArg.Length < 4)
            {
                SetInvalidArgument(curArg);
                return;
            }

            switch (curArg.Substring(2))
            {
                case "add-to-path":
                    if (IsAmbiguityError(curArg))
                        return;
                    Action = OptionsAction.AddToPath;
                    return;

                case "force":
                    Force = true;
                    return;

                case "verbose":
                    Verbose = true;
                    return;

                default:
                    SetInvalidArgument(curArg);
                    return;
            }
        }

        private void ParseMultiOptionsArg(string curArg, IEnumerator<string> argsScanner)
        {
            if (curArg.Length < 2)
            {
                SetInvalidArgument(curArg);
                return;
            }

            for (var i = 1; i < curArg.Length; ++i)
            {
                var ch = curArg[i];
                switch (ch)
                {
                    case 'v':
                        Verbose = true;
                        break;

                    case 'f':
                        Force = true;
                        return;

                    default:
                        Action = OptionsAction.ShowHelp;
                        ErrorMessage = $"Unrecognized option character '{ch}' at \"{curArg}\".";
                        ResultCode = ResultCodes.InvalidArguments;
                        return;
                }
            }
        }

        private static string Flush(IEnumerator<string> argsScanner)
        {
            if (!argsScanner.MoveNext())
                return null;

            var singleArg = argsScanner.Current;
            if (!argsScanner.MoveNext())
                return singleArg;

            var sb = new StringBuilder();
            sb.Append(singleArg);
            do
            {
                sb.Append(" ");
                sb.Append(argsScanner.Current);
            } while (argsScanner.MoveNext());

            return sb.ToString();
        }

        public OptionsAction Action { get; private set; }
        public bool Verbose { get; private set; }
        public string ErrorMessage { get; private set; }
        public DirectoryInfo RootDirectory { get; }
        public string Command { get; }
        public string CommandArguments { get; }
        public int ResultCode { get; private set; }
        public bool Force { get; private set; }
    }
}