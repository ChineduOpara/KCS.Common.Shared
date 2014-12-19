using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace KCS.Common.Shared
{
	/// <summary>
	/// Contains general extension methods.
	/// </summary>
	public static class Extensions
    {
        private static Random _random = new Random();

        public static T[] Shuffle<T>(this IEnumerable<T> array)
        {
            List<KeyValuePair<int, T>> list = new List<KeyValuePair<int, T>>();
            // Add new random int each time
            foreach (T s in array)
            {
                list.Add(new KeyValuePair<int, T>(_random.Next(), s));
            }
            // Sort the list by the random number
            var sorted = from item in list
                         orderby item.Key
                         select item;
            // Allocate new string array
            T[] result = new T[array.Count()];

            // Copy values to array
            int index = 0;
            foreach (KeyValuePair<int, T> pair in sorted)
            {
                result[index] = pair.Value;
                index++;
            }

            // Return copied array
            return result;
        }		

		/// <summary>
		/// Flashes a window.
		/// </summary>
		/// <param name="form">Form whose window will be flashed.</param>
		public static void FlashWindow(this Form form)
		{
			FlashWindow(form, Win32API.User32.FlashWindow.UntilForeground);
		}

		/// <summary>
		/// Flashes a window.
		/// </summary>
		/// <param name="form">Form whose window will be flashed.</param>
		/// <param name="mode">Window flashing mode</param>
		public static void FlashWindow(this Form form, Win32API.User32.FlashWindow mode)
		{
			Win32API.User32.FLASHWINFO fw = new Win32API.User32.FLASHWINFO();

			fw.cbSize = Convert.ToUInt32(System.Runtime.InteropServices.Marshal.SizeOf(typeof(Win32API.User32.FLASHWINFO)));
			fw.hwnd = form.Handle;
			fw.dwFlags = (int)mode;
			fw.uCount = UInt32.MaxValue;
			Win32API.User32.FlashWindowEx(ref fw);
		}        		

		///// <summary>
		///// Puts a blank row into the given datatable.
		///// </summary>
		///// <param name="dt"></param>
		///// <param name="position">0-based position in which to insert the new row.</param>
		///// <returns>Newly-added row.</returns>
		//public static DataRow AddBlankRow(this DataTable dt, int position)
		//{
		//    DataRow dr = dt.NewRow();
		//    foreach (DataColumn col in dt.Columns)
		//    {
		//        if (col.DefaultValue != null)
		//        {
		//            dr[col] = col.DefaultValue;
		//        }

		//        // If the column is still empty, but must have something in it, then use the default
		//        // of that particular type.
		//        if (Convert.IsDBNull(dr[col]) && !col.AllowDBNull && col.DataType.IsValueType)
		//        {
		//            dr[col] = Activator.CreateInstance(col.DataType);
		//        }				
		//    }
		//    dt.Rows.InsertAt(dr, position);
		//    return dr;
		//}

		public static string GetErrorsCSV(this DataTable dt)
		{
			return string.Join(", ", dt.GetErrors().Select(x => x.RowError).ToArray());
		}

        /// <summary>
        /// Make sure all the rows in a table have an Index, regardless of position. Skips deleted rows.
        /// </summary>
        public static void EnsureIndexes(this DataTable dt)
        {
            dt.EnsureColumn("INDEX", typeof(int));
            DataRow[] currentRows = dt.Select("", "", DataViewRowState.CurrentRows);
            for (int i = 0; i < currentRows.Length; i++)
            {
                DataRow dr = currentRows[i];
                bool acceptChange = !Utility.IsRowReallyChanged(dr);
                dr["INDEX"] = i;
                if (acceptChange) dr.AcceptChanges();
            }
        }

        /// <summary>
        /// Make sure all the rows in the view have an Index.
        /// </summary>
        public static void EnsureIndexes(this DataView dv)
        {
            dv.Table.EnsureColumn("INDEX", typeof(int));
            for (int i = 0; i < dv.Count; i++)
            {
                bool acceptChange = !Utility.IsRowReallyChanged(dv[i].Row);
                dv[i]["INDEX"] = i;
                if (acceptChange) dv[i].Row.AcceptChanges();
            }
        }

		/// <summary>
		/// Gets 2 columns of a table as a list of non-unique Key-Value pairs.
		/// </summary>
		/// <param name="dt">Table to work on.</param>
		/// <param name="columnName">Name of the column that will be the key and value.</param>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<T, K>> GetDictionary<T, K>(this DataTable dt, string columnName)
		{
			return dt.GetDictionary<T, K>(columnName, columnName);
		}

		/// <summary>
		/// Gets 2 columns of a table as a list of non-unique Key-Value pairs.
		/// </summary>
		/// <param name="table">Table to work on.</param>
		/// <param name="keyColumnName">Name of the column that will be the keys.</param>
		/// <param name="valueColumnName">Name of the column that will be the values.</param>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<T, K>> GetDictionary<T, K>(this DataTable table, string keyColumnName, string valueColumnName)
		{
			KeyValuePair<T, K> pair;
			foreach (DataRow row in table.Rows)
			{
				T value1 = (T)Convert.ChangeType(row[keyColumnName], typeof(T));
				K value2 = (K)Convert.ChangeType(row[valueColumnName], typeof(K));
				pair = new KeyValuePair<T, K>(value1, value2);
				yield return pair;
			}
		}

		/// <summary>
		/// Gets a list of all the values of a particular column, as a CSV.
		/// </summary>
		/// <param name="table">Table to work on.</param>
		/// <param name="columnName">Name of the column to be extracted.</param>
		/// <returns></returns>
		public static string GetColumnValuesAsCSV(this DataTable table, string columnName)
		{
			var query = from row in table.Select()
						select Convert.IsDBNull(row[columnName]) ? string.Empty : row[columnName].ToString();

			return string.Join(",", query.ToArray());
		}

		/// <summary>
		/// Gets a list of all the values of a particular column, as a List.
		/// </summary>
		/// <param name="table">Table to work on.</param>
		/// <param name="columnName">Name of the column to be extracted.</param>
		/// <returns></returns>
		public static List<T> GetColumnValuesAsList<T>(this DataTable table, string columnName, bool distinct)
		{
			var query = from row in table.Select()
						where (!(row[columnName] == null || Convert.IsDBNull(row[columnName])))
						select (T)System.Convert.ChangeType(row[columnName], typeof(T));

            if (distinct)
            {
                return query.Distinct().ToList();
            }
            else
            {
                return query.ToList();
            }
		}

        /// <summary>
        /// Gets a list of all the values of a particular column, as a List.
        /// </summary>
        /// <param name="table">Table to work on.</param>
        /// <param name="columnName">Name of the column to be extracted.</param>
        /// <returns></returns>
        public static List<T> GetColumnValuesAsList<T>(this DataView dv, string columnName, bool distinct)
        {
            List<T> list = new List<T>();
            foreach (DataRowView drv in dv)
            {
                if (drv[columnName] == null || Convert.IsDBNull(drv[columnName]))
                    continue;
                list.Add((T)System.Convert.ChangeType(drv[columnName], typeof(T)));
            }

            if (distinct)
            {
                list = list.Distinct().ToList();
            }
            return list;
        }

        public static List<T> GetColumnValuesAsList<T>(this DataTable table, string columnName)
        {
            return GetColumnValuesAsList<T>(table, columnName, false);
        }

        public static List<T> GetColumnValuesAsList<T>(this DataView dv, string columnName)
        {
            return GetColumnValuesAsList<T>(dv, columnName, false);
        }

		/// <summary>
		/// Reduces the columns in a table to a set list.
		/// </summary>
		/// <param name="table">Table to reduce.</param>
		/// <param name="columnNames">Names of the columns to keep.</param>
		public static void TrimToColumns(this DataTable table, params string[] columnNames)
		{
			TrimToColumns(table, columnNames.ToList());
		}

		/// <summary>
		/// Reduces the columns in a table to a set list.
		/// </summary>
		/// <param name="table">Table to reduce.</param>
		/// <param name="columnNames">Names of the columns to keep.</param>
		public static void TrimToColumns(this DataTable table, IEnumerable<string> columnNames)
		{
			int index = 0;
			
			// Make all the column names lowercase, to be sure comparison happens properly.
			IList<string> list = columnNames.ToList();
			for(int i = 0; i < list.Count; i++)
			{
				list[i] = list[i].ToLower();
			}

			// Drop all unnecessary columns from dt, to make it lighter.
			do
			{
                if (list.Contains(table.Columns[index].ColumnName.ToLower()) || table.PrimaryKey.Contains(table.Columns[index]))
				{
					index++;
				}
				else
				{
					table.Columns.RemoveAt(index);
				}
			} while (index < table.Columns.Count);
		}

        public static void ClearExpressions(this DataTable dt)
        {
            foreach (DataColumn dc in dt.Columns)
            {
                if (!string.IsNullOrEmpty(dc.Expression))
                {
                    dc.Expression = string.Empty;
                }
            }
        }

        public static void ClearRowErrors(this DataTable dt)
        {
            foreach (DataRow dr in dt.Rows)
            {
                dr.ClearErrors();
            }
        }

        /// <summary>
        /// Created by Georgi.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="rowLimit"></param>
        /// <returns></returns>
        public static IEnumerable<DataTable> Split(this DataTable dt, int rowLimit)
        {
            if (rowLimit == 0) return new List<DataTable>(new DataTable[] { dt });

            int size = dt.Rows.Count / rowLimit == 0 ? 1 : dt.Rows.Count / rowLimit;
            List<DataTable> lstTables = new List<DataTable>(size);

            int count = 0;
            DataTable copyTable = null;

            foreach (DataRow dr in dt.Rows)
            {
                if ((count % rowLimit) == 0)
                {
                    copyTable = new DataTable();            // Clone the structure of the table.     
                    copyTable = dt.Clone();            // Add the new DataTable to the list.   
                    lstTables.Add(copyTable);
                }

                // Import the current row.    
                copyTable.ImportRow(dr);
                count++;
            }

            return lstTables;
        }
		
		/// <summary>
		/// Delegate to support the GetXML method.
		/// </summary>
		/// <param name="dr"></param>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public delegate string FormatDataForXMLDelegate(DataRow dr, string columnName);

		/// <summary>
		/// Gets the pure-XML for saving.
		/// </summary>
		/// <param name="dt">This datatable.</param>
		/// <param name="rootElementName">Name of root element.</param>
		/// <param name="formatDelegate">Method to be called in case we need special formatting.</param>
		/// <returns>Well-formed XML.</returns>
		public static string GetXML(this DataTable dt, string rootElementName, FormatDataForXMLDelegate formatDelegate)
		{
			XmlDocument doc = new XmlDocument();// Create the XML Declaration, and append it to XML document
			XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", null, null);
			doc.AppendChild(dec);// Create the root element
			XmlElement root = doc.CreateElement(rootElementName);
			doc.AppendChild(root);

			foreach (DataRow dr in dt.Rows)
			{
				XmlElement elProd = doc.CreateElement(rootElementName);
				root.AppendChild(elProd);

				// Process all the columns. This is pretty straightforward
				foreach (DataColumn column in dt.Columns)
				{
					XmlElement elRow = doc.CreateElement(column.ColumnName.ToLower());
					if (formatDelegate == null)
					{
						elRow.InnerText = KCS.Common.Shared.Utility.GetStringValue(dr[column.ColumnName]);
					}
					else
					{
						elRow.InnerText = formatDelegate(dr, column.ColumnName);
					}
					elProd.AppendChild(elRow);
				}
			}

			return doc.OuterXml;
		}

        /// <summary>
        /// Sets the value in a row without disturbing the Modified flag.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <param name="columnName"></param>
        /// <param name="value"></param>
        public static void SetValueWithoutModifyingRow<T>(this DataRow dr, string columnName, T value)
        {
            bool doAccept = !dr.HasVersion(DataRowVersion.Proposed);
            dr[columnName] = value;

            if (doAccept)
            {
                dr.AcceptChanges();
            }
        }

        /// <summary>
        /// Builds a filter from a list of columns.
        /// </summary>
        /// <param name="olumns"></param>
        /// <param name="dr"></param>
        public static string GetFilterForColumns(this DataRow dr, IEnumerable<string> columns)
        {
            return dr.GetFilterForColumns(columns, false);
        }

        /// <summary>
        /// Builds a filter from a list of columns.
        /// </summary>
        /// <param name="olumns"></param>
        /// <param name="dr"></param>
        public static string GetFilterForColumns(this DataRow dr, IEnumerable<string> columns, bool treatNumericNullsAsZero)
        {
            List<string> filters = new List<string>(10);

            foreach (string col in columns)
            {
                if (Convert.IsDBNull(dr[col]))
                {
                    if (dr.Table.Columns[col].DataType.IsNumeric() && treatNumericNullsAsZero)
                    {
                        filters.Add(string.Format("{0} is null OR {0} = 0", col));
                    }
                    else
                    {
                        filters.Add(string.Format("{0} is null", col));
                    }
                }
                else
                {
                    if (dr.Table.Columns[col].DataType.IsNumeric())
                    {
                        filters.Add(string.Format("{0} = {1}", col, dr[col]));
                    }
                    else
                    {
                        filters.Add(string.Format("{0} = '{1}'", col, dr[col]));
                    }
                }
            }

            return string.Join(" AND ", filters.ToArray());
        }

        /// <summary>
        /// Gets the values of a particular property of a DataColumn collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <remarks>This should be refactored to handle different inputs</remarks>
        public static IEnumerable<T> GetColumnPropertyValue<T>(this DataColumnCollection dcCol, string propertyName)
        {
            Type type = typeof(DataColumn);
            PropertyInfo prop = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop == null)
            {
                throw new MissingMemberException(string.Format("The property \"{0}\" does not exist on type {1}", propertyName, type.FullName));
            }

            List<T> list = new List<T>(dcCol.Count);
            foreach (DataColumn dc in dcCol)
            {
                object value = prop.GetValue(dc, null);
                list.Add((T)Convert.ChangeType(value, typeof(T)));
            }
            return list;
        }

		/// <summary>
		/// Determines whether a column has been changed.
		/// </summary>
		/// <param name="columnName">Column name</param>
		/// <param name="row">the datarow</param>
		/// <returns>true if its changed, else false</returns>
		public static bool IsCellValueChanged(this DataRow row, string columnName)
		{
			// Added by COPARA 6/29/2010
			if (string.IsNullOrEmpty(columnName)) return false;
            if (row.RowState == DataRowState.Added) return true;

			object original = row[columnName, DataRowVersion.Original];
			object current = row[columnName, DataRowVersion.Current];

			return Utility.ColChanged2(original, current);
		}

		/// <summary>
		/// Gets a string of all the values in a DataRow.
		/// </summary>
		/// <param name="dr"></param>
		/// <returns></returns>
		/// <remarks>
		/// 1. This does not check for custom data types.
		/// </remarks>
		public static string GetConcatenatedValues(this DataRow dr, params string[] columnNames)
		{
			StringBuilder sb = new StringBuilder();
			foreach (string colname in columnNames)
			{
				sb.Append(Shared.Utility.GetStringValue(dr[colname]));
			}

			return sb.ToString();
		}

		/// <summary>
		/// Gets the number of rows that were deleted.
		/// </summary>
		/// <returns></returns>
		public static int GetDeletedRowCount(this DataTable dt)
		{
			DataRow[] rows = dt.Select("", "", DataViewRowState.Deleted);
			return rows.Length;
		}

		/// <summary>
		/// Gets the number of rows that were changed (added, modified, deleted).
		/// </summary>
		/// <returns></returns>
		public static int GetChangedRowCount(this DataTable dt)
		{
			return dt.GetDeletedRowCount() + dt.GetModifiedRowCount() + dt.GetAddedRowCount();
		}

		/// <summary>
		/// Gets the number of rows that were added.
		/// </summary>
		/// <returns></returns>
		public static int GetAddedRowCount(this DataTable dt)
		{
			DataRow[] rows = dt.Select("", "", DataViewRowState.Added);
			return rows.Length;
		}

		/// <summary>
		/// Gets the number of rows that were modified.
		/// </summary>
		/// <returns></returns>
		public static int GetModifiedRowCount(this DataTable dt)
		{
			int total = 0;
			DataTable dtChanged = dt.GetChanges(DataRowState.Modified);

			if (dtChanged != null && dtChanged.Rows.Count > 0)
			{
				foreach (DataRow dr in dtChanged.Rows)
				{
					foreach (DataColumn col in dt.Columns)
					{
						if (IsCellValueChanged(dr, col.ColumnName))
							total++;
					}
				}
			}


			return total;
		}

        /// <summary>
        /// Checks to see if any rows were changed or deleted.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static bool IsAnyRowModified(this DataTable dt)
        {
            foreach (DataRow dr in dt.Select("", "", DataViewRowState.CurrentRows))
            {
                if (Utility.IsRowReallyChanged(dr))
                    return true;
            }
            DataRow[] deletedRows = dt.Select("", "", DataViewRowState.Deleted);
            return deletedRows.Length > 0;
        }

        /// <summary>
        /// Checks to see if any rows were added, changed or deleted.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static bool IsChanged(this DataTable dt)
        {
            foreach (DataRow dr in dt.Select("", "", DataViewRowState.CurrentRows))
            {
                if (Utility.IsRowReallyChanged(dr))
                    return true;
            }
            DataRow[] deletedRows = dt.Select("", "", DataViewRowState.Deleted);
            if (deletedRows.Length > 0)
            {
                return true;
            }

            DataRow[] newRows = dt.Select("", "", DataViewRowState.Added);
            return newRows.Length > 0;
        }

		/// <summary>
		/// Gets the center of a Rect.
		/// </summary>
		/// <param name="rect"></param>
		/// <returns></returns>
		public static Point Center(this Rectangle rect)
		{
			return new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
		}

		/// <summary>
		/// Gets the hexcode representation of a color, without the # sign.
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		public static string GetHexCode(this Color color)
		{
			return GetHexCode(color, false);
		}

		/// <summary>
		/// Gets the hexcode representation of a color, without the # sign, with or without the alpha portion.
		/// </summary>
		/// <param name="color">Color to process</param>
		/// <param name="includeAlpha">If true, the alpha value is included.</param>
		/// <returns></returns>
		public static string GetHexCode(this Color color, bool includeAlpha)
		{
			if (includeAlpha)
			{
				return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);
			}
			else
			{
				return string.Format("{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
			}
		}		

        /*
		/// <summary>
		/// Gets the Color represented by an NCS code.
		/// </summary>
		/// <param name="c">Color.</param>
		/// <param name="ncsCode">NCS code, complete with prefix and spaces. For example "S 3502-Y"</param>
		/// <returns></returns>
		public static Color FromNCS(this Color c, string ncsCode, out string errorMessage)
		{
			errorMessage = "";

			try
			{
				int rgb = NCS.NCSUtils.NCSToRGB(ncsCode);
				return Color.FromArgb(rgb);
			}
			catch (NCS.InvalidNCSCodeException ex)
			{
				errorMessage = ex.Message;
				return System.Drawing.Color.Transparent;
			}
		}

		public static Color FromNCS(this Color c, string ncsCode)
		{
			string errorMessage;
			return FromNCS(c, ncsCode, out errorMessage);
		}*/

		/// <summary>
		/// Gets a value from a DataRow, taking into consideration the possibility of it being DBNull.
		/// </summary>
		/// <param name="row">Row containing value.</param>
		/// <param name="columnName">Column name.</param>
		/// <param name="default">Default value.</param>
		/// <returns>Column value, or default.</returns>
		public static T GetValue<T>(this DataGridViewRow dr, string columnName)
		{
			if (Convert.IsDBNull(dr.Cells[columnName].Value))
			{
				return default(T);
			}
			else
			{
				return (T)Convert.ChangeType(dr.Cells[columnName].Value, typeof(T));
			}
		}

		/// <summary>
		/// Gets a value from a DataRow, taking into consideration the possibility of it being DBNull.
		/// </summary>
		/// <param name="row">Row containing value.</param>
		/// <param name="columnName">Column name.</param>
		/// <param name="default">Default value.</param>
		/// <returns>Column value, or default.</returns>
		public static T GetValue<T>(this DataRow dr, string columnName, T @default)
		{
			if (Convert.IsDBNull(dr[columnName]))
			{
				return @default;
			}
			else
			{
				return (T)Convert.ChangeType(dr[columnName], typeof(T));
			}
		}

		public static T GetValue<T>(this DataRow dr, string columnName)
		{
			return dr.GetValue(columnName, default(T));
		}

		public static T GetValue<T>(this DataRowView drv, string columnName)
		{
			return drv.GetValue(columnName, default(T));
		}

		public static T GetValue<T>(this DataRowView drv, string columnName, T @default)
		{
			return drv.Row.GetValue(columnName, @default);
		}

		/// <summary>
		/// Copies data from one row to another. Any columns that don't exist in the target row
		/// are skipped.
		/// </summary>
		/// <param name="drTarget">Target row.</param>
		/// <param name="dtSource">Source row.</param>
		public static void CopyFrom(this DataRow drTarget, DataRow drSource)
		{
			foreach (DataColumn col in drSource.Table.Columns)
			{
				if (drTarget.Table.Columns.Contains(col.ColumnName))
				{
					drTarget[col.ColumnName] = drSource[col.ColumnName];
				}
			}
		}

        /// <summary>
        /// Gets the DataRowView that matches the given DataRow.
        /// </summary>
        /// <param name="drTarget">Target row.</param>
        public static DataRowView GetDataRowView(this DataRow dr)
        {
            DataRowView drvReturn = null;
            foreach (DataRowView drv in dr.Table.DefaultView)
            {
                if (drv.Row == dr)
                {
                    drvReturn = drv;
                }
            }

            if (drvReturn == null) throw new Exception("This DataRow does not belong to a valid DataTable! Check your calling code!");
            return drvReturn;
        }

		/// <summary>
		/// Copies data of a particular version from one row to another. Any columns that don't exist in the target row
		/// are skipped.
		/// </summary>
		/// <param name="drTarget">Target row.</param>
		/// <param name="dtSource">Source row.</param>
		public static void CopyFrom(this DataRow drTarget, DataRow drSource, DataRowVersion version)
		{
			foreach (DataColumn col in drSource.Table.Columns)
			{
				if (drTarget.Table.Columns.Contains(col.ColumnName))
				{
					if (drSource.HasVersion(version))
					{
						drTarget[col.ColumnName] = drSource[col.ColumnName, version];
					}
					else
					{
						drTarget[col.ColumnName] = drSource[col.ColumnName];
					}
				}
			}
		}

		/// <summary>
		/// Copies data from one row to another. Any columns that don't exist in the target row
		/// are skipped.
		/// </summary>
		/// <param name="drTarget">Target row.</param>
		/// <param name="dtSource">Source row.</param>
		/// <returns>Number of fields copied.</returns>
		public static uint CopyFrom(this DataRow drTarget, DataRow drSource, List<string> exclude)
		{
            uint counter = 0;
			foreach (DataColumn col in drSource.Table.Columns)
			{
				if (!exclude.Contains(col.ColumnName) && drTarget.Table.Columns.Contains(col.ColumnName))
				{
					drTarget[col.ColumnName] = drSource[col.ColumnName];
                    counter++;
				}
			}
            return counter;
		}

		/// <summary>
		/// Ensures that a table contains the given columns of the given type.
		/// </summary>
		/// <param name="dt">Datatable.</param>
		/// <param name="type">Desired type of the columns.</param>
		/// <param name="columns">Column Names.</param>
        /// <returns>Number of columns added.</returns>
		public static uint EnsureColumns<T>(this DataTable dt, params string[] columns)
		{
            uint counter = 0;
			foreach (string name in columns)
			{
                //if (!dt.Columns.Contains(name.ToUpper()))
                //{
                //    dt.Columns.Add(name.ToUpper(), type);
                //    counter++;
                //}

                if (!dt.Columns.Contains(name))
                {
                    dt.Columns.Add(name, typeof(T));
                    counter++;
                }
			}

            return counter;
		}

		/// <summary>
		/// Ensures that a table contains the given columns.
		/// </summary>
		/// <param name="dt">Datatable.</param>
		/// <param name="columns">Columns and matching types.</param>
		public static uint EnsureColumns(this DataTable dt, params KeyValuePair<string, Type>[] columns)
		{
            uint counter = 0;
			foreach (KeyValuePair<string, Type> pair in columns)
			{
                //if (!dt.Columns.Contains(pair.Key.ToUpper()))
                //{
                //    dt.Columns.Add(pair.Key.ToUpper(), pair.Value);
                //    counter++;
                //}

                if (!dt.Columns.Contains(pair.Key))
                {
                    dt.Columns.Add(pair.Key, pair.Value);
                    counter++;
                }
			}
            return counter;
		}

		/// <summary>
		/// Ensures that a table contains the given columns.
		/// </summary>
		/// <param name="dt">Datatable.</param>
		/// <param name="columns">Columns and matching types.</param>
		public static uint EnsureColumns(this DataTable dt, Dictionary<string, Type> columns)
		{
            uint counter = 0;
            //foreach (KeyValuePair<string, Type> pair in columns)
            //{
            //    if (!dt.Columns.Contains(pair.Key.ToUpper()))
            //    {
            //        dt.Columns.Add(pair.Key.ToUpper(), pair.Value);
            //        counter++;
            //    }
            //}

            foreach (KeyValuePair<string, Type> pair in columns)
            {
                if (!dt.Columns.Contains(pair.Key))
                {
                    dt.Columns.Add(pair.Key, pair.Value);
                    counter++;
                }
            }


            return counter;
		}

		/// <summary>
		/// Makes sure that a column is in the table.
		/// </summary>
		/// <param name="dt"></param>
		/// <param name="name"></param>
		public static DataColumn EnsureColumn(this DataTable dt, string name, Type type)
		{
            //if (dt.Columns.Contains(name.ToUpper()))
            //{
            //    return dt.Columns[name.ToUpper()];
            //}
            //else
            //{
            //    return dt.Columns.Add(name.ToUpper(), type);
            //}
            if (dt.Columns.Contains(name))
            {
                return dt.Columns[name];
            }
            else
            {
                return dt.Columns.Add(name, type);
            }
		}

		/// <summary>
		/// Makes sure that a string column is in the given table.
		/// </summary>
		/// <param name="dt"></param>
		/// <param name="name"></param>
		public static DataColumn EnsureColumn(this DataTable dt, string name)
		{
			return dt.EnsureColumn(name, typeof(string));
		}

		/// <summary>
		/// Ensures that a given column is of the particular type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dt"></param>
		/// <param name="columnName"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool EnsureColumnIsType(this DataTable dt, string columnName, Type type)
		{
			List<int> failureIndexes;
			return dt.EnsureColumnIsType(columnName, type, out failureIndexes);
		}

		/// <summary>
		/// Ensures that a given column is of the particular type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dt"></param>
		/// <param name="columnName"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool EnsureColumnIsType(this DataTable dt, string columnName, Type type, out List<int> failureIndexes)
		{
			int rowIndex = 0;
			string tempColumnName = "Temp_Col_Name";
			bool columnWasPrimaryKey = dt.PrimaryKey.Contains(dt.Columns[columnName]);
			List<DataColumn> pKeys = null;

			failureIndexes = new List<int>();
			dt.EnsureColumn(tempColumnName, type);

			foreach (DataRow dr in dt.Rows)
			{
				try
				{
					if (!Convert.IsDBNull(dr[columnName]))
					{
						dr[tempColumnName] = Convert.ChangeType(dr[columnName], type);
					}
				}
				catch
				{
					failureIndexes.Add(rowIndex++);
				}				
			}

			// Remember to preserve the primary key
			if (columnWasPrimaryKey)
			{
				pKeys = dt.PrimaryKey.ToList();
				pKeys.Remove(dt.Columns[columnName]);
				dt.PrimaryKey = null;
			}
			
			dt.Columns.Remove(columnName);

			if (columnWasPrimaryKey)
			{
				pKeys.Add(dt.Columns[tempColumnName]);
				dt.PrimaryKey = pKeys.ToArray();
			}

			// Rename the temporary column
			dt.Columns[tempColumnName].ColumnName = columnName;

			return failureIndexes.Count == 0;
		}

        /// <summary>
        /// After adding a column, sets the default value in all rows.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="dt"></param>
        /// <param name="name"></param>
        /// <param name="type">Type of column.</param>
        /// <param name="defaultValue">Default value. If null, the allowNull parameter is ignored.</param>
        /// <param name="allowNull">Only used if the defaultValue parameter is not null.</param>
        /// <returns></returns>
        public static DataColumn EnsureColumn<T>(this DataTable dt, string name/*, bool allowNull, T defaultValue*/)
        {
            DataColumn c = dt.EnsureColumn(name, typeof(T));
            //if (!Convert.IsDBNull(defaultValue) || defaultValue == null)
            //{
            //    foreach (DataRow dr in dt.Rows)
            //    {
            //        dr[name] = defaultValue;
            //    }

            //    if (!allowNull)
            //    {
            //        dt.Columns[name].AllowDBNull = allowNull;
            //    }
            //}
            

            return c;
        }		

        /// <summary>
        /// Converts a raw binary data back to a string, using the default ASCII encoding.
        /// </summary>
        /// <param name="data">Data to convert.</param>
        /// <returns>A string, or null.</returns>
        public static string GetString(this byte[] data)
        {
            return GetString(data, new System.Text.ASCIIEncoding());
        }

        /// <summary>
        /// Converts a raw binary data back to a string.
        /// </summary>
        /// <param name="data">Data to convert.</param>
        /// <param name="encoding">Desired encoding.</param>
        /// <returns>A string, or null.</returns>
        public static string GetString(this byte[] data, System.Text.Encoding encoding)
        {
            return encoding.GetString(data);
        }

        public static string Right(this string @string, int length)
        {
            if (@string.Length > length)
            {
                return @string.Substring(@string.Length - length, length);
            }
            else
            {
                return @string;
            }
        }

        public static string Left(this string @string, int length)
        {
            if (@string.Length > length)
            {
                return @string.Substring(0, length);
            }
            else
            {
                return @string;
            }
        }

		/// <summary>
		/// Gets the length of a string, in the context of its host control.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="ctrl"></param>
		/// <returns></returns>
		public static float GetLength(this string str, Control ctrl)
		{
			Graphics graphics = null;

			try
			{
				graphics = Graphics.FromHwnd(ctrl.Handle);
				return graphics.MeasureString(str, ctrl.Font).Width;
			}
			catch
			{
				return 0;
			}
			finally
			{
				if (graphics != null)
				{
					graphics.Dispose();
					graphics = null;
				}
			}
		}

		/// <summary>
		/// Created by 9OPARA7. Gets the width of the longest string in a collection.
		/// </summary>
		/// <param name="list">List of strings.</param>
		/// <param name="ctrl">Control surface used as reference.</param>
		/// <param name="defaultValue">Default value, in case the method fails.</param>
		/// <returns></returns>
		public static float GetLongestString(this IList<string> list, Control ctrl, float defaultValue)
		{
			float textLength;
			float widestTextLength = 0;
			try
			{
				foreach (string value in list)
				{
					textLength = value.GetLength(ctrl);
					if (textLength > widestTextLength)
					{
						widestTextLength = textLength;
					}
				}
				return widestTextLength;
			}
			catch
			{
				return defaultValue;
			}
		}

        /// <summary>
        /// Gets all the files from a given folder, that match certain extensions
        /// </summary>
        /// <param name="path">Folder to list files from.</param>
        /// <param name="extensions">Array of filename extensions.</param>
        /// <returns>Array of FileInfo objects.</returns>
        public static IEnumerable<FileInfo> GetFiles(this DirectoryInfo dirInfo, bool recursive, IEnumerable<string> extensions)
        {
            var extList = extensions.Select(x => x.Replace("*", "")).ToList();
            extList = extList.ConvertAll(x => x.StartsWith(".") ? x : "." + x);
            var allowedExtensions = new HashSet<string>(extList, StringComparer.OrdinalIgnoreCase);

            return dirInfo.EnumerateFiles("*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                          .Where(f => allowedExtensions.Contains(f.Extension));
        }

        public static IEnumerable<FileInfo> GetFiles(this DirectoryInfo dirInfo, params string[] extensions)
        {
            return GetFiles(dirInfo, true, extensions);
        }

        /// <summary>
        /// Dot Net DOES NOT have a String split function.  It can only split by a character or ONE OF the characters but not ALL characters.
        /// </summary>
        /// <param name="inString"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string[] Split(this string inString, string delimiter)
        {
            // TODO: Rewrite this using regular expressions
            int i = -1, p = 0;
            if (inString.Length == 0) return new String[0];
            ArrayList s = new ArrayList();
            while (true)
            {
                i = inString.IndexOf(delimiter, i + 1);
                if (i == -1)
                {
                    s.Add(inString.Substring(p, inString.Length - p));
                    break;
                }
                else
                {
                    s.Add(inString.Substring(p, i - p));
                    p = i + delimiter.Length;
                }
            }
            string[] s2 = new string[s.Count];
            s.CopyTo(s2);
            return s2;
        }

        /// <summary>
        /// Performs a case-insensitive (or sensitive) replace.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        /// <param name="comparison"></param>
        /// <returns></returns>
        static public string Replace(this string input, string oldValue, string newValue, StringComparison comparison)
        {
            StringBuilder sb = new StringBuilder();

            int previousIndex = 0;
            int index = input.IndexOf(oldValue, comparison);
            while (index != -1)
            {
                sb.Append(input.Substring(previousIndex, index - previousIndex));
                sb.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = input.IndexOf(oldValue, index, comparison);
            }
            sb.Append(input.Substring(previousIndex));

            return sb.ToString();
        }		

        ///// <summary>
        ///// Exports a DataGridView to Excel.
        ///// </summary>
        ///// <param name="dgv"></param>
        //public static int ExportToExcel<T, K>(this System.Windows.Forms.DataGridView dgv)
        //{
        //    return ExportToExcel<T, K>(dgv, null, null, true, Size.Empty);
        //}

        ///// <summary>
        ///// Exports a DataGridView to Excel, with the option to exclude particular columns.
        ///// </summary>
        ///// <param name="dgv"></param>
        ///// <param name="excludeColumns">
        ///// Names of column names (not header texts, but actual underlying column names)
        ///// that will be excluded from the output.
        ///// </param>
        //public static int ExportToExcel<T, K>(this System.Windows.Forms.DataGridView dgv, IEnumerable<string> excludeColumns, IDictionary<string, List<KeyValuePair<T, K>>> lookupColumns, bool includeUnboundColumns, System.Drawing.Size imageColSize)
        //{
        //    System.Threading.Thread thisThread = System.Threading.Thread.CurrentThread;
        //    System.Globalization.CultureInfo originalCulture = thisThread.CurrentCulture;

        //    try
        //    {
        //        thisThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        //        return ExportToExcelInner(dgv, excludeColumns, lookupColumns, originalCulture, includeUnboundColumns, imageColSize);

        //    }
        //    finally
        //    {
        //        thisThread.CurrentCulture = originalCulture;
        //    }
        //}

        //private static int ExportToExcelInner<T, K>(this System.Windows.Forms.DataGridView dgv, IEnumerable<string> excludeColumns, IDictionary<string, List<KeyValuePair<T, K>>> lookupColumns, System.Globalization.CultureInfo cultureInfo, bool includeUnboundColumns, System.Drawing.Size imageColSize)
        //{
        //    DesktopMSExcel excel = new DesktopMSExcel(true);            
        //    List<DataGridViewColumn> visibleColumns = new List<DataGridViewColumn>(dgv.DisplayedColumnCount(true));
        //    int rowIndex, columnIndex = 0;
        //    int totalExported = 0;
        //    Type colType;
        //    bool isLookupColumn;

        //    bool isImage;
        //    List<object> colData = new List<object>(dgv.Rows.Count);
        //    object[,] dtArr = new object[dgv.Rows.Count, 1];

        //    // Sanity checks.
        //    if (excludeColumns == null)
        //    {
        //        excludeColumns = new string[0];
        //    }
        //    if (lookupColumns == null)
        //    {
        //        lookupColumns = new Dictionary<string, List<KeyValuePair<T, K>>>();
        //    }

        //    excel.CreateNewWorkSheet();
        //    excel.SetReadonly(true);
        //    dgv.TopLevelControl.Focus();

        //    // Get the columns we'll be exporting. If the includeUnboundColumns is false, then we need to exclude unbound columns
        //    foreach (DataGridViewColumn col in dgv.Columns)
        //    {
        //        if (col.Visible && !excludeColumns.Contains(col.DataPropertyName))              // Visible + not excluded
        //        {
        //            if (includeUnboundColumns || !string.IsNullOrEmpty(col.DataPropertyName))   // Respect the includeUnboundColumns parameter
        //            {
        //                visibleColumns.Add(col);
        //            }
        //        }
        //    }

        //    #region Populate the default worksheet
        //    // Now fill the sheet
        //    for (int i = 0; i < visibleColumns.Count; i++)
        //    {
        //        rowIndex = 1;
        //        colType = visibleColumns[i].ValueType;
        //        isLookupColumn = lookupColumns.ContainsKey(visibleColumns[i].DataPropertyName);
        //        isImage = (colType == typeof(System.Drawing.Image));

        //        // Headers first.
        //        // TODO: Use a neutral background color
        //        excel.SetCellValue(1, ++columnIndex, visibleColumns[i].HeaderText, false, true);

        //        colData.Clear();

        //        // Fill the rows of each column
        //        for (int j = 0; j < dgv.Rows.Count; j++)
        //        {
        //            object value = dgv.Rows[j].Cells[visibleColumns[i].Name].Value;
        //            if (Utility.GetStringValue(value).Length > 0 && isLookupColumn)
        //            {
        //                List<KeyValuePair<T, K>> pairs = lookupColumns[visibleColumns[i].DataPropertyName];
        //                KeyValuePair<T, K> match = pairs.Where(x => x.Key.ToString().Equals(value.ToString())).FirstOrDefault();
        //                value = match.Value;
        //            }
        //            if (isImage)
        //            {
        //                if (value != null)
        //                {
        //                    colData.Add(value);
        //                }
        //                else
        //                {
        //                    colData.Add(null);
        //                }
        //            }
        //            else
        //            {
        //                //xl.SetCellValue(++rowIndex, columnIndex, Utility.GetStringValue(value), colType.IsNumeric(), false);
        //                colData.Add(Utility.GetStringValue(value));
        //            }
        //        }

        //        for (int j = 0; j < dgv.Rows.Count; j++)
        //        {
        //            dtArr[j, 0] = colData[j];
        //        }

        //        // Set the actual value in the cell (image, non-image)
        //        if (isImage)
        //        {
        //            excel.SetImageCellValue(2, i + 1, dtArr);
        //        }
        //        else
        //        {
        //            excel.SetCellValue(2, i + 1, dtArr, colType.IsNumeric());
        //        }
        //    }
        //    #endregion

        //    excel.AutoFit();
        //    excel.SetReadonly(false);
        //    excel.ResetCellTypes();

        //    #region Resize image columns
        //    var imageColumns = visibleColumns.Where(x => x.ValueType == typeof(Image));
        //    // If the image column Size was provided, attempt to set the column width of the image column
        //    if (imageColSize != Size.Empty)
        //    {
        //        try
        //        {                    
        //            if (imageColumns.Count() > 0)
        //            {                        
        //                ResizeImageColumns(excel, imageColumns, imageColSize);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            // Unable to resize image columns. Oh well. Life goes on...
        //        }
        //    }
        //    #endregion

        //    // Set the totalExported value by extracting it from the active worksheet.
        //    int maxCols = 0;
        //    string range;
        //    excel.GetMaxRowAndCol(out totalExported, out maxCols, out range);
            
        //    return totalExported;
        //}

        ///// <summary>
        ///// Resizes image columns to certain dimensions.
        ///// </summary>
        ///// <param name="excel"></param>
        ///// <param name="imageColumns"></param>
        ///// <param name="imageColSize"></param>
        //private static void ResizeImageColumns(DesktopMSExcel excel, IEnumerable<DataGridViewColumn> imageColumns, Size imageColSize)
        //{
        //    Microsoft.Office.Interop.Excel.Range firstRow;
        //    Microsoft.Office.Interop.Excel.Range cellRange;

        //    excel.SetWorksheets();
        //    DataTable dtWorkSheets = excel.GetWorksheets();
        //    string firstSheetName = dtWorkSheets.Rows[0]["Name"].ToString();
        //    Microsoft.Office.Interop.Excel.Worksheet wrkSheet = (Microsoft.Office.Interop.Excel.Worksheet)excel.Worksheets[firstSheetName];

        //    // Save the height of the first row
        //    firstRow = (Microsoft.Office.Interop.Excel.Range)wrkSheet.Rows[1, Type.Missing];
        //    int originalHeight = Convert.ToInt32(firstRow.EntireRow.RowHeight);

        //    // Set the column widths and the row height                        
        //    foreach (DataGridViewColumn col in imageColumns)
        //    {
        //        cellRange = (Microsoft.Office.Interop.Excel.Range)wrkSheet.Columns[col.DisplayIndex + 1, Type.Missing];
        //        cellRange.EntireColumn.ColumnWidth = imageColSize.Width/7;
        //        cellRange.EntireColumn.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
        //        cellRange.EntireColumn.VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
                
        //        //cellRange.EntireColumn.Select();
        //        cellRange.EntireRow.RowHeight = imageColSize.Height;
        //        cellRange.EntireRow.VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
        //    }

        //    // reset the height of the first row
        //    firstRow.EntireRow.RowHeight = originalHeight;
        //}

        ///// <summary>
        ///// Checks to make sure that a date is within a list of dates (time is ignored).
        ///// </summary>
        ///// <param name="dates">List of dates.</param>
        ///// <param name="date">Date to check.</param>
        ///// <returns>Boolean.</returns>
        //public static bool Contains(this IEnumerable<DateTime> dates, DateTime date)
        //{
        //    var distinctDates = dates.Select(x => x.Date).Distinct();
        //    return distinctDates.Contains(date.Date);
        //}

        /// <summary>
        /// Gets the date and time in a format that can be sent via querystring, with or without the time part.
        /// </summary>
        /// <param name="date">DateTime instance.</param>
        /// <returns></returns>
        public static string ToRoundTripString(this DateTime date, bool includeTime = true)
        {
            string sFormat = date.ToString("o");
            if (includeTime)
            {
                return sFormat;
            }
            else
            {
                string[] parts = sFormat.Split("T".ToCharArray());
                return parts[0];
            }            
        }

        /// <summary>
        /// Gets the date and time, without the milliseconds.
        /// </summary>
        /// <param name="date">DateTime instance.</param>
        /// <returns></returns>
        public static DateTime RemoveMilliseconds(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
        }

		/// <summary>
		/// Gets the full month name.
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static string GetMonthName(this DateTime date)
		{
			return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month);
		}

		/// <summary>
		/// Gets the abbreviated month name.
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static string GetAbbreviatedMonthName(this DateTime date)
		{
			return CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(date.Month);
		}

        /// <summary>
        /// Gets the 4-digit UTC time from a DateTime.
        /// </summary>
        /// <param name="dt">DateTime value.</param>
        /// <param name="militaryTime">If true, returns the time in 24-hour format. Otherwise, 12-hour format.</param>
        /// <returns>4-digit time string.</returns>
        public static string Get4DigitUTCTime(this DateTime dt, bool militaryTime)
        {
            if (dt == DateTime.MinValue)
            {
                return string.Empty;
            }
            try
            {
                if (dt.Kind != DateTimeKind.Utc)
                {
                    dt = dt.ToUniversalTime();
                }
                return dt.ToString(militaryTime ? "HHmm" : "hhmm");
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// A "common language" representation of the DATE part only.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ToCommonString(this DateTime date, bool includeTime = false)
        {
            string displayString = date.ToString("D");
            if (date.Date == DateTime.Today.Date)
            {
                displayString = "Today";
            }

            if (date.Date == DateTime.Today.Date.AddDays(-1))
            {
                displayString = "Yesterday";
            }

            if (date.Date == DateTime.Today.Date.AddDays(1))
            {
                displayString = "Tomorrow";
            }

            if (includeTime)
            {
                displayString = string.Format("{0} @ {1:t}", displayString, date);
            }

            return displayString;
        }

        /// <summary>
        /// A short string representation of a date.
        /// </summary>
        /// <returns></returns>
        public static string ToShortString(this DateRange date, bool includeTime = true)
        {
            bool sameDay = date.Start.Date == date.End.Date;

            if (includeTime)
            {
                if (sameDay)
                {
                    return string.Format("{0} @ {1:t} - {2:t}", date.Start.ToCommonString(), date.Start, date.End);
                }
                else
                {
                    return string.Format("{0:g} - {1:g}", date.Start, date.End);
                }
            }
            else
            {
                return string.Format("{0:t} - {1:t}", date.Start, date.End);
            }
        }

		/// <summary>
		/// Get the first PropertyInfo object matching the given Property name.
		/// </summary>
		/// <param name="type">Type to query.</param>
		/// <param name="propertyName">Propery name for which to search.</param>
		/// <returns>Property Info.</returns>
		public static PropertyInfo GetValuePropertyInfo(this Type type, string propertyName)
		{
			foreach (PropertyInfo pi in type.GetProperties())
			{
				if (string.Compare(pi.Name, propertyName, true) == 0)
				{
					return pi;
				}
			}
			return null;
		}

        public static string GetAttributeValue(this XmlNode node, string attributeName)
        {
            return GetAttributeValue<string>(node, attributeName, null);
        }

        public static T GetAttributeValue<T>(this XmlNode node, string attributeName, T defaultValue = default(T))
        {
            object data = null;
            if (node.Attributes[attributeName] != null)
            {
                data = node.Attributes[attributeName].Value;

                try
                {   
                    return (T)Convert.ChangeType(data, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }
        }

        public static bool? GetBoolAttributeValue(this XmlNode node, string attributeName, bool? defaultValue = null)
        {
            object data = null;
            if (node.Attributes[attributeName] != null)
            {
                data = node.Attributes[attributeName].Value;

                try
                {
                    bool result = false;
                    if (bool.TryParse(data.ToString(), out result))
                    {
                        return result;
                    }
                    else
                    {
                        return defaultValue;
                    }
                }
                catch
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Retruns a string list with quoted strings 
        /// <example>list.GetQuotedString("'")</example>
        /// </summary>
        /// <param name="list"></param>
        /// <param name="quote">The quote to use</param>
        /// <returns></returns>
        public static List<string> GetQuotedString(this List<string> list, string quote)
        {
            return list.Select(l => { return string.Format("{0}{1}{0}",quote, l); }).ToList<string>();
        }		
	}
}
