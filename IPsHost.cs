using System;
using System.Collections.Generic;
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
        /// <param name="scriptFname">The script fname.</param>
        /// <param name="step">if set to <c>true</c> [step].</param>
        /// <param name="items">objects to push into PowerShell</param>
        /// <param name="type">The type.</param>
        /// <param name="skipUntil">The skip until.</param>
        /// <param name="quiet">if true doesn't show progress of scripts, or echo scripts if they are set to echo</param>
        /// <param name="timings">Optional collection for the timings.</param>
        /// <returns>
        /// ProcessingResult indicating how the script ended
        /// </returns>
        Task<ProcessingResult> InvokeAsync(string scriptFname, bool step, System.Collections.Generic.IDictionary<string, object> items, ScriptInfo.ScriptType type = ScriptInfo.ScriptType.normal, string skipUntil = null, bool quiet = false, IList<StepTiming> timings = null);

        /// <summary>
        /// cancel the currently running steps
        /// </summary>
        void Cancel();

        /// <summary>
        /// Invokes the script asynchronously, letting caller process output.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="script">The script.</param>
        /// <param name="outputProcessor">The output processor to process objects in the pipeline</param>
        /// <param name="parms">The parameters to pass to the script.</param>
        /// <returns></returns>
        Task InvokeScriptAsync<T>( string script, Action<T> outputProcessor, System.Collections.Generic.IDictionary<string, object> parms );
    }
}
