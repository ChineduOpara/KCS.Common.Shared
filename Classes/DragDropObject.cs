using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Represents data being drag-and-dropped. This class includes the source control, as well as other useful methods.
    /// </summary>
    public class DragDropObject<T>
    {
        /// <summary>
        /// Contains a reference to the source control.
        /// </summary>
        public Control Source { get; private set; }

        /// <summary>
        /// Contains a reference to the data being dragged.
        /// </summary>
        public T Data { get; private set; }

        /// <summary>
        /// Private constructor.
        /// </summary>
        private DragDropObject()
        {
        }

        /// <summary>
        /// Alternate private constructor.
        /// </summary>
        /// <param name="source">Source control.</param>
        /// <param name="data">Data being drag-dropped.</param>
        private void Initialize(Control source, T data)
        {
            if (source == null || data == null)
            {
                throw new System.Exception("Neither Source nor Data parameters can be null.");
            }

            Source = source;
            Data = data;
        }

        /// <summary>
        /// Public constructor, for use with ListViews.
        /// </summary>
        /// <param name="source">Source control.</param>
        /// <param name="data">SelectedListViewItemCollection being drag-dropped.</param>
        public DragDropObject(Control source, T data)
        {
            Initialize(source, data);
        }

		/// <summary>
		/// Checks to see if the Data being drag-dropped is of a particular type.
		/// </summary>
		/// <param name="type">Type for which to check.</param>
		/// <returns>TRUE if the embedded Data is of a particular type.</returns>
		public bool GetDataPresent(Type type)
		{
			return Data != null && Data.GetType().Equals(type);
		}

		/// <summary>
		/// Returns TRUE if the Data is any of the given types.
		/// </summary>
		/// <param name="types"></param>
		/// <returns></returns>
		public bool GetDataPresent(IEnumerable<Type> types)
		{
			foreach (Type type in types)
			{
				if (GetDataPresent(type))
				{
					return true;
				}
			}
			return false;
		}

        /// <summary>
        /// Gets the list of objects.
        /// </summary>
        /// <returns>List of objects.</returns>
        public T GetObject()
        {
			return (T)Data;
        }

		///// <summary>
		///// Gets a list of objects from a collection of DataGridViewRows.
		///// </summary>
		///// <param name="collection"></param>
		///// <returns></returns>
		//private List<T> GetObjects(IEnumerable<T> collection)
		//{
		//    List<T> list = new List<T>(collection.Count());
		//    var query = from item in collection
		//                select item;

		//    list.AddRange(query);

		//    return list;
		//}
    }
}
