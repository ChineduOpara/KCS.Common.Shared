using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

namespace KCS.Common.Shared
{
	/// <summary>
	/// Contains information about an operation which contains a list of items.
	/// </summary>
	public class ListEventArgs<T> : System.EventArgs, IList<T>
	{
		private List<T> _items;

		/// <summary>
		/// Constructor.
		/// </summary>
		public ListEventArgs()
		{
			_items = new List<T>(10);
		}

		public ListEventArgs(IEnumerable<T> items)
		{
			_items = new List<T>(items);
		}

		#region IList<string> Members

		public int IndexOf(T item)
		{
			return _items.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			_items.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			_items.RemoveAt(index);
		}

		public T this[int index]
		{
			get
			{
				return _items[index];
			}
			set
			{
				_items[index] = value;
			}
		}

		#endregion


		public void Add(T item)
		{
			_items.Add(item);
		}

		public void Clear()
		{
			_items.Clear();
		}

		public bool Contains(T item)
		{
			return _items.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_items.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return _items.Count; }
		}

		public bool IsReadOnly
		{
			get { return true; }
		}

		public bool Remove(T item)
		{
			return _items.Remove(item);
		}


		#region IEnumerable<string> Members

		public IEnumerator<T> GetEnumerator()
		{
			return _items.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _items.GetEnumerator();
		}

		#endregion
	}
}
