using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using System.Text;

namespace KCS.Common.Shared
{
	/// <summary>
	/// Defines members for making sure a control can be used with the ExpandingPopup form, etc.
	/// </summary>
	public interface IListControl
	{
		bool IsItemSelected { get; }

		IList<T> GetItems<T>();

		void SetItems<T>(IList<T> items);

		void SelectAll();
		void SelectNone();
	}
}
