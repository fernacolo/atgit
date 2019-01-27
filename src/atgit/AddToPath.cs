using System;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
#if NET452

#endif

namespace atgit
{
    internal sealed class AddToPath : ActionBase
    {
        public AddToPath(Options options) : base(options)
        {
        }

        public int Execute()
        {
            var targetBin = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            Verbose(() => $"Current binary: {targetBin}");
            var targetDir = Path.GetDirectoryName(targetBin);

#if NET452
            Info(() => "Analysing your system...");

            var pathExt = GetPathExt();
            var targetKey = $"{Registry.CurrentUser.Name}\\Environment";

            var onProcessPath = GetMatchingPath(pathExt, Environment.GetEnvironmentVariable("PATH"));
            if (onProcessPath != null)
                Info(() => $"Found this on session PATH variable: {onProcessPath}");

            var onSystemPath = GetMatchingPath(pathExt, GetPathFromRegistry(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment"));
            if (onSystemPath != null)
                Info(() => $"Found this on system PATH variable: {onSystemPath}");

            var onUserPath = GetMatchingPath(pathExt, GetPathFromRegistry(Registry.CurrentUser, @"Environment"));
            if (onUserPath != null)
                Info(() => $"Found this on user PATH variable: {onUserPath}");

            if (string.Equals(onUserPath, targetDir, StringComparison.OrdinalIgnoreCase))
            {
                Info(() => $"The setting {targetKey}\\PATH is already up-to-date. No change performed.");
                return 0;
            }

            if (!_options.Force)
            {
                if (onProcessPath != null || onUserPath != null)
                {
                    Error(() => "Other atgit is on path. No change performed. Use -f to override this.");
                    return ResultCodes.InvalidState;
                }
            }

            if (onSystemPath != null)
            {
                Error(() => "Other atgit is on system path. No change performed.");
                return ResultCodes.InvalidState;
            }

            var environment = Registry.CurrentUser.OpenSubKey("Environment", true);
            if (environment == null)
            {
                Error(() => $"Could not find registry key: {targetKey}");
                return ResultCodes.InvalidState;
            }

            var curValue = (string) environment.GetValue("PATH");

            var newValue = string.Empty;
            if (onUserPath != null)
            {
                Info(() => $"Replacing {onUserPath} by {targetDir} on {targetKey}\\PATH...");
                var parts = curValue.Split(';');
                var ft = true;
                foreach (var part in parts)
                {
                    if (ft)
                        ft = false;
                    else
                        newValue += ';';

                    if (part.Trim() == onUserPath)
                    {
                        newValue += targetDir;
                        onUserPath = null; // replaces only first match.
                    }
                    else
                        newValue += part;
                }
            }
            else
            {
                Info(() => $"Adding {targetDir} to {targetKey}\\PATH...");
                newValue = curValue?.TrimEnd() ?? "";
                if (newValue != "" && !newValue.EndsWith(";"))
                    newValue += ";";
                newValue += targetDir;
            }

            try
            {
                environment.SetValue("PATH", newValue, RegistryValueKind.String);
                Info(() => "Done. Beware that the change might not be effective until next shell session.");
            }
            catch (Exception ex)
            {
                Error(() => $"Unable to change registry setting: {Registry.CurrentUser.Name}\\Environment:\r\n{ex.Message}");
                return ResultCodes.InvalidState;
            }

            return 0;

#else
            Error(() => $"Command not supported in this environment. In case you want to do it manually, I live on {targetDir}.");
            return ResultCodes.InvalidState;
#endif
        }

        private string GetMatchingPath(string[] pathExt, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;
            var parts = path.Split(';');
            foreach (var part in parts)
            {
                var result = part.Trim();
                if (result == string.Empty)
                    continue;

                try
                {
                    foreach (var ext in pathExt)
                    {
                        var file = new FileInfo(Path.Combine(result, "atgit" + ext));
                        if (file.Exists)
                            return result;
                    }
                }
                catch (Exception ex)
                {
                    Error(() => $"Unable to examine {result}: {ex.Message}");
                }
            }

            return null;
        }

#if NET452
        private string GetPathFromRegistry(RegistryKey root, string environmentPath)
        {
            Verbose(() => $"Opening {root.Name}\\{environmentPath}...");
            var environment = root.OpenSubKey(environmentPath);
            return environment?.GetValue("PATH") as string;
        }
#endif

        private string[] GetPathExt()
        {
            var pathExt = Environment.GetEnvironmentVariable("PATHEXT");
            Verbose(() => $"PATHEXT: {pathExt}");
            if (string.IsNullOrWhiteSpace(pathExt))
                pathExt = ".COM;.EXE;.BAT;.CMD;.VBS;.VBE;.JS;.JSE;.WSF;.WSH;.MSC";
            return pathExt.Split(';');
        }
    }
}