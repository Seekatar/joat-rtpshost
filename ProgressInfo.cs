using System;

namespace RtPsHost
{
    /// <summary>
    /// class to wrap reporting progress in PS
    /// </summary>
    /// This has members that mirror the parameters for Write-Version
    public class ProgressInfo
    {
        /// <summary>
        /// non-PS parameter to indicate success or failure
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// goes above the progress bar
        /// </summary>
        public string Activity { get; set; }

        /// <summary>
        /// optional goes under activity, above progress bar
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// optional goes below the progress bar
        /// </summary>
        public string TimeRemaining { get; set; }

        /// <summary>
        /// goes below timermaining and the progress bar
        /// </summary>
        public string CurrentOperation { get; set; }

        /// <summary>
        /// controls progress bar
        /// </summary>
        public int PercentComplete { get; set; }

        /// <summary>
        /// id used to differentiate between multiple progresses
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///  constructor with default parameters for all but activity
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="status"></param>
        /// <param name="currentOperation"></param>
        /// <param name="percentComplete"></param>
        /// <param name="id"></param>
        /// <param name="secondsRemaining"></param>
        public ProgressInfo(string activity, string status = null, string currentOperation = null, int percentComplete = 0, int id = 0, int secondsRemaining = 0)
        {
            Success = true;
            Activity = activity;
            Status = status;
            CurrentOperation = currentOperation;
            PercentComplete = percentComplete;
            Id = id;
            if (secondsRemaining > 0)
            {
                var ts = TimeSpan.FromSeconds(secondsRemaining);
                TimeRemaining = String.Format("{0:d2}:{1:d2}:{2:d2}", ts.Hours, ts.Minutes, ts.Seconds);
            }
        }

        public static implicit operator string(ProgressInfo pi) { return pi.Activity; }

        public override string ToString()
        {
            return String.Format("{0} {3,3}% {1} {2} ", Activity, CurrentOperation, Status, PercentComplete);
        }

    }

}
