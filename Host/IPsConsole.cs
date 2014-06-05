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
    /// Implement this interface in your application to handle output and user interaction
    public interface IPsConsole
    {
        /// <summary>
        /// set to true if a script calls exit
        /// </summary>
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
        /// write a message in the current colors, without a newline
        /// </summary>
        /// <param name="msg">the message type write</param>
        /// <param name="type">type of message being written, output, verbose, etc.</param>
        void Write(string msg, WriteType type);

        /// <summary>
        /// write a line in the current colors
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="msg">the message type write</param>
        /// <param name="type">type of message being written, output, verbose, etc.</param>
        void WriteLine(string msg, WriteType type);

        /// <summary>
        /// write a message using specified colors, without a new line
        /// </summary>
        /// <param name="foreground">the foreground color to use</param>
        /// <param name="background">the background color to use</param>
        /// <param name="msg">the message type write</param>
        /// <param name="type">type of message being written, output, verbose, etc.</param>
        void Write(ConsoleColor foreground, ConsoleColor background, string msg, WriteType type);

        /// <summary>
        /// write a line using specified colors
        /// </summary>
        /// <param name="foreground">the foreground color to use</param>
        /// <param name="background">the background color to use</param>
        /// <param name="msg">the message type write</param>
        /// <param name="type">type of message being written, output, verbose, etc.</param>
        void WriteLine(ConsoleColor foreground, ConsoleColor background, string msg, WriteType type);

        /// <summary>
        /// prompt the user for a single choice
        /// </summary>
        /// <param name="caption">caption to display above the choices</param>
        /// <param name="message">more detailed message about the choices</param>
        /// <param name="choices">the choices</param>
        /// <param name="defaultChoice">the zero-based default choice, use -1 for no default</param>
        /// <returns>the zero-based choice the user picked</returns>
        int PromptForChoice(string caption, string message, IEnumerable<PromptChoice> choices, int defaultChoice );

        /// <summary>
        /// Write the progress to whoemver is listening for it.
        /// </summary>
        /// <param name="progressInfo">the details about the progress</param>
        void WriteProgress(ProgressInfo progressInfo);

        /// <summary>
        /// prompt for a string
        /// </summary>
        /// <param name="caption">Text that preceeds the prompt (a title). usually empty</param>
        /// <param name="message">Text of the prompt. usually empty </param>
        /// <param name="descriptions">description</param>
        /// <returns>a string they entered</returns>
        string PromptForString(string caption, string message, string descriptions);
    
        /// <summary>
        /// prompt for a password string
        /// </summary>
        /// <param name="caption">Text that preceeds the prompt (a title). usually empty</param>
        /// <param name="message">Text of the prompt. usually empty </param>
        /// <param name="descriptions">description</param>
        /// <returns>a string they entered</returns>
        System.Security.SecureString PromptForSecureString(string caption, string message, string description);
    }
}
