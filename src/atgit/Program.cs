using System;

namespace atgit
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var options = new Options(args);
                switch (options.Action)
                {
                    case OptionsAction.ShowHelp:
                        ShowErrorIfNeeded(options.ErrorMessage);
                        Options.ShowHelp();
                        return options.ResultCode;

                    case OptionsAction.AddToPath:
                        var registerer = new AddToPath(options);
                        return registerer.Execute();

                    case OptionsAction.RunCommand:
                        var runner = new Runner(options);
                        return runner.Execute();

                    default:
                        throw new InvalidOperationException($"Unknown action: {options.Action}");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
                Console.ResetColor();
                return ResultCodes.InternalError;
            }
        }

        private static void ShowErrorIfNeeded(string errorMessage)
        {
            if (errorMessage == null)
                return;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errorMessage);
            Console.ResetColor();
        }
    }
}