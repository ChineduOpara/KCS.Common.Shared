using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Represents a single parameter.
    /// </summary>
    public class NameValuePair
    {
        /// <summary>
        /// Parameter name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Parameter value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        public NameValuePair(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Contains the string representation of the parameter.
        /// </summary>
        /// <returns>[Name] = [Value]</returns>
        public override string ToString()
        {
            return string.Format("{0}={1}", Name, Value);
        }
    }
}
