using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace atgit
{
    internal sealed class Options
    {
        public static void ShowIntro()
        {
            var productVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

            Console.WriteLine($"atgit {productVersion}");
            Console.WriteLine("Visit the project home at https://github.com/fernacolo/atgit/");
            Console.WriteLine();
        }

        public static void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine();
            Console.WriteLine("    atgit [<options>] [<command>]");
            Console.WriteLine();
            Console.WriteLine("Executes <command> on all Git repositories found in the tree of current directory.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine();
            Console.WriteLine("    --add-to-path        Add itself to HKEY_CURRENT_USER\\Environment\\Path (Windows only).");
            Console.WriteLine("    -f, --force          Force execution on all repositories (ignore non-zero exit codes).");
            Console.WriteLine("    -l, --list           Only list repositories. Do not run any command.");
            Console.WriteLine("    -v, --verbose        Enable verbose logging.");
        }

        public Options(string[] args)
        {
            var argsScanner = ((IEnumerable<string>) args).GetEnumerator();
            while (argsScanner.MoveNext())
            {
                var curArg = argsScanner.Current;

                if (curArg.StartsWith("--"))
                {
                    if (!ParseOptionArg(curArg, argsScanner))
                        return;
                    continue;
                }

                if (curArg.StartsWith("-"))
                {
                    if (!ParseMultiOptionsArg(curArg, argsScanner))
                        return;
                    continue;
                }

                if (!SetActionIfNotAmbiguous(curArg + " (run command)", OptionsAction.RunCommand))
                    return;

                Command = curArg;
                CommandArguments = Flush(argsScanner);
                break;
            }

            if (argsScanner.MoveNext())
                throw new InvalidOperationException();

            if (Action != OptionsAction.None)
            {
                if (RootDirectory == null)
                    RootDirectory = new DirectoryInfo(new DirectoryInfo(".").FullName);
                return;
            }

            ShowIntro();
            _action = OptionsAction.ShowHelp;
        }

        private bool SetActionIfNotAmbiguous(string curArg, OptionsAction action)
        {
            if (_action == action)
                return true;

            if (_action != OptionsAction.None)
            {
                _action = OptionsAction.ShowHelp;
                ErrorMessage = $"Options are mutually exclusive: {_actionArg} and {curArg}.";
                ResultCode = ResultCodes.InvalidArguments;
                return false;
            }

            _action = action;
            _actionArg = curArg;
            return true;
        }

        private void SetInvalidArgument(string curArg)
        {
            _action = OptionsAction.ShowHelp;
            ErrorMessage = $"Unrecognized option: \"{curArg}\".";
            ResultCode = ResultCodes.InvalidArguments;
        }

        private bool ParseOptionArg(string curArg, IEnumerator<string> argsScanner)
        {
            if (curArg.Length < 4)
            {
                SetInvalidArgument(curArg);
                return false;
            }

            switch (curArg.Substring(2))
            {
                case "add-to-path":
                    return SetActionIfNotAmbiguous(curArg, OptionsAction.AddToPath);

                case "force":
                    Force = true;
                    return true;

                case "list":
                    return SetActionIfNotAmbiguous(curArg, OptionsAction.ListOnly);

                case "verbose":
                    Verbose = true;
                    return true;

                default:
                    SetInvalidArgument(curArg);
                    return false;
            }
        }

        private bool ParseMultiOptionsArg(string curArg, IEnumerator<string> argsScanner)
        {
            if (curArg.Length < 2)
            {
                SetInvalidArgument(curArg);
                return false;
            }

            for (var i = 1; i < curArg.Length; ++i)
            {
                var ch = curArg[i];
                switch (ch)
                {
                    case 'f':
                        Force = true;
                        break;

                    case 'l':
                        if (!SetActionIfNotAmbiguous("-l", OptionsAction.ListOnly))
                            return false;
                        break;

                    case 'v':
                        Verbose = true;
                        break;


                    default:
                        _action = OptionsAction.ShowHelp;
                        ErrorMessage = $"Unrecognized option character '{ch}' at \"{curArg}\".";
                        ResultCode = ResultCodes.InvalidArguments;
                        return false;
                }
            }

            return true;
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

        private OptionsAction _action;
        private string _actionArg;

        public OptionsAction Action => _action;
        public bool Verbose { get; private set; }
        public string ErrorMessage { get; private set; }
        public DirectoryInfo RootDirectory { get; }
        public string Command { get; }
        public string CommandArguments { get; }
        public int ResultCode { get; private set; }
        public bool Force { get; private set; }
    }
}