using System;

namespace RtPsHost
{
    /// <summary>
    /// class to wrap reporting progress in PowerShell
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
        /// <param name="activity">name used to identify the progress</param>
        /// <param name="status">status string to show</param>
        /// <param name="currentOperation">operation string to show</param>
        /// <param name="percentComplete">what percent complete it is.</param>
        /// <param name="id">id used to differentiate multiple instances of the progress bar</param>
        /// <param name="secondsRemaining">optional seconds remaining to show</param>
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

        /// <summary>
        /// implicit string conversion operator the return the activity string.\
        /// </summary>
        /// <param name="pi"></param>
        /// <returns></returns>
        public static implicit operator string(ProgressInfo pi) { return pi.Activity; }

        /// <summary>
        /// show the progress as a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0} {3,3}% {1} {2} ", Activity, CurrentOperation, Status, PercentComplete);
        }

    }

}
