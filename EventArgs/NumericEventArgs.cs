using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Indicates success or failure.
    /// </summary>
    public class NumericEventArgs : TimeStampEventArgs
    {
        /// <summary>
        /// Contains the numeric value.
        /// </summary>
        public decimal Value { get; private set; }

        public NumericEventArgs(decimal value) : this(value, "")
        {
        }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="success">Success value.</param>
        public NumericEventArgs(decimal value, string message) : base(DateTime.Now, message)
		{
            this.Value = value;
			base.Data = message;
		}
    }
}
