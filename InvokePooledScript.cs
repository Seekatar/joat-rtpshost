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
        int _activityId = 54;
        TimeSpan _timeout = TimeSpan.FromMinutes(5);
        TimeSpan _pollingInterval = TimeSpan.FromSeconds(1);
        bool _testing = false;

        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0, HelpMessage = "Info about the script to run")]
        public PooledScript PooledScript;

        [Parameter(HelpMessage = "Max number of threads in the pool")]
        [ValidateRange(1,100)]
        public int MaxThreads;

        [Parameter(HelpMessage = "Timeout limit for any of the threads, defaults to 5 minutes")]
        [ValidateRange(5, Int16.MaxValue)]
        public int TimeoutSecs;

        [Parameter(HelpMessage = "Output the output of threads as they finish")]
        public SwitchParameter PassThru;

        [Parameter(HelpMessage = "Show the progress")]
        public SwitchParameter ShowProgress;

        [Parameter(HelpMessage = "Any additional modules to load")]
        public string[] ImportModules;

        [Parameter(Mandatory=true, HelpMessage = "Host you are running in.")]
        public System.Management.Automation.Host.PSHost Host;

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            _info = new List<PooledScript>();
            if ( MaxThreads >0 )
                _maxThreads = Math.Max(1, Math.Min( MaxThreads,1000));
            if ( TimeoutSecs > 0 )
                _timeout = TimeSpan.FromSeconds(Math.Max(1,Math.Min(TimeoutSecs,Int32.MaxValue)));
        }

        protected override void ProcessRecord()
        {
            _info.Add(PooledScript);
        }

        protected override void EndProcessing()
        {
            var initialSessionState = InitialSessionState.CreateDefault();

            if (ImportModules != null && ImportModules.Length > 0)
            {
                initialSessionState.ImportPSModule(ImportModules);
            }

            using (var _pool = RunspaceFactory.CreateRunspacePool(1, _maxThreads,initialSessionState,Host))
            {
                var pr = new ProgressRecord( _activityId, "Running pooled scripts", String.Format( "Starting {0} scripts...",_info.Count));
                _pool.Open();

                if ( ShowProgress && !_testing )
                {
                    pr.PercentComplete = 0;
                    WriteProgress(pr);
                }

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
                    foreach (var t in _info.Where(o => !o.Ended))
                    {
                        if (t.Result.IsCompleted)
                        {
                            if (!_testing) WriteVerbose("Completed " + t.NameAndSequence);
                            completed++;
                            t.Stopped();
                        }
                        else if (_timeout < DateTime.Now - t.Start) // timeout?
                        {
                            if (!_testing) WriteWarning("Timeout of " + t.NameAndSequence);
                            completed++;
                            t.Stopped(true);
                        }

                        if (ShowProgress && !_testing)
                        {
                            pr.PercentComplete = 100*completed / _info.Count;
                            pr.StatusDescription = String.Format( "Completed script: '{0}'", t.NameAndSequence);
                            WriteProgress(pr);
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
                if ( !_testing ) WriteProgress(pr);
            }
 
        }

        ConcurrentQueue<PSObject> _passThruOutput = new ConcurrentQueue<PSObject>();

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
