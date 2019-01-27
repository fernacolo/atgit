using System;

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
        
    }
}