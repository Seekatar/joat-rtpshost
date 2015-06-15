using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace RtPsHost
{

    /// <summary>
    /// Invoke one or more script in a thread pool
    /// </summary>
    [Cmdlet("Invoke", "PooledScript")]
    public class InvokeAsync : Cmdlet
    {
        IList<PooledScript> _info;
        int _maxThreads = 10;
        int _activityId = 54; // for progress
        TimeSpan _timeout = TimeSpan.FromMinutes(5);
        TimeSpan _pollingInterval = TimeSpan.FromSeconds(1);
        bool _testing = false;
        DateTime _startTime;

        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0, HelpMessage = "Info about the script to run")]
        public PooledScript PooledScript;

        [Parameter(HelpMessage = "Max number of threads in the pool.  Defaults to 10")]
        [ValidateRange(1,100)]
        public int MaxThreads = 10;

        [Parameter(HelpMessage = "Timeout limit for any of the threads, defaults to 0 (no timeout)")]
        [ValidateRange(0, int.MaxValue)]
        public int TimeoutMin = 0;

        [Parameter(HelpMessage = "Output the output of threads as they finish")]
        public SwitchParameter PassThru;

        [Parameter(HelpMessage = "Show the progress")]
        public SwitchParameter ShowProgress;

        [Parameter(HelpMessage = "Any additional modules to load")]
        public string[] ImportModules;

        [Parameter(Mandatory=true, HelpMessage = "Host you are running in.")]
        public System.Management.Automation.Host.PSHost Host;

        /// <summary>
        /// Begins the processing.
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            _info = new List<PooledScript>();
            if ( MaxThreads > 0 )
                _maxThreads = Math.Max(1, Math.Min( MaxThreads,1000));
            if ( TimeoutMin > 0 )
                _timeout = TimeSpan.FromMinutes(Math.Max(1,Math.Min(TimeoutMin,Int32.MaxValue)));
            else
                _timeout = TimeSpan.FromMinutes(Int32.MaxValue);
        }

        /// <summary>
        /// Processes the record.
        /// </summary>
        protected override void ProcessRecord()
        {
            _info.Add(PooledScript);
        }

        /// <summary>
        /// Ends the processing.
        /// </summary>
        protected override void EndProcessing()
        {
            var initialSessionState = InitialSessionState.CreateDefault();

            if (ImportModules != null && ImportModules.Length > 0)
            {
                initialSessionState.ImportPSModule(ImportModules);
            }

            Console.TreatControlCAsInput = true;

            using (var _pool = RunspaceFactory.CreateRunspacePool(1, _maxThreads,initialSessionState,Host))
            {
                var pr = new ProgressRecord( _activityId, "Running pooled scripts", String.Format( "Starting {0} scripts...",_info.Count));
                _pool.Open();

                if ( ShowProgress && !_testing )
                {
                    pr.PercentComplete = 0;
                    WriteProgress(pr);
                }

                _startTime = DateTime.Now;

                int sequence = 0;
                foreach (var i in _info)
                {
                    var posh = PowerShell.Create().AddScript(i.ScriptBlock.ToString());
                    if (i.Parameters != null)
                        posh.AddParameters(i.Parameters);
                    posh.RunspacePool = _pool;

                    i.Starting(posh,sequence++);

                    if (PassThru)
                        i.Output.DataAdded += Output_DataAdded;

                    if ( !_testing ) WriteVerbose("Starting thread " + i.NameAndSequence);

                    i.Result = i.Posh.BeginInvoke(new PSDataCollection<PSObject>(), i.Output);
                }

                // wait for them to end
                int completed = 0;
                while (completed < _info.Count)
                {
                    if ( checkForBreak() )
                        break;

                    foreach (var t in _info.Where(o => !o.Ended))
                    {
                        if (t.Result.IsCompleted)
                        {
                            completed += completeScript(pr, t, false, completed );
                        }
                        else if (_timeout < DateTime.Now - _startTime) // timeout?
                        {
                            completed += completeScript(pr, t, true, completed);
                        }

                    }
                    PSObject output;
                    while (_passThruOutput.TryDequeue(out output))
                    {
                        WriteObject(output);
                    }
                    Thread.Sleep(_pollingInterval);
                }

                _pool.Close();

                pr.PercentComplete = 100;
                pr.RecordType = ProgressRecordType.Completed;
                if ( ShowProgress && !_testing ) WriteProgress(pr);
            }
 
        }

        /// <summary>
        /// Checks for break (abort the threads)
        /// </summary>
        /// <returns></returns>
        private bool checkForBreak()
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if ((key.Modifiers & ConsoleModifiers.Control) != 0 && key.Key == ConsoleKey.C)
                {
                    WriteWarning("User cancelled");
                    foreach( var t in _info.Where( o => !o.Ended ))
                    {
                        t.Posh.Stop();
                    }
                    return true;
                }
                else if ( (key.Modifiers & (ConsoleModifiers.Alt | ConsoleModifiers.Control)) == 0 )
                {
                    switch ( key.Key )
                    {
                        case ConsoleKey.Q:
                            // show any pending threads that have been started
                            Host.UI.WriteWarningLine(String.Format("Running or pending scripts ({1:f2} secs since started):",(DateTime.Now - _startTime).TotalSeconds));
                            foreach ( var t in _info.Where(o => !o.Ended))
                            {
                                Host.UI.WriteWarningLine(t.NameAndSequence);
                            }
                            break;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// called when the script has completed
        /// </summary>
        /// <param name="pr">The progress info.</param>
        /// <param name="t">The pooled script that's ending.</param>
        /// <param name="timedOut">if set to <c>true</c> [timed out].</param>
        /// <param name="completed">The current completed count.</param>
        /// <returns>1</returns>
        private int completeScript(ProgressRecord pr, RtPsHost.PooledScript t, bool timedOut, int completed )
        {
            if (!_testing)
                WriteVerbose((timedOut ? "Time out for " : "Completed ") + t.NameAndSequence);

            try
            {
                t.Stopped(timedOut);
            }
            catch ( PipelineStoppedException e )
            {
                WriteError(new ErrorRecord(e, "Pipeline stopped", ErrorCategory.NotSpecified, null));
            }

            if (ShowProgress && !_testing)
            {
                pr.PercentComplete = 100 * completed / _info.Count;
                pr.StatusDescription = String.Format("Completed {0}/{1}", completed+1, _info.Count);
                pr.CurrentOperation = String.Format("Last completed script: '{0}'", t.NameAndSequence);
                WriteProgress(pr);
            }
            return 1;
        }

        ConcurrentQueue<PSObject> _passThruOutput = new ConcurrentQueue<PSObject>();

        /// <summary>
        /// Handles the DataAdded event of the Output control.
        /// </summary>
        /// called when any threads send output.  If passthru set, it will immediately write it out
        /// This is called on background thread so we can do WriteObject here.
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataAddedEventArgs"/> instance containing the event data.</param>
        void Output_DataAdded(object sender, DataAddedEventArgs e)
        {
            var rec = sender as PSDataCollection<PSObject>;
            if (rec != null)
                _passThruOutput.Enqueue( rec[e.Index] );
            
        }

        public void Test(IEnumerable<PooledScript> info)
        {
            _testing = true;
            BeginProcessing();
            foreach (var i in info)
            {
                PooledScript = i;
                ProcessRecord();
            }
            EndProcessing();
        }
    }

}
