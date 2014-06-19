﻿using System;
using System.Threading.Tasks;
namespace RtPsHost
{
    /// <summary>
    /// Interface for a simplified hosting of PowerShell
    /// </summary>
    public interface IPsHost : IDisposable
    {
        /// <summary>
        /// initialize powershell
        /// </summary>
        /// <param name="console">an implementation of the console to handle output and user interaction</param>
        void Initialize(IPsConsole console);

        /// <summary>
        /// run the a series of scripts in the scriptFname, async
        /// </summary>
        /// <param name="scriptFname">file name of XML with PowerShell snippets.  See the doc of XSD for details</param>
        /// <param name="step">if true and snippet allows stepping, will prompt before running a setp</param>
        /// <param name="items">objects to push into PowerShell</param>
        /// <param name="type">type of scripts to read from the XML</param>
        /// <param name="skipUntil">if supplied, skips steps until one of this name is hit</param>
        /// <param name="quiet">if true doesn't show progress of scripts, or echo scripts if they are set to echo</param>
        /// <returns>ProcessingResult indicating how the script ended</returns>
        Task<ProcessingResult> InvokeAsync(string script, string name, System.Collections.Generic.IDictionary<string, object> items, bool dontPrompt = true, bool quiet = true);

        /// <summary>
        /// run a script, async
        /// </summary>
        /// <param name="script">PowerShell script to run</param>
        /// <param name="name">name shown if quiet is false</param>
        /// <param name="items">objects to push into PowerShell</param>
        /// <param name="quiet">if true doesn't show progress of scripts, or echo scripts if they are set to echo</param>
        /// <param name="dontPrompt">if true never prompts the user for anything.  Will error if no default values available.</param>
        /// <returns>ProcessingResult indicating how the script ended</returns>
        Task<ProcessingResult> InvokeAsync(string scriptFname, bool step, System.Collections.Generic.IDictionary<string, object> items, ScriptInfo.ScriptType type = ScriptInfo.ScriptType.normal, string skipUntil = null, bool quiet = false);

        /// <summary>
        /// cancel the currently running steps
        /// </summary>
        void Cancel();
    }
}