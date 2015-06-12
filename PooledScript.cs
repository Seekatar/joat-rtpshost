﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;


namespace RtPsHost
{
    /// <summary>
    /// class for passing in and out pooled threads in PowerShell
    /// </summary>
    public class PooledScript
    {
        PSDataCollection<DebugRecord> _debug;
        PSDataCollection<VerboseRecord> _verbose;
        PSDataCollection<WarningRecord> _warning;
        PSDataCollection<ErrorRecord> _error;

        internal PowerShell Posh;

        internal void SetStreams(PSDataStreams ps)
        {
            Debug = ps.Debug;
            Verbose = ps.Verbose;
            Warning = ps.Warning;
            Error = ps.Error;
        }

        internal IAsyncResult Result { get; set; }

        private void _dataAdded<T>(object sender, DataAddedEventArgs e)
        {
            var rec = sender as PSDataCollection<T>;
            if (rec != null)
                LogList.Enqueue(new Tuple<DateTimeOffset, object, PooledScript>(DateTime.Now, rec[e.Index], this));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledScript" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="sb">The script block to run</param>
        public PooledScript(string name, ScriptBlock sb)
            : this(name, sb, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledScript" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="sb">The script block to run</param>
        /// <param name="parms">The parameters.</param>
        public PooledScript(string name, ScriptBlock sb, Hashtable parms) : this(name,sb,parms,null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledScript" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="sb">The script block to run</param>
        /// <param name="parms">The parameters.</param>
        /// <param name="list">The list for log messages from another PooledScript so they share the log.</param>
        /// <exception cref="System.Exception">List object not empty.  Cannot reuse list if not empty</exception>
        public PooledScript(string name, ScriptBlock sb, Hashtable parms, ConcurrentQueue<Tuple<DateTimeOffset, object, PooledScript>> list)
        {
            Name = name;
            ScriptBlock = sb;
            Parameters = parms;
            if (list != null)
            {
                if (list.Any())
                    throw new Exception("List object not empty.  Cannot reuse list if not empty");
                LogList = list;
            }
            else
                LogList = new ConcurrentQueue<Tuple<DateTimeOffset, object, PooledScript>>();
        }

        /// <summary>
        /// Gets the friendly name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the name and sequence as a string for logging
        /// </summary>
        public string NameAndSequence { get { return String.Format("{0} - {1}", Name, Sequence); } }

        /// <summary>
        /// Gets or sets the sequence.
        /// </summary>
        public int Sequence { get; internal set; }

        /// <summary>
        /// Gets the script block.
        /// </summary>
        /// <value>
        /// The script block.
        /// </value>
        public ScriptBlock ScriptBlock { get; private set; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public Hashtable Parameters { get; private set; }

        /// <summary>
        /// Gets the debug log after the thread has run.
        /// </summary>
        public PSDataCollection<DebugRecord> Debug { get { return _debug; } internal set { _debug = value; _debug.DataAdded += _dataAdded<DebugRecord>; } }

        /// <summary>
        /// Gets the verbose log after the thread has run.
        /// </summary>
        public PSDataCollection<VerboseRecord> Verbose { get { return _verbose; } internal set { _verbose = value; _verbose.DataAdded += _dataAdded<VerboseRecord>; } }

        /// <summary>
        /// Gets the warning log after the thread has run.
        /// </summary>
        public PSDataCollection<WarningRecord> Warning { get { return _warning; } internal set { _warning = value; _warning.DataAdded += _dataAdded<WarningRecord>; } }

        /// <summary>
        /// Gets the error log after the thread has run.
        /// </summary>
        public PSDataCollection<ErrorRecord> Error { get { return _error; } internal set { _error = value; _error.DataAdded += _dataAdded<ErrorRecord>; } }

        /// <summary>
        /// Gets the log list that holds the mix of log object in order they were logged
        /// </summary>
        /// Share with other PooledScripts to combine output
        public ConcurrentQueue<Tuple<DateTimeOffset, object, PooledScript>> LogList { get; private set; }

        /// <summary>
        /// Gets the output objects after the thread has run.
        /// </summary>
        public PSDataCollection<PSObject> Output { get; internal set; }

        /// <summary>
        /// Gets the start time.
        /// </summary>
        public DateTime Start { get; internal set; }

        /// <summary>
        /// Gets the stop.
        /// </summary>
        public DateTime Stop { get; internal set; }

        /// <summary>
        /// Gets the duration.
        /// </summary>
        public TimeSpan Duration { get { return Stop - Start;  } }

        /// <summary>
        /// Gets a value indicating whether [had errors].
        /// </summary>
        public bool HadErrors { get; internal set; }

        /// <summary>
        /// Gets the exception if there was one.
        /// </summary>
        public Exception Exception { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether the thread timed out.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [timed out]; otherwise, <c>false</c>.
        /// </value>
        public bool TimedOut { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PooledScript"/> is ended.
        /// </summary>
        internal bool Ended { get; set; }

        /// <summary>
        /// Gets all the log messages in order, combining debug, verbose, warning, and error
        /// </summary>
        /// <param name="includeTime">if set to <c>true</c> to include the timestamp as a prefix.</param>
        /// <param name="timeFormat">The time format.</param>
        /// <returns></returns>
        public IEnumerable<string> LogMessages(bool includeTime = true, bool includePooledScriptName = false, string timeFormat = "u" )
        {
            return LogList.Select(o =>
            {
                var prefix = string.Empty;
                if (includeTime)
                    prefix = o.Item1.ToString("u") + " ";
                if (includePooledScriptName)
                    prefix += o.Item3.NameAndSequence + " ";

                if (o.Item2 is DebugRecord)
                    return prefix + "DEBUG: " + o.Item2.ToString();
                else if (o.Item2 is VerboseRecord)
                    return prefix + "VERBOSE: " + o.Item2.ToString();
                else if (o.Item2 is WarningRecord)
                    return prefix + "WARNING: " + o.Item2.ToString();
                else if (o.Item2 is ErrorRecord)
                    return prefix + "ERROR: " + o.Item2.ToString();
                else
                    return String.Empty;
            });
        }

        /// <summary>
        /// Starting.
        /// </summary>
        /// <param name="posh">The posh.</param>
        /// <param name="sequence">The sequence.</param>
        internal void Starting(PowerShell posh, int sequence)
        {
            Posh = posh;
            SetStreams(posh.Streams);
            Start = DateTime.Now;
            Sequence = sequence;
        }

        /// <summary>
        /// Stopping
        /// </summary>
        /// <param name="timedOut">if set to <c>true</c> [timed out].</param>
        internal void Stopped(bool timedOut = false)
        {
            Stop = DateTime.Now;
            Ended = true;
            TimedOut = timedOut;
            if (timedOut)
            {
                Posh.Stop();
            }
            else
            {
                Ended = true;
                Stop = DateTime.Now;
                HadErrors = Posh.HadErrors;
                try
                {
                    Output = Posh.EndInvoke(Result);
                }
                catch (Exception e)
                {
                    HadErrors = true;
                    Exception = e;
                }

            }

            // if have syntax error Dispose clears the error collection, copy it
            if (Exception != null)
            {
                // add exception as error and to loglist, if it exists
                var exceptions = new ErrorRecord[1] { new ErrorRecord(this.Exception, "Script exception", ErrorCategory.NotSpecified, null) };
                // this closes collection, so have to add all in one shot
                _error = new PSDataCollection<ErrorRecord>(_error.Union(exceptions));
                LogList.Enqueue(new Tuple<DateTimeOffset, object, PooledScript>(DateTime.Now, exceptions[0], this));
            }
            else
            {
                _error = new PSDataCollection<ErrorRecord>(_error);
            }
            if ( Exception != null )
            {
            }
            Posh.Dispose();
            Posh = null;
            Result = null;
        }
    }

}