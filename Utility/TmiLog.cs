using System;
using System.IO;
using System.Text;

namespace libtoxtmi.Utility
{
    /// <summary>
    /// Internal logging library for libtoxtmi.
    /// </summary>
    internal static class TmiLog
    {
        /// <summary>
        /// Log a message to stdout.
        /// </summary>
        public static void Log(params object[] args)
        {
            Log(Console.Out, args);
        }

        /// <summary>
        /// Log a message to stderr.
        /// </summary>
        public static void Error(params object[] args)
        {
            Log(Console.Error, args);
        }

        /// <summary>
        /// Log a message to debug / diagnostics output only.
        /// </summary>
        public static void Debug(params object[] args)
        {
            Log(null, args);
        }

        /// <summary>
        /// Internal helper method for writing a log message to an otuput stream.
        /// </summary>
        private static void Log(TextWriter outputStream, params object[] args)
        {
            if (args.Length == 0)
                return;

            var logFormat = new StringBuilder();
            logFormat.Append($"[{DateTime.UtcNow.ToLongTimeString()}]");

            foreach (var arg in args)
            {
                logFormat.Append(" ");

                try
                {
                    logFormat.Append(arg.ToString());
                }
                catch (Exception)
                {
                    logFormat.Append($"[{arg.GetType().ToString()}]");
                }
            }

            var finalLogMessage = logFormat.ToString();

            if (outputStream != null)
                outputStream.WriteLine(finalLogMessage);

            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debug.WriteLine(finalLogMessage);
        }
    }
}