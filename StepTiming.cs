using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RtPsHost
{
    /// <summary>
    /// class for returning timing about steps executed in PS
    /// </summary>
    public class StepTiming
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepTiming"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="skipped">if set to <c>true</c> [skipped].</param>
        public StepTiming( string name, TimeSpan duration, bool skipped )
        {
            Name = name;
            Duration = duration;
            Skipped = skipped;
        }

        /// <summary>
        /// Gets the duration.
        /// </summary>
        /// <value>
        /// The duration.
        /// </value>
        public TimeSpan Duration { get; private set; }

        /// <summary>
        /// Gets or sets the name of the step.
        /// </summary>
        /// <value>
        /// The name of the step.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="StepTiming"/> is skipped.
        /// </summary>
        /// <value>
        ///   <c>true</c> if skipped; otherwise, <c>false</c>.
        /// </value>
        public bool Skipped { get; set; }
    }
}
