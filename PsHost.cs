﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;

namespace RtPsHost
{
    /// <summary>
    /// Simplified PowerShell host class to run a series of PowerShell commands in an XML file
    /// </summary>
    public class PsHost : IDisposable
    {
        private List<ScriptInfo> _commands = new List<ScriptInfo>();
        private Runspace _myRunSpace;
        private MyHost _myHost;
        private PowerShell currentPowerShell;
        private object instanceLock = new object();
        private string _scriptFileName = "scripts.xml";
        private bool _canceled = false;
        private IPsConsole _console;
        bool _initialized = false;

        /// <summary>
        /// initialize powershell
        /// </summary>
        /// <param name="console">an implementation of the console to handle output and user interaction</param>
        public void Initialize(IPsConsole console)
        {
            if (!_initialized)
            {
                _initialized = true;

                _console = console;

                // Create the host and runspace instances for this interpreter. 
                // Note that this application does not support console files so 
                // only the default snap-ins will be available.
                this._myHost = new MyHost(console);
                this._myRunSpace = RunspaceFactory.CreateRunspace(this._myHost);
                this._myRunSpace.Open();

                // Create a PowerShell object to run the commands used to create 
                // $profile and load the profiles.
                lock (this.instanceLock)
                {
                    this.currentPowerShell = PowerShell.Create();
                }
            }
        }

        /// <summary>
        /// run the a series of scripts in the scriptFname, async
        /// </summary>
        /// <param name="scriptFname">file name of XML with PowerShell snippets.  See the doc of XSD for details</param>
        /// <param name="step">if true and snippet allows stepping, will prompt before running a setp</param>
        /// <param name="items">objects to push into PowerShell</param>
        /// <param name="type">type of scripts to read from the XML</param>
        /// <param name="skipUntil">if supplied, skips steps until one of this name is hit</param>
        /// <returns>ProcessingResult indicating how the script ended</returns>
        public async Task<ProcessingResult> InvokeAsync(string scriptFname, bool step, IDictionary<string, object> items, ScriptInfo.ScriptType type = ScriptInfo.ScriptType.normal, string skipUntil = null )
        {
            _scriptFileName = scriptFname;

            _canceled = false;
            ProcessingResult ret = ProcessingResult.ok;

            _commands.LoadFromXmlFile(_scriptFileName);

            // hide transcript cmdlets
            hideTranscriptCmdlets(type);

            // insert any global variables they set
            setVariables(items,type);

            var commands = _commands.Where(o => o.Type == type).ToList( );
            _console.WriteLine(String.Format("Running {0} commands of type {1} loaded from \"{1}\"", commands.Count(), type, _scriptFileName),WriteType.System);

            var progress = new ProgressInfo("Running PowerShell commands",id:1999);
            var totalCommands = commands.Count;
            int i = 0;

            foreach (var c in commands)
            {
                progress.CurrentOperation = c.Name;
                progress.PercentComplete = 100 * i++ / totalCommands;
                _console.WriteProgress(progress);

                if (String.Equals(c.Name, skipUntil))
                    skipUntil = null;

                if (!c.NeverPrompt && !String.IsNullOrWhiteSpace(skipUntil))
                {
                    _console.WriteLine("Skipping until " + skipUntil, WriteType.System);
                }
                else
                {
                    if (step && !c.NeverPrompt)
                    {
                        var choices = new List<PromptChoice>()
                        {
                            new PromptChoice("&Yes"),
                            new PromptChoice("&No"),
                            new PromptChoice("&Stop Stepping"),
                            new PromptChoice("&Cancel script"),
                        };
                        int choice = _console.PromptForChoice("Do you want to execute " + c.Name, c.Description, choices, 0);

                        if (choice == 3)
                        {
                            _console.WriteLine("Canceled", WriteType.System); // cancel whole thing
                            ret = ProcessingResult.canceled;
                            break;
                        }
                        else if (choice == 2) // stop skipping
                            step = false;
                        else if (choice == 1) // no
                            continue;
                        // else execute it
                    }

                    if (!await executeHelperAsync(c, null)  && !_canceled)
                    {
                        ret = ProcessingResult.failed;
                        int choice = 1;
                        if (c.PromptOnError)
                        {
                            var choices = new List<PromptChoice>()
                            {
                                new PromptChoice("&Yes"),
                                new PromptChoice("&No"),
                            };
                            choice = _console.PromptForChoice(String.Format( "{0} errored. Do you want to continue?", c.Name), String.Empty, choices, 1);
                        }
                        if (choice == 1) // No
                            break;
                        else // Yes, continue
                            ret = ProcessingResult.ok;
                    }
                }

                if (_canceled)
                {
                    _console.WriteLine("Canceled",WriteType.System);
                    ret = ProcessingResult.canceled;
                    break;
                }

                if (_console.ShouldExit)
                {
                    _console.WriteLine(String.Format("Exit called from script with exit code of {0}", _console.ExitCode), WriteType.System);
                    ret = _console.ExitCode == 0 ? ProcessingResult.ok : ProcessingResult.failed;
                    break;
                }

                _console.WriteLine("",WriteType.Host);
            }

            _console.WriteLine("Processing complete. "+ret.ToString(),WriteType.System);

            progress.PercentComplete = 100;
            progress.Success = ret == ProcessingResult.ok;
            _console.WriteProgress(progress);

            return ret;
        }

        /// <summary>
        /// calling start/stop transcript gives unupported error and I can't find out how to support them, so for now
        /// do this just to hide them
        /// </summary>
        private void hideTranscriptCmdlets(ScriptInfo.ScriptType type )
        {
            StringBuilder set = new StringBuilder();
            set.AppendLine("function Start-Transcript() { \"Start-Transcript ignored with args of $args\" }");
            set.AppendLine("function Stop-Transcript() { \"Stop-Transcript ignored with args of $args\" }");
            _commands.Insert(0, new ScriptInfo() { Script = set.ToString(), Name = "Hide Start/Stop-Transcript", NeverPrompt = true,  EchoScript = false, Type = type });
        }

        /// <summary>
        /// set variables into the PowerShell runspace
        /// </summary>
        /// <param name="variables"></param>
        /// <param name="type"></param>
        private void setVariables(IDictionary<string, object> variables, ScriptInfo.ScriptType type )
        {
            if (variables != null)
            {
                StringBuilder set = new StringBuilder();
                foreach (var s in variables)
                {
                    var setPS = PowerShell.Create(); // create new PS each time otherwise keeps adding to it
                    setPS.Runspace = this._myRunSpace;
                    setPS.AddScript(String.Format("function _setGlobal( $a ) {{ $global:{0} = $a; Write-Verbose \"{0} is $($global:{0})\"}}", s.Key));
                    setPS.Invoke();
                    setPS.AddCommand("_setGlobal");
                    setPS.AddArgument(s.Value);
                    setPS.Invoke();

                }
                if (set.Length > 0)
                {
                    _commands.Insert(0, new ScriptInfo() { Script = set.ToString(), Name = "Set Globals", NeverPrompt = true, EchoScript = false, Type = type });
                }
            }
        }

        /// <summary>
        /// cancel the currently running steps
        /// </summary>
        public void Cancel()
        {
            currentPowerShell.Stop(); // TODO use begin stop
            _canceled = true;
        }

        #region PowerShell helpers

        /// <summary>
        /// executes a PS command async
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        private Task<bool> executeHelperAsync(ScriptInfo cmd, object input)
        {
            return Task.Run(() =>
            {
                try
                {
                    return executeHelper(cmd, input);
                }
                catch (RuntimeException rte)
                {
                    this.reportException(rte);
                    return false;
                }
            });
        }

        private int getLastExitCode()
        {
            int exitCode = 0;
            Collection<PSObject> ret;
            lock (this.instanceLock)
            {
                this.currentPowerShell = PowerShell.Create();
            }
            this.currentPowerShell.Runspace = this._myRunSpace;
            this.currentPowerShell.AddScript("$LASTEXITCODE;$Error");
            ret = this.currentPowerShell.Invoke();
            foreach (var o in ret)
            {
                if (o != null && o.BaseObject is int)
                    exitCode = (int)o.BaseObject;
            }
            return exitCode;
        }

        /// <summary>
        /// A helper class that builds and executes a pipeline that writes 
        /// to the default output path. Any exceptions that are thrown are 
        /// just passed to the caller. Since all output goes to the default 
        /// outter, this method does not return anything.
        /// </summary>
        /// <param name="cmd">The script to run.</param>
        /// <param name="input">Any input arguments to pass to the script. 
        /// If null then nothing is passed in.</param>
        private bool executeHelper(ScriptInfo cmd, object input)
        {
            // Ignore empty command lines.
            if (String.IsNullOrEmpty(cmd.Script))
            {
                return true;
            }

            if (cmd.EchoScript)
                _console.WriteLine( "Executing: " + cmd.Script,WriteType.System);
            else
                _console.WriteLine("Executing script named: " + cmd.Name, WriteType.System);

            // Create the pipeline object and make it available to the
            // ctrl-C handle through the currentPowerShell instance
            // variable.
            lock (this.instanceLock)
            {
                this.currentPowerShell = PowerShell.Create();
            }

            // Add a script and command to the pipeline and then run the pipeline. Place 
            // the results in the currentPowerShell variable so that the pipeline can be 
            // stopped.
            try
            {
                this.currentPowerShell.Runspace = this._myRunSpace;

                this.currentPowerShell.AddScript("$LASTEXITCODE=0");
                this.currentPowerShell.AddScript(cmd.Script);

                // Add the default outputter to the end of the pipe and then call the 
                // MergeMyResults method to merge the output and error streams from the 
                // pipeline. This will result in the output being written using the PSHost
                // and PSHostUserInterface classes instead of returning objects to the host
                // application.
                this.currentPowerShell.AddCommand("out-default");
                this.currentPowerShell.Commands.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);

                // If there is any input pass it in, otherwise just invoke the
                // the pipeline.
                if (input != null)
                {
                    this.currentPowerShell.Invoke(new object[] { input });
                }
                else
                {
                    this.currentPowerShell.Invoke();
                }
                if (this.currentPowerShell.Streams.Error.Count() > 0)
                {
                    var errs = new StringBuilder();
                    foreach (var e in currentPowerShell.Streams.Error)
                    {
                        errs.AppendLine(e.ToString());
                    }
                    throw new RuntimeException("Error executing script: " + errs.ToString());
                }
                int exitCode = getLastExitCode();
                if (exitCode != 0)
                {
                    throw new RuntimeException("Last exit code was " + exitCode);
                }
            }
            finally
            {
                // Dispose the PowerShell object and set currentPowerShell to null. 
                // It is locked because currentPowerShell may be accessed by the 
                // ctrl-C handler.
                lock (this.instanceLock)
                {
                    this.currentPowerShell.Dispose();
                    this.currentPowerShell = null;
                }
            }
            return true;
        }


        /// <summary>
        /// To display an exception using the display formatter, 
        /// run a second pipeline passing in the error record.
        /// The runtime will bind this to the $input variable,
        /// which is why $input is being piped to the Out-String
        /// cmdlet. The WriteErrorLine method is called to make sure 
        /// the error gets displayed in the correct error color.
        /// </summary>
        /// <param name="e">The exception to display.</param>
        private void reportException(Exception e)
        {
            if (e != null)
            {
                object error;
                IContainsErrorRecord icer = e as IContainsErrorRecord;
                if (icer != null)
                {
                    error = icer.ErrorRecord;
                }
                else
                {
                    error = (object)new ErrorRecord(e, "Host.ReportException", ErrorCategory.NotSpecified, null);
                }

                lock (this.instanceLock)
                {
                    this.currentPowerShell = PowerShell.Create();
                }

                this.currentPowerShell.Runspace = this._myRunSpace;

                try
                {
                    this.currentPowerShell.AddScript("$input").AddCommand("out-string");

                    // Do not merge errors, this function will swallow errors.
                    Collection<PSObject> result;
                    PSDataCollection<object> inputCollection = new PSDataCollection<object>();
                    inputCollection.Add(error);
                    inputCollection.Complete();
                    result = this.currentPowerShell.Invoke(inputCollection);

                    if (result.Count > 0)
                    {
                        string str = "Exception: ";
                        str += result[0].BaseObject as string;
                        if (!string.IsNullOrEmpty(str))
                        {
                            // Remove \r\n, which is added by the Out-String cmdlet.
                            this._myHost.UI.WriteErrorLine(str.Substring(0, str.Length - 2));
                        }
                    }
                }
                finally
                {
                    // Dispose of the pipeline and set it to null, locking it  because 
                    // currentPowerShell may be accessed by the ctrl-C handler.
                    lock (this.instanceLock)
                    {
                        this.currentPowerShell.Dispose();
                        this.currentPowerShell = null;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// cleanup 
        /// </summary>
        public void Dispose()
        {
            if (_myHost != null)
            {
                this._myHost.Dispose();
            }
       
            if (currentPowerShell != null)
            {
                this.currentPowerShell.Stop();
                this.currentPowerShell.Dispose();
                this.currentPowerShell = null;
            }
            if (_myRunSpace != null)
            {
                this._myRunSpace.Close();
                this._myRunSpace.Dispose();
                this._myRunSpace = null;
            }
          
            if (_commands != null)
            {
                _commands.Clear();
                _commands = null;
            }

            _console = null;
  
         
     
        }
    }
}
