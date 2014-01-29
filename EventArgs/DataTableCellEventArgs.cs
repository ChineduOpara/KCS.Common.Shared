using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
	/// <summary>
	/// Contains the location of a cell in a DataTable.
	/// </summary>
	public class DataTableCellEventArgs : Shared.GenericEventArgs<object>
	{
		/// <summary>
		/// Row index;
		/// </summary>
		public int RowIndex
		{
			get;
			private set;
		}

		/// <summary>
		/// Column index;
		/// </summary>
		public int ColumnIndex
		{
			get;
			private set;
		}

		/// <summary>
		/// Column name.
		/// </summary>
		public string ColumnName
		{
			get;
			private set;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="rowIndex">Row Index.</param>
		/// <param name="columnIndex">Column index.</param>
		/// <param name="columnName">Column name.</param>
		public DataTableCellEventArgs(int rowIndex, int columnIndex, string columnName) : base(null)
		{
			this.RowIndex = rowIndex;
			this.ColumnIndex = ColumnIndex;
			this.ColumnName = columnName;
		}
	}
}
