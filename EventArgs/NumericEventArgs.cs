using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Numeric event arg with TimeStamp.
    /// </summary>
    public class NumericEventArgs : TimeStampEventArgs
    {
        /// <summary>
        /// Contains the numeric value.
        /// </summary>
        public decimal Value { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">Value.</param>
        public NumericEventArgs(decimal value) : base(DateTime.Now, string.Empty)
        {
            this.Value = value;
        }

		/// <summary>
		/// Alternate constructor.
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="message">Message.</param>
        public NumericEventArgs(decimal value, string message) : base(DateTime.Now, message)
		{
            this.Value = value;
		}
    }
}
