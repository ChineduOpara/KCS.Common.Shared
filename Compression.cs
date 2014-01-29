using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Data;
using System.Xml;

namespace KCS.Common.Shared
{
	/// <summary>
	/// Provides static compression-related methods.
	/// </summary>
	/// <remarks>
	/// These routines are mainly used for converting a DataSet into byte[] so that it can be
	/// transported through web services. XML comments above each method were added by 9OPARA7.
	///
	/// There are 2 sets of routines:  
	/// 1. Compress and de-compress a dataset with the Current Data.  RowState will always be equal to DataRowState.Added.
	/// 2. Compress and de-compress a dataset with the Current Data AND Original Data, preserving RowState.
	///
	/// #2 above will result in bigger size.
	/// 
	/// #1 usage:
	/// Before sending the DataSet to a web service, call CompressDataSet and pass the resulting byte[] to 
	/// the Web Service.  On the Web Service side, call the DeCompressDataSet or DeCompressDataTable to get back the
	/// DataSet or DataTable, respectively.
	///
	/// #2 usage:
	/// Before sending the DataSet to a web service, call CompressDataSetDiffGram and CompressDataSetSchema to return
	/// the DataSet content and DataSet schema.   Note the Schema and content cannot be stored in the same
	/// XML file so we have to send the data and schema separately.  On the Web Service side, call DeCompressDataSet
	/// and pass the DiffGram and Schema byte[] to get back the dataset.
	/// </remarks>
	public static class Compression
	{
		/// <summary>
		/// Compresses a Dataset's schema.
		/// </summary>
		/// <param name="ds">DataSet whose schema will be compressed.</param>
		/// <returns>Byte array.</returns>
		public static byte[] CompressDataSetSchema(DataSet ds)
		{
			return CompressDataSet(ds, true, false);
		}

		/// <summary>
		/// Compresses a DataSet's DiffGram.
		/// </summary>
		/// <param name="ds">DataSet whose DiffGram will be compressed.</param>
		/// <returns>Byte array.</returns>
		public static byte[] CompressDataSetDiffGram(DataSet ds)
		{
			return CompressDataSet(ds, false, true);
		}

		/// <summary>
		/// Compresses a DataRow. This works by attaching the row into a dummy table, and compressing that.
		/// </summary>
		/// <param name="dr">DataRow to be compressed.</param>
		/// <returns>Byte array.</returns>
		public static byte[] CompressDataRow(DataRow dr)
		{
			// Created by COpara. Not yet tested as of 2/16
			DataTable dt = dr.Table.Clone();
			dt.Rows.Add(dr.ItemArray);
			return CompressDataTable(dt);
		}

		/// <summary>
		/// Compresses a DataTable. This works by attaching a default DataSet to the table, and calling CompressDataSet.
		/// </summary>
		/// <param name="dt">DataTable to be compressed.</param>
		/// <returns>Byte array.</returns>
		public static byte[] CompressDataTable(DataTable dt)
		{
			if (dt.DataSet == null)
			{
				DataSet ds = new DataSet();
				ds.Tables.Add(dt);
			}
			return CompressDataSet(dt.DataSet, false, false);
		}

		/// <summary>
		/// Compresses a DataSet.
		/// </summary>
		/// <param name="ds">DataSet to be compressed.</param>
		/// <returns>Byte array.</returns>
		public static byte[] CompressDataSet(DataSet ds)
		{
			return CompressDataSet(ds, false, false);
		}

		/// <summary>
		/// Compresses a DataSet.
		/// </summary>
		/// <param name="ds">DataSet to be compressed.</param>
		/// <param name="schemaOnly">If TRUE, compresses only the schema.</param>
		/// <param name="diffGram"></param>
		/// <returns>Byte array.</returns>
		private static byte[] CompressDataSet(DataSet ds, bool schemaOnly, bool diffGram)
		{
			SetNeutralDataTable(ds);
			MemoryStream ms2 = new MemoryStream();
			ICSharpCode.SharpZipLib.Zip.Compression.Deflater defl = new ICSharpCode.SharpZipLib.Zip.Compression.Deflater(9, false);
			Stream s = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream(ms2, defl);
			MemoryStream ms3 = new MemoryStream();
			if (schemaOnly)
			{
				ds.WriteXmlSchema(ms3);
			}
			else
			{
				ds.WriteXml(ms3, (diffGram) ? XmlWriteMode.DiffGram : XmlWriteMode.WriteSchema);
			}
			s.Write(ms3.ToArray(), 0, (int)ms3.Length);
			s.Close();
            
			return (byte[])ms2.ToArray();

		}

		/// <summary>
		/// Converts all the DateTime columns in a dataset to Unspecified mode.
		/// </summary>
		/// <param name="ds">DataSet to convert.</param>
		/// <remarks>
		/// I don't think this method is properly named.
		/// </remarks>
		public static void SetNeutralDataTable(DataSet ds)
		{
			foreach (DataTable dt in ds.Tables)
			{
				foreach (DataColumn dc in dt.Columns)
				{
					if (dc.DataType == typeof(DateTime))
					{
						dc.DateTimeMode = DataSetDateTime.Unspecified;
					}
				}
			}
		}

		/// <summary>
		/// Converts all the DataTime columns in a dataset to UnspecifiedLocal mode.
		/// </summary>
		/// <param name="ds">DataSet to convert.</param>
		/// <remarks>
		/// I don't think this method is properly named.
		/// </remarks>
		public static void SetDefaultDataTable(DataSet ds)
		{
			foreach (DataTable dt in ds.Tables)
			{
				foreach (DataColumn dc in dt.Columns)
				{
					if (dc.DataType == typeof(DateTime))
					{
						dc.DateTimeMode = DataSetDateTime.UnspecifiedLocal;
					}
				}
			}
		}

		/// <summary>
		/// Decompresses a byte array representation of a DataTable.
		/// It works by decompressing the entire DataSet and returning the first table.
		/// </summary>
		/// <param name="bytDs">Byte array to be decompressed.</param>
		/// <returns>DataTable.</returns>
		public static DataTable DecompressDataTable(byte[] bytDs)
		{
			return DecompressDataSet(bytDs).Tables[0];
		}

		/// <summary>
		/// Decompresses a byte array representation of a DataSet.
		/// </summary>
		/// <param name="bytDs">Byte array to be decompressed.</param>
		/// <returns>DataSet.</returns>
		public static DataSet DecompressDataSet(byte[] bytDs)
		{
			return DecompressDataSet(bytDs, null);
		}

		/// <summary>
		/// Decompresses a byte array representation of a DataSet.
		/// </summary>
		/// <param name="bytDs">Byte array to be decompressed.</param>
		/// <param name="bytSchema"></param>
		/// <returns>DataSet.</returns>
		public static DataSet DecompressDataSet(byte[] bytDs, byte[] bytSchema)
		{
			MemoryStream ms = new MemoryStream(bytDs);
			Stream s2 = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream(ms);// Inflater(); // SharpZipLib.Zip.Compression.Streams.InflaterInputStream(ms);
			DataSet ds = new DataSet();
            
			if (bytSchema != null)
			{
				MemoryStream ms10 = new MemoryStream(bytSchema);
				Stream s10 = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream(ms10);// Inflater(); // SharpZipLib.Zip.Compression.Streams.InflaterInputStream(ms);
				ds.ReadXmlSchema(s10);
				ds.ReadXml(s2, XmlReadMode.DiffGram);
			}
			else
			{
				ds.ReadXml(s2);
			}
			SetDefaultDataTable(ds);
			return ds;
		}
        /// <summary>
        /// Compresses File Content
        /// </summary>
        /// <param name="ds">File Content to be compressed.</param>
        /// <returns>Byte array.</returns>
        public static byte[] CompressFileContent(byte[] blobContent)
        {

            MemoryStream ms2 = new MemoryStream(blobContent);
            StreamReader reader = new StreamReader(ms2);
            MemoryStream ms3 = new MemoryStream();
            
            
            string str = reader.ReadToEnd(); 

            ICSharpCode.SharpZipLib.Zip.Compression.Deflater defl = new ICSharpCode.SharpZipLib.Zip.Compression.Deflater(9 , false);
            ms2.Position = 0;
            Stream s = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream(ms3, defl);
            
            
            s.Write(ms2.ToArray(), 0,(int) ms2.Length);
            s.Close();
            
            
            return (byte[])ms3.ToArray();

        }
        
        /// <summary>
        /// Decompresses a byte array 
        /// </summary>
        /// <param name="bytDs">Byte array to be decompressed.</param>
        /// <returns>byte[].</returns>
        public static byte[] DeCompressFileContent(byte[] bytDs)
        {
            byte[] blobContent = new byte[100];
            MemoryStream ms = new MemoryStream(bytDs);
            ms.Position = 0;
            Stream s2 = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream(ms);// Inflater(); // SharpZipLib.Zip.Compression.Streams.InflaterInputStream(ms);
            blobContent = ReadFully(s2, 100);
            return (byte[]) blobContent;
        }
        /// <summary>
        /// Reads data from a stream until the end is reached. The
        /// data is returned as a byte array. An IOException is
        /// thrown if any of the underlying IO calls fail.
        /// </summary>
        /// <param name="stream">The stream to read data from</param>
        /// <param name="initialLength">The initial buffer length</param>
        public static byte[] ReadFully(Stream stream, int initialLength)
        {
            // If we've been passed an unhelpful initial length, just
            // use 32K.
            if (initialLength < 1)
            {
                initialLength = 32768;
            }

            byte[] buffer = new byte[initialLength];
            int read = 0;

            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;

                // If we've reached the end of our buffer, check to see if there's
                // any more information
                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();

                    // End of stream? If so, we're done
                    if (nextByte == -1)
                    {
                        return buffer;
                    }

                    // Nope. Resize the buffer, put in the byte we've just
                    // read, and continue
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            // Buffer is now too big. Shrink it.
            byte[] ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }


	
	}
}
