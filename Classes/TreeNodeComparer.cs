using System;
using System.Collections;
using System.Windows.Forms;
using System.Text;

namespace KCS.Common.Shared
{
	/// <summary>
	/// Compares TreeNodes.
	/// </summary>
	public class TreeNodeComparer : IComparer
	{
		public int Compare(object x, object y)
		{
			TreeNode tx = x as TreeNode;
			TreeNode ty = y as TreeNode;

			return string.Compare(tx.Text, ty.Text);
		}
	}
}
