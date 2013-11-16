using System;
using System.Collections.Generic;

namespace RtPsHost
{
    /// <summary>
    /// enumeration of types of messages to write
    /// </summary>
    public enum WriteType
    {
        // PowerShell types
        Output,
        Host,
        Verbose,
        Debug,
        Warning,
        Error,
        // from the host
        System
    }

    /// <summary>
    /// Simplified interface to a PSHost
    /// </summary>
    public interface IPsConsole
    {
        /// <summary>
        /// set to true if a script calls exit
        /// </summary>
        /// 
        bool ShouldExit { get; set; }

        /// <summary>
        /// the exit code set on a script exit
        /// </summary>
        int ExitCode { get; set; }

        /// <summary>
        /// ForegroundColor to get/set via script
        /// </summary>
        ConsoleColor ForegroundColor { get; set; }

        /// <summary>
        /// BackgroundColor to get/set via script
        /// </summary>
        ConsoleColor BackgroundColor { get; set; }

        /// <summary>
        /// get the width of the window in characters
        /// </summary>
        int WindowWidth { get; }

        /// <summary>
        /// write a message in the current colors
        /// </summary>
        /// <param name="msg"></param>
        void Write(string msg, WriteType type);

        /// <summary>
        /// write a line in the current colors
        /// </summary>
        /// <param name="msg"></param>
        void WriteLine(string msg, WriteType type);

        /// <summary>
        /// write a message using specified colors
        /// </summary>
        /// <param name="msg"></param>
        void Write(ConsoleColor foreground, ConsoleColor background, string msg, WriteType type);

        /// <summary>
        /// write a line using specified colors
        /// </summary>
        /// <param name="msg"></param>
        void WriteLine(ConsoleColor foreground, ConsoleColor background, string msg, WriteType type);

        /// <summary>
        /// prompt the user for a single choice
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="message"></param>
        /// <param name="choices"></param>
        /// <param name="defaultChoice"></param>
        /// <returns></returns>
        int PromptForChoice(string caption, string message, IEnumerable<PromptChoice> choices, int defaultChoice);

        /// <summary>
        /// Write the progress to whoever is listening for it.
        /// </summary>
        /// <param name="progressInfo"></param>
        void WriteProgress(ProgressInfo progressInfo);
    }
}
