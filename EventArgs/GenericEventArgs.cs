using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
	/// <summary>
	/// This class is meant to be subclassed.
	/// </summary>
	public abstract class GenericEventArgs<T> : System.EventArgs
	{
		#region Properties
		/// <summary>
		/// The data (could be anything).
		/// </summary>
		protected T Data
		{
			get;
			set;
		}
		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="data">String data.</param>
		public GenericEventArgs(T data)
		{
			this.Data = data;
		}

        public GenericEventArgs()
        {
            this.Data = default(T);
        }
	}
}
