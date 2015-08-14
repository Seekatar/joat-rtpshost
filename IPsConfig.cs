using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RtPsHost
{
    /// <summary>
    /// PowerShell configuration
    /// </summary>
    public interface IPsConfig
    {
        /// <summary>
        /// Run in quiet mode
        /// </summary>
        bool Quiet { get; set; }

        /// <summary>
        /// Don't prompt (not interactive)
        /// </summary>
        bool NoPrompt { get; set; }

        /// <summary>
        /// Should we run any PowerShell
        /// </summary>
        bool Run { get; set; }

        /// <summary>
        /// Should it step when running a script file
        /// </summary>
        bool Step { get; set; }

        /// <summary>
        /// If true, does everything but run the script, used for testing script sets, or transforms
        /// </summary>
        bool Test { get; set; }

        /// <summary>
        /// Use this script file
        /// </summary>
        string ScriptFile { get; set; }

        /// <summary>
        /// Optional step to skip until
        /// </summary>
        string SkipUntil { get; set; }

        /// <summary>
        /// Use this script set (null if use default)
        /// </summary>
        string ScriptSet { get; set; }

        /// <summary>
        /// Name of log file 
        /// </summary>
        string LogFileName { get; set; }

        /// <summary>
        /// Working directory to use
        /// </summary>
        string WorkingDir { get; set; }

        /// <summary>
        /// Gets the name of the log file base on the type of scripts to run
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        string GetLogFileName(ScriptInfo.ScriptType type);

        /// <summary>
        /// Appends the exports to the dict for PowerShell.
        /// </summary>
        /// <param name="dict">The dictionary.</param>
        void AppendExports(Dictionary<string, object> dict);

        /// <summary>
        /// Gets the script sets in the script file, if any
        /// </summary>
        /// <returns>list of script set names and descriptions</returns>
        IDictionary<string, string> GetScriptSets();

        /// <summary>
        /// Gets all steps.
        /// </summary>
        /// <returns>list of step names and descriptions</returns>
        IDictionary<string, string> GetAllSteps();

        /// <summary>
        /// Determines whether step is in the specified script set.
        /// </summary>
        /// <param name="scriptSet">The script set.</param>
        /// <param name="stepId">The step identifier.</param>
        /// <returns></returns>
        bool IsStepInScriptSet(string scriptSet, string stepId);

    }
}
