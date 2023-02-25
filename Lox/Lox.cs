using System;
using System.Collections.Generic;
using System.IO;
using Lox.Lexer;

namespace Lox
{
    /// <summary>
    /// The entry class that run lox language
    /// </summary>
    public class Lox
    {
        /// <summary>
        /// It will be used to ensure that it will prevent executing code that has a known error.
        /// </summary>
        static bool HadError = false;

        static void Main(string[] args)
        {
            //In case of the user enter more than one argument.
            //We need only 1 arg that is refered to the path of lox script file
            if (args.Length > 1)
            {
                Console.WriteLine("Consider Correct Usage: lox [script].lox");
                Environment.Exit(64);
            }
            //The user enter the command to run lox code from a file
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            //User didn't write any args, that means the interactive mode will be activated.
            else
            {
                RunPrompt();
            }
        }

        /// <summary>
        /// The function takes file path to read and execute it by passing the file content to <see cref="Execute"/> function
        /// </summary>
        /// <param name="path">The file path</param>
        private static void RunFile(string path)
        {
            string sciptText = File.ReadAllText(path);
            Execute(sciptText);
            if (HadError) Environment.Exit(65);
        }

        /// <summary>
        /// Run scource code in interactive mode.
        /// </summary>
        private static void RunPrompt()
        {
            for (;;)
            {
                Console.Write("> ");
                string? line = Console.ReadLine();
                if (line == null) break;
                Execute(line);
                HadError= false;
            }

        }

        /// <summary>
        /// Run Source code whether it's coming from file or command line in interactive mode.
        /// </summary>
        /// <param name="sourceCode"></param>
        private static void Execute (string sourceCode)
        {
            Scanner scanner = new Scanner(sourceCode);
            List<Token> tokens = scanner.ScanTokens();

            foreach (Token token in tokens)
            {
                Console.WriteLine(token);
            }

        }

        /// <summary>
        /// Sending errors to Report function to tell user about the details of error occured
        /// </summary>
        /// <param name="line">The line number where error occured</param>
        /// <param name="message">Error Message</param>
        public static void Error(int line , string message)
        {
            Report(line, "", message);
        }

        /// <summary>
        /// The function reports to the user that some syntax error occured on a given line.
        /// </summary>
        /// <param name="line">The line number where error occured</param>
        /// <param name="where">The location of erro.</param>
        /// <param name="message">Error message</param>
        private static void Report(int line, string where, string message)
        {
            Console.WriteLine($"[Line {line}] Error {where}: {message}");
            HadError = true;
        }


    }
}