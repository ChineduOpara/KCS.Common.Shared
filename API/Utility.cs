using System;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Data;
using System.Globalization;
using System.Collections;
using System.Diagnostics;
using System.Web;
using System.ComponentModel;
using System.Net;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Reflection;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Contains general utility methods. Most of these are legacy methods (pre .Net 2.0), obsolete, or just not well-written.
    /// Most can be moved to other classes. After moving them, keep all the data-related methods here and rename the file to "Data.cs".
    /// </summary>
    public static class Utility
    {        
        private static object _Lock = new object();
        private static List<Type> _numericTypes;


        #region Properties
        /// <summary>
        /// Contains the list of numeric types.
        /// </summary>
        public static List<Type> NumericTypes
        {
            get
            {
                if (_numericTypes == null)
                {
                    lock (_Lock)
                    {
                        _numericTypes = new List<Type>()
						{
							typeof(System.Byte),
							typeof(System.SByte),
							typeof(System.Int16),
							typeof(System.UInt16),
							typeof(System.Int32),
							typeof(System.UInt32),
							typeof(System.Int64),
							typeof(System.UInt64),
							typeof(System.Single),
							typeof(System.Double),
							typeof(System.Decimal)
						};
                    }
                }
                return _numericTypes;
            }
        }        
        #endregion        

        //public static T IsValidEnum<T>(string value) where T:struct
        //{
        //    T roleValue;
        //    bool success = Enum.TryParse<T>(value, true, out roleValue);
        //    //Enum.IsDefined(typeof(T), roleName))

        //    return roleValue;
        //}

        public static bool IsValidEnum<T>(string value) where T : struct
        {
            value = GetStringValue(value);
            value = value.ToLower();
            var names = Enum.GetNames(typeof(T)).Select(x => x.ToLower());
            return names.Contains(value);
        }

        public static bool CanChangeType(object value, Type targetType)
        {
            if (targetType == null)
            {
                return false;
            }

            if (value == null)
            {
                return false;
            }

            IConvertible convertible = value as IConvertible;

            if (convertible == null)
            {
                return false;
            }

            return true;
        }

        ///// <summary>
        ///// Transforms an XML document using XSLT string from a file.
        ///// </summary>
        ///// <param name="xml">XML Document.</param>
        ///// <param name="xsl">XSL Document</param>
        ///// <returns></returns>
        //public static string GetXslTransform(string xml, Uri xslUri)
        //{
        //    StreamReader sr = null;
        //    try
        //    {
        //        sr = new StreamReader(xslUri.AbsolutePath);
        //        return GetXslTransform(xml, sr.ReadToEnd());
        //    }
        //    finally
        //    {
        //        if (sr != null)
        //        {
        //            sr.Close();
        //            sr = null;
        //        }
        //    }
        //}

        ///// <summary>
        ///// Transforms an XML document using XSLT string.
        ///// </summary>
        ///// <param name="xml">XML Document.</param>
        ///// <param name="xsl">XSL Document</param>
        ///// <returns></returns>
        //public static string GetXslTransform(string xml, string xsl)
        //{
        //    try
        //    {
        //        StringReader srXml = new StringReader(xml);
        //        StringReader srXsl = new StringReader(xsl);
        //        StringBuilder sb = new StringBuilder();

        //        // Load the Xml document
        //        XPathDocument xPathDoc = new XPathDocument(srXml);

        //        // Load the Xsl Transform document
        //        XmlDocument xslDoc = new XmlDocument();
        //        xslDoc.LoadXml(xsl);

        //        XslTransform myXslTrans = new XslTransform();
        //        myXslTrans.Load(xslDoc);

        //        // Create the output stream
        //        TextWriter myWriter = new StringWriter(sb);

        //        // Perform the actual transform
        //        myXslTrans.Transform(xPathDoc, null, myWriter);
        //        myWriter.Close();
        //        return sb.ToString();
        //    }
        //    catch (Exception e)
        //    {
        //        return string.Empty;
        //    }
        //}

        /// <summary>
        /// Gets a resource whose name matches a given string. This assumes that the resource file is the "Properties" resource file.
        /// </summary>
        /// <typeparam name="T">Resource type.</typeparam>
        /// <param name="type">Type.</param>
        /// <param name="resourceName">Resource name (key).</param>
        /// <returns>If resource not found, returns null.</returns>
        public static T GetResource<T>(Type type, string resourceName)
        {
            Assembly asm = Assembly.GetAssembly(type);
            string[] resourceFiles = asm.GetManifestResourceNames();
            string targetResFile = resourceFiles.Where(x => x.Contains("Properties.Resources")).FirstOrDefault();
            targetResFile = targetResFile.Replace(".resources", "");	// Remove the extension

            return GetResource<T>(asm, targetResFile, resourceName);
        }

        /// <summary>
        /// Gets a resource whose name matches a given string.
        /// </summary>
        /// <typeparam name="T">Resource type.</typeparam>
        /// <param name="asm">Assembly.</param>
        /// <param name="resourceFile">Resource file base name.</param>
        /// <param name="resourceName">Resource name (key).</param>
        /// <returns>If resource not found, returns null.</returns>
        public static T GetResource<T>(Assembly asm, string resourceFile, string resourceName)
        {
            System.Resources.ResourceManager rm = new System.Resources.ResourceManager(resourceFile, asm);
            var items = rm.GetResourceSet(CultureInfo.CurrentCulture, true, true);
            try
            {
                object obj = items.GetObject(resourceName, true);

                // TODO: Type converstion test, with custom exception if it fails

                return (T)obj;
            }
            catch
            {
                return default(T);	// if resource not found, return null.
            }
        }

        /// <summary>
        /// Checks that a Type is a numeric type.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <returns>True or false.</returns>
        public static bool IsNumeric(this Type type)
        {
            return NumericTypes.Contains(type);
        }        

        /// <summary>
        /// Just like it sounds! Translation from minutes to milliseconds happens automatically.
        /// </summary>
        /// <param name="ws">Web Service.</param>
        /// <param name="minutes">Timeout, in minutes.</param>
        public static void SetWebServiceTimeout(System.Web.Services.Protocols.WebClientProtocol ws, int minutes)
        {
            ws.Timeout = minutes * 1000 * 60;
        }

        #region DataTable Manipulation
        /// <summary>
        /// Sets up a Primary Key for the specified DataTable
        /// </summary>
        /// <param name="dt">DataTable that needs a primary key setup</param>
        /// <param name="tableName">Name of table</param>
        /// <param name="primaryKey">string[] Primary Key Fields</param>
        /// <returns></returns>
        public static DataTable SetPrimaryKey(DataTable dt, string tableName, string[] primaryKey)
        {
            dt.TableName = tableName;
            if (primaryKey.Length > 0)
            {
                DataColumn[] dc = new DataColumn[primaryKey.Length];
                for (int i = 0; i < primaryKey.Length; i++)
                {
                    dc[i] = dt.Columns[primaryKey[i]];
                }
                dt.PrimaryKey = dc; //new DataColumn[primaryKey.Length]{ds.Tables[0].Columns["P_L_ID"],ds.Tables[0].Columns["P_ID"]};
            }
            return dt;
        }

        #endregion

        #region In-line SQL manipulation
        /// <summary>
        /// Converts wild card characters to ANSI-92 SQL characters (for example: * to %)
        /// and add quotes
        /// </summary>
        /// <param name="val"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public static string ConvertToANSI92SQL(string val, bool upper)
        {
            if (val.IndexOf("*") >= 0)
            {
                if (upper) val = val.ToUpper();
                val = " like '" + AddQuote2(val.Trim().Replace("*", "%")) + "'";
            }
            else
            {
                val = " = '" + AddQuote2(val.Trim()) + "'";
            }
            return val;
        }

        /// <summary>
        /// Adds quotes to in-line SQL code.
        /// </summary>
        /// <param name="inStr"></param>
        /// <returns></returns>
        #endregion

        #region Imported from PreCosting. Undocumented methods.
        public static object[][] HashToArray(Hashtable ht)
        {
            object[][] obj = new object[ht.Count][];
            int i = 0;
            foreach (object key in ht.Keys)
            {
                obj[i] = new object[] { key, ht[key] };
                i++;
            }
            return obj;
        }
        public static Hashtable ArrayToHash(object[][] obj)
        {
            Hashtable ht = new Hashtable(obj.Length);
            foreach (object[] pair in obj)
            {
                object key = pair[0];
                object value = pair[1];
                ht[key] = value;
            }
            return ht;
        }

        public static string AddQuote2(string inStr)
        {
            string[] s = inStr.Split('\'');
            if (s.Length <= 1) return inStr;

            System.Text.StringBuilder s1 = new System.Text.StringBuilder(inStr.Length);
            int i;
            for (i = 0; i < s.Length - 1; i++)
            {
                s1 = s1.Append(s[i]);
                s1.Append('\'');
                s1.Append('\'');
            }
            s1.Append(s[i]);
            return s1.ToString();
        }

        public static bool IsNumeric(object n)
        {
            try
            {
                double d = System.Double.Parse(n.ToString(), NumberStyles.Any);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool IsNumeric(object n, CultureInfo ci)
        {
            try
            {
                double d = System.Double.Parse(n.ToString(), NumberStyles.Any, ci);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static string FormatDBColumnName(string inStr)
        {
            char[] s = inStr.ToUpper().ToCharArray();
            char c;
            for (int i = 0; i < s.Length; i++)
            {
                c = s[i];
                if (c != '_' && !(c >= '0' && c <= '9') && !(c >= 'A' && c <= 'Z')) s[i] = '_';
            }

            if (s[0] >= '0' && s[0] <= '9') return "_" + new string(s, 0, s.Length);
            return new string(s, 0, s.Length);
        }

        public static bool IsDateTimeValid(string dateTime)
        {
            try
            {
                Convert.ToDateTime(dateTime);
            }
            catch (System.FormatException)
            {
                return false;
            }
            return true;
        }

        public static bool IsDateTimeValid(string dateTime, string dateFormat)
        {
            string day, mo, yr;

            if (dateFormat == "DD/MM/YY")
            {
                day = dateTime.Substring(0, 2);
                mo = dateTime.Substring(3, 2);
                yr = dateTime.Substring(6, 2);
                dateTime = mo + "/" + day + "/" + yr;
            }

            try
            {
                Convert.ToDateTime(dateTime);
            }
            catch (System.FormatException)
            {
                return false;
            }
            return true;
        }

        public static string GetRandomPassword(int length, int seed)
        {
            string s = "";
            string choice = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            long ticks = DateTime.Now.Ticks;
            int t = (int)(ticks % 10000) + seed;

            Random r = new Random(t);
            for (int i = 0; i < length; i++)
            {
                s = s + choice.Substring(r.Next(0, choice.Length), 1);
            }

            return s;
        }

        public static bool IsInteger(string n)
        {
            try
            {
                int i = Int32.Parse(n);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }        
        public static object IntNullToDBNull(string v)
        {
            if (v == null || v.Equals(string.Empty)) return System.DBNull.Value;
            return Convert.ToInt32(v);
        }
        public static object StringNullToDBNull(string v)
        {
            if (v == null || v.Equals(string.Empty)) return System.DBNull.Value;
            return v;
        }
        public static object DecimalNullToDBNull(string v)
        {
            if (v == null || v.Equals(string.Empty)) return System.DBNull.Value;
            return Convert.ToDecimal(v);
        }
        public static object StringNullToEmpty(string v)
        {
            if (v == null) return String.Empty;
            return v;
        }
        public static string StringBlankNullToEmpty(object v)
        {
            if (v == null || v.ToString().Trim().Length == 0) return String.Empty;
            return v.ToString();
        }
        // will only trim zeros, if end is not zero, it will not do anything
        public static string TrimEndingZeroFromNumber(string inNumber, int decimalPlaces)
        {
            string[] number = inNumber.Split('.');
            if (number.Length != 2) return inNumber;
            string temp = number[1].Substring(decimalPlaces);
            temp = temp.Replace('0', ' ').Trim();
            if (temp.Length > 0) return inNumber;
            if (decimalPlaces == 0) return number[0];
            return number[0] + "." + number[1].Substring(0, decimalPlaces);
        }
        public static string GetTableData(DataTable dt, string fieldName, int decimalPlaces)
        {
            return TrimEndingZeroFromNumber(GetTableData(dt, fieldName), decimalPlaces);
        }
        public static string GetTableData(DataTable dt, string fieldName)
        {
            if (dt == null || dt.Rows.Count == 0) return string.Empty;
            return GetRowData(dt.Rows[0], fieldName);
        }

        public static string GetRowData(DataRow dr, string fieldName, int decimalPlaces)
        {
            return TrimEndingZeroFromNumber(GetRowData(dr, fieldName), decimalPlaces);
        }

        public static string GetRowData(DataRow dr, string fieldName)
        {
            if (dr == null) return string.Empty;

            object d = DBNull.Value;
            if (dr.Table.Columns.Contains(fieldName))
            {
                d = dr[fieldName];
            }
            if (d != DBNull.Value) return d.ToString();
            return string.Empty;
        }
        public static string GetTableDataDateTime(DataTable dt, string fieldName)
        {
            if (dt == null || dt.Rows.Count == 0) return string.Empty;
            return GetRowDataDateTime(dt.Rows[0], fieldName);
        }
        public static string GetRowDataDateTime(DataRow dr, string fieldName)
        {
            if (dr == null) return string.Empty;
            object d = dr[fieldName];
            if (d != DBNull.Value)
            {
                DateTime dt = Convert.ToDateTime(d);
                return dt.ToShortDateString();
            }
            return string.Empty;
        }
        public static object LongToNull(long v)
        {
            if (v == long.MinValue) return System.DBNull.Value;
            return v;
        }
        public static object IntToNull(int v)
        {
            if (v == int.MinValue) return System.DBNull.Value;
            return v;
        }
        public static object IntToNull(decimal v)
        {
            if (v == decimal.MinValue) return System.DBNull.Value;
            return v;
        }

        public static string JoinPath(string path1, string path2)
        {
            if (path1.EndsWith("\\")) return path1 + path2;
            return path1 + "\\" + path2;
        }

        public static string EncodedUnicodeToChar(string s, string delimiter)
        {
            string ss = "";
            int i, j, k;

            k = delimiter.Length;

            i = s.IndexOf(delimiter);
            while (i >= 0)
            {
                ss = s.Substring(i + k, 4);
                try
                {
                    j = Int32.Parse(ss, NumberStyles.HexNumber);
                    s = s.Substring(0, i) + Convert.ToChar(j) + s.Substring(i + k + 4);
                }
                catch (Exception)
                {
                }
                i = s.IndexOf(delimiter, ++i);
            }
            return s;
        }

        public static string StringTrimLtRtChar(string strIn, string lt, string rt)
        {
            try
            {
                if (strIn.StartsWith(lt)) strIn = strIn.Remove(0, lt.Length);
                if (strIn.EndsWith(rt)) strIn = strIn.Remove(strIn.LastIndexOf(rt), rt.Length);

            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
            return strIn;
        }

        public static string StringTrim(string strIn)
        {
            string st = "";

            if (strIn == null) return st;
            else
            {
                st = strIn.ToString();
                return st.Trim();
            }
        }

        public static string SubString(string delStrCol, char delimiter, int strCount)
        {
            //default start position is from the first string
            int count = 0;
            return SubString(delStrCol, delimiter, strCount, ref count);
        }

        public static string SubString(string strCol, char delimiter, int ItemCount, ref int startPosition)
        {
            //use this function only when the itemcount <1050
            int counter = 0;
            if (strCol.Length < 1)
            {
                return "";
            }
            else
            {
                while (true)
                {
                    if (strCol.Length < startPosition)
                    {
                        return strCol;
                    }

                    startPosition = strCol.IndexOf(delimiter, startPosition);

                    counter++;
                    if (startPosition < 0)
                    {
                        return strCol;
                    }
                    else if (counter == ItemCount)
                    {
                        return strCol.Substring(0, startPosition);
                    }
                    startPosition++;

                }
            }
        }

        public static List<string> GetStringList(string EntireStr, char delimiter, int ItemCount)
        {
            List<string> strList = new List<string>();
            int startPosition = 0;//start from first item
            string tempstr = "";
            while (true)
            {
                tempstr = SubString(EntireStr, delimiter, ItemCount, ref startPosition); ;
                if (tempstr != string.Empty)
                {
                    strList.Add(tempstr);
                    if (startPosition > -1)
                    {
                        EntireStr = EntireStr.Substring(startPosition + 1);
                        startPosition = 0;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }


            return strList;
        }

        public static string CallASPPage(string url)
        {
            string data = "";
            try
            {
                WebRequest req = WebRequest.Create(url);
                req.ContentType = "application/x-www-form-urlencoded";
                req.Credentials = CredentialCache.DefaultCredentials;

                WebResponse response = req.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader sreader = new StreamReader(stream);
                data = sreader.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw ex; //PreCostingLog.LogError(ex.Message, "Error encountered in " + new StackFrame().GetMethod().Name + " url=" + url);
            }
            return data;
        }

        public static string LastPartOfName(string aName)
        {
            if (aName == null) return string.Empty;

            int j = -1;
            char[] ch = aName.ToCharArray();
            for (int i = ch.Length - 1; i >= 0; i--)
            {
                if (ch[i] >= 'A' && ch[i] <= 'Z' || ch[i] >= 'a' && ch[i] <= 'z' || ch[i] >= '0' && ch[i] <= '9' || ch[i] == '/') // is alpha
                {
                    j = i;
                }
                else
                {
                    break;
                }
            }
            if (j > -1)
            {
                return aName.Substring(j, aName.Length - j);
            }
            return string.Empty;
        }

        public static string ChkNum(string n, int digits, int decimals, string fldName)
        {
            double v;

            if (n == null || n.Trim() == string.Empty) return string.Empty;
            try
            {
                v = Convert.ToDouble(n);
            }
            catch (Exception)
            {
                return "Value " + n + " of field " + fldName + " is not a number.";
            }
            string[] nn = n.Split('.');  //may need to be localized

            if (nn.Length == 2) // got decimail points
            {
                if (nn[1].Length > decimals)
                {
                    int actualDecLen = (v - Convert.ToDouble(nn[0])).ToString().Length - 2;  // value 1.20 should have decimal len = 1 instead of 2 because the zero is insignificant
                    if (actualDecLen > decimals)
                    {
                        return "Value " + v + " of field " + fldName + " can have only " + decimals.ToString() + " decimal place(s).";
                    }
                }
            }

            double maxV;
            if (decimals > 0)
            {
                maxV = Math.Pow(10, digits - decimals) - 1 + ((Math.Pow(10, decimals) - 1) / Math.Pow(10, decimals));
            }
            else
            {
                maxV = Math.Pow(10, digits) - 1;
            }

            if (v > maxV || v < -maxV)
            {
                return ("Value " + v + " of field " + fldName + " is not between " + maxV + " and " + "-" + maxV);
            }

            return string.Empty;
        }

        public static string ChkNum(string n, int digits)
        {
            double v;
            if (n == null || n.Trim() == string.Empty) return string.Empty;
            try
            {
                v = Convert.ToDouble(n);
            }
            catch (Exception)
            {
                return "Value " + n + " is not a number.";
            }
            return string.Empty;
        }

        public static string FormatDateForSQL(string dateTime)
        {
            string sRet = "";
            try
            {
                if (dateTime != "")
                {
                    DateTime dt = Convert.ToDateTime(dateTime);
                    sRet = dt.ToString("dd MMM yyyy");
                }
            }
            catch (System.FormatException)
            {
                return "Error: " + dateTime + " is not a valid date.";
            }
            return sRet;
        }

        public static string FormatDateForSQL(string dateTime, string dateFormat)
        {
            string sRet = "";
            string day, mo, yr;

            if (dateFormat == "DD/MM/YY")
            {
                day = dateTime.Substring(0, 2);
                mo = dateTime.Substring(3, 2);
                yr = dateTime.Substring(6, 2);
                dateTime = mo + "/" + day + "/" + yr;
            }
            try
            {
                if (dateTime != "")
                {
                    DateTime dt = Convert.ToDateTime(dateTime);
                    sRet = dt.ToString("dd MMM yyyy");
                }
            }
            catch (System.FormatException)
            {
                return "Error: " + dateTime + " is not a valid date.";
            }
            return sRet;
        }

        public static string FormatDateForSQL4DigitYr(string dateTime, string dateFormat)
        {
            string sRet = "";
            string day, mo, yr;

            if (dateFormat == "DD/MM/YY")
            {
                string[] dts = dateTime.Split('/');
                day = dts[0].ToString();
                mo = dts[1].ToString();
                yr = dts[2].ToString();
                dateTime = mo + "/" + day + "/" + yr;
            }
            try
            {
                if (dateTime != "")
                {
                    DateTime dt = Convert.ToDateTime(dateTime);
                    sRet = dt.ToString("dd MMM yyyy");
                }
            }
            catch (System.FormatException)
            {
                return "Error: " + dateTime + " is not a valid date.";
            }
            return sRet;
        }

        public static bool IsDateRangeValid(string BeginDate, string EndDate)
        {
            bool ret = true;
            if (BeginDate.Trim().Length > 0 && EndDate.Trim().Length > 0)
            {
                if (Convert.ToDateTime(EndDate) <= Convert.ToDateTime(BeginDate))
                    ret = false;
            }
            return ret;
        }

        public static string SqlUpdateFormat(string sql, string fld, string val, string datatype)
        {
            switch (datatype)
            {
                case "D": //date
                    {
                        if (val.ToLower() == "sysdate")
                        {
                            sql += fld + "=sysdate,";
                        }
                        else
                        {
                            sql += (val.Trim() != string.Empty) ? fld + "='" + FormatDateForSQL(val.Trim()) + "'," : fld + "=null,";
                        }
                        break;
                    }
                case "S": //string
                    {
                        sql += (val.Trim() != string.Empty) ? fld + "='" + val.Trim() + "'," : fld + "=null,";
                        break;

                    }
                case "N": //numeric
                    {
                        sql += (val.Trim() != string.Empty) ? fld + "=" + val.Trim() + "," : fld + "=null,";
                        break;
                    }
            }
            return sql;
        }

        /// <summary>
        /// Determine whether to enclose a varibale with a single quote 
        /// or double quote or without quote based on the input datatype
        /// </summary>
        /// <param name="inputValue">Input string which is to be enclosed by quotes</param>
        /// <param name="dataType">Date type of the input string</param>
        /// <returns>String enclosed with quotes</returns>
        public static string AddQuotes(string inputValue, string dataType)
        {
            string quotes = String.Empty;
            string dateFormat = "YYYY/MM/DD";
            string outputValue = String.Empty;

            try
            {
                if (dataType.Equals("NUMERIC"))
                {
                    quotes = "";
                }
                else
                {
                    quotes = "'";
                }

                if (dataType.Equals("CHARACTER"))
                {
                    if ((inputValue.IndexOf("'") > 0) && (inputValue.IndexOf("''") <= 0))
                    {
                        inputValue.Replace("'", "''");
                    }
                }
                outputValue = quotes + inputValue + quotes;

                if (dataType.Equals("DATE"))
                {
                    outputValue = "to_date(" + outputValue + ",'" + dateFormat + "')";
                }
            }
            catch (Exception exception)
            {
                throw exception; //PreCostingLog.LogError(exception.Message, "Error encountered in " + new StackFrame().GetMethod().Name);
            }
            return outputValue;
        }

        /// <summary>
        /// This function check the occurance of a specific fields value in the string 
        /// representating list of values for a specific fileds all concatenated 
        /// by a delimiters.
        /// </summary>
        ///  <param name="fieldList">Name of the filed for which this value corresponds to</param>
        /// <param name="fieldValue">The value of the specific field which needs to be checked for uniqueness</param>
        /// <returns>Returns true if the field value is unique</returns>
        public static bool CheckUniqueness(object fieldList, object fieldValue)
        {
            bool returnValue = true;
            string tempValues = String.Empty;
            try
            {
                if (fieldValue.Equals(System.DBNull.Value)) tempValues = "";
                else tempValues = fieldValue.ToString();
                if (Convert.ToString(tempValues + "").Equals("")) return false;

                if (fieldList.ToString().Length > 0)
                {
                    if (fieldList.ToString().Substring(0, 1) != "~") fieldList = "~" + fieldList.ToString();
                    if (fieldList.ToString().Substring(fieldList.ToString().Length - 1, 1) != "~")
                        fieldList = fieldList.ToString() + "~";

                    tempValues = "~" + tempValues + "~";
                    fieldList = " " + fieldList.ToString();
                    if (fieldList.ToString().IndexOf(tempValues) > 0) returnValue = false;
                }
            }
            catch (Exception exception)
            {
                throw exception; //PreCostingLog.LogError(exception.Message, "Error encountered in " + new StackFrame().GetMethod().Name);
            }
            return returnValue;
        }
        /// <summary>
        /// Truncate input string from left and right side.
        /// </summary>
        /// <param name="inputString">Input string</param>
        /// <param name="leftString">Truncate input string from left until leftString is matched</param>
        /// <param name="rightString">Truncate input string from right until rightString is matched</param>
        public static string TrimSpecialCharacter(string leftString, string rightString, string inputString)
        {
            try
            {

                if (inputString.StartsWith(leftString))
                    inputString = inputString.Substring(leftString.Length - 1);
                if (inputString.EndsWith(rightString))
                    inputString = inputString.Substring(0, inputString.Length - rightString.Length);

            }
            catch (Exception exception)
            {
                throw exception;
            }
            return inputString;
        }
        /// <summary>
        /// Truncate input string from left and right side.
        /// </summary>
        /// <param name="inputString">Input string</param>
        /// <param name="leftString">Truncate input string from left until leftString is matched</param>
        /// <param name="rightString">Truncate input string from right until rightString is matched</param>
        public static string TrimCharacter(string leftString, string rightString, string inputString)
        {
            try
            {
                if (inputString.StartsWith(leftString))
                    inputString = inputString.Substring(leftString.Length);
                if (inputString.EndsWith(rightString))
                    inputString = inputString.Substring(0, inputString.Length - rightString.Length);
            }
            catch (Exception exception)
            {
                throw exception; //PreCostingLog.LogError(exception.Message, "Error encountered in " + new StackFrame().GetMethod().Name);
            }
            return inputString;
        }

        /// <summary>
        /// PURPOSE: This fnction will extract the file extension from a file name
        /// </summary>
        /// <param name="fileNameIn"></param>
        /// <returns></returns>
        public static string ExtractFileExtension(ref string fileName)
        {
            string fileExtension = string.Empty;
            try
            {
                if (fileName.Length > 0)
                {
                    int dotPosition = fileName.LastIndexOf(".", 0);
                    if (dotPosition > 0)
                        fileExtension = fileName.Substring(dotPosition);
                }
            }
            catch (Exception exception)
            {
                throw exception; //PreCostingLog.LogError(exception.Message, "Error encountered in " + new StackFrame().GetMethod().Name);
            }
            return fileExtension;
        }

        public static void SplitStringAtDelim(string src, string delim, ref string leftPart, ref string rightPart)
        {
            leftPart = "";
            rightPart = "";
            try
            {
                int posDelim = src.IndexOf(delim);
                if (posDelim > 0)
                {
                    leftPart = src.Substring(0, posDelim);
                    rightPart = src.Substring(posDelim + delim.Length);
                }
                else
                {
                    leftPart = src;
                }
            }
            catch (Exception)
            {
            }
        }

        public static string OracleWildCard(string val, bool upper)
        {
            if (val.IndexOf("*") >= 0)
            {
                if (upper) val = val.ToUpper();
                val = " like '" + AddQuote2(val.Trim().Replace("*", "%")) + "'";
            }
            else
            {
                val = " = '" + AddQuote2(val.Trim()) + "'";
            }
            return val;
        }

        /// <summary>
        /// Sets an Oracle where condition.  e.g. where field = 'abc' or where field like 'abc%'
        /// </summary>
        /// <param name="val">The actual value to determine using '=' or like  </param>
        /// <param name="upper">Upper the value</param>
        /// <param name="variableName">The name of the variable</param>
        /// <returns>the where condition</returns>
        public static string OracleWildCardVariableName(ref string val, bool upper, string variableName)
        {
            string where = "";
            if (upper) val = val.ToUpper();

            if (val.IndexOf("*") >= 0)
            {
                val = val.Trim().Replace("*", "%");
                where = " like ";
            }
            else
            {
                where = " = ";
            }

            return where + "{" + variableName + "}";
        }

        public static string LeftStr(string s)
        {
            string result = "";
            string[] ss = s.Split(',');

            if (ss.GetLength(0) != 2)
                throw new ArgumentException("Requires 2 arguments.");

            string s1 = ss[0].ToString();
            int len;
            if (int.TryParse(ss[1], out len))
            {
                int availLen;
                if (len > s1.Length)
                {
                    availLen = s1.Length;
                }
                else
                {
                    availLen = len;
                }
                result = s1.Substring(0, availLen);
            }
            return result;
        }

        /// <summary>
        /// test if two columns are changed.  empty string equals null and DBNull.Value.  byte[] are always considered not changed
        /// </summary>
        /// <param name="one"></param>
        /// <param name="two"></param>
        /// <returns></returns>
        public static bool ColChanged2(object one, object two)
        {
            if (!IsEmpty(one) && IsEmpty(two) || IsEmpty(one) && !IsEmpty(two)) return true;
            if (!IsEmpty(one) && !IsEmpty(two))
            {
                if (one.GetType() == typeof(System.Decimal))
                {
                    try
                    {
                        decimal oneDec = Convert.ToDecimal(one);
                        decimal twoDec = Convert.ToDecimal(two);
                        if (oneDec != twoDec) return true;
                    }
                    catch (Exception /*ex*/) // if crash, they are not the same
                    {
                    }
                }
                else
                {
                    if (one.ToString() != two.ToString()) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// test if two columns are changed.  empty string is NOT null and not DBNull.Value.  byte[] are always considered not changed
        /// </summary>
        /// <param name="one"></param>
        /// <param name="two"></param>
        /// <returns></returns>
        public static bool ColChanged(object one, object two)
        {
            return ColChanged(one, two, false);
        }

        /// <summary>
        /// test if two columns are changed.  empty string is NOT null and not DBNull.Value.  byte[] are always considered not changed
        /// </summary>
        /// <param name="one"></param>
        /// <param name="two"></param>
        /// <returns></returns>
        public static bool ColChanged(object one, object two, bool onlyNonBlankData_two)
        {
            if (onlyNonBlankData_two && two.ToString().Length == 0) return false;
            if (one != DBNull.Value && two == DBNull.Value || one == DBNull.Value && two != DBNull.Value) return true;
            if (one != DBNull.Value && two != DBNull.Value)
            {
                if (one.GetType() == typeof(System.Decimal))
                {
                    try
                    {
                        decimal oneDec = Convert.ToDecimal(one);
                        decimal twoDec = Convert.ToDecimal(two);
                        if (oneDec != twoDec) return true;
                    }
                    catch (Exception /*ex*/) // if crash, they are not the same
                    {
                    }
                }
                else
                {
                    if (one.ToString() != two.ToString()) return true;
                }
            }
            return false;
        }

        public static Hashtable DataChanged(DataTable dtSeas, Hashtable ht, string excludeCols)
        {
            return DataChanged(dtSeas, ht, excludeCols, null);
        }

        public static Hashtable DataChanged(DataTable dtSeas, Hashtable ht, string excludeCols, List<string> incOnlyCols)
        {
            return DataChanged(dtSeas, ht, excludeCols, incOnlyCols, false);
        }

        public static Hashtable DataChanged(DataTable dtSeas, Hashtable ht, string excludeCols, List<string> incOnlyCols, bool onlyNonBlankData)
        {
            Hashtable htChanged = new Hashtable(ht.Keys.Count);
            string ex = excludeCols.ToUpper() + ",";
            foreach (string s in ht.Keys)
            {
                if ((excludeCols.Length > 0 && ex.IndexOf(s.ToUpper() + ",") >= 0)) continue;
                if (incOnlyCols != null && !incOnlyCols.Contains(s)) continue;

                if (dtSeas.Columns.Contains(s))
                {
                    if (dtSeas.Columns[s].DataType == typeof(System.Decimal))  // take care of rounding errors by storing the actual value in the table column then check
                    {
                        object orgValue = dtSeas.Rows[0][s];
                        dtSeas.Rows[0][s] = ht[s];
                        object newValue = dtSeas.Rows[0][s];
                        if (ColChanged(orgValue, newValue, onlyNonBlankData))
                        {
                            htChanged.Add(s, ht[s]);
                            dtSeas.Rows[0][s] = orgValue;
                        }
                    }
                    else
                    {
                        if (ColChanged(dtSeas.Rows[0][s], ht[s], onlyNonBlankData))
                        {
                            htChanged.Add(s, ht[s]);
                        }
                    }
                }
            }
            return htChanged;
        }

        public static Hashtable DataChanged(DataTable dtSeas, Hashtable ht)
        {
            return DataChanged(dtSeas, ht, "", null, false);
        }

        public static Hashtable DataChanged(DataTable dtSeas, Hashtable ht, bool onlyNonBlankData)
        {
            return DataChanged(dtSeas, ht, "", null, onlyNonBlankData);
        }

        public static Hashtable DataChanged(DataTable dtSeas, Hashtable ht, List<string> incOnlyCols)
        {
            return DataChanged(dtSeas, ht, "", incOnlyCols, false);
        }

        public static bool DataChanged(DataRow dr1, DataRow dr2)
        {
            int colCnt = dr1.Table.Columns.Count;
            for (int i = 0; i < dr1.Table.Columns.Count; i++)
            {
                if (ColChanged(dr1[i], dr2[i])) return true;
            }
            return false;
        }

        public static void AssignIfEmpty(DataRow dr, string columnName, object data)
        {
            if (Utility.IsEmpty2(dr[columnName]))
            {
                dr[columnName] = data;
            }
        }

        public static void NullSet(Hashtable htCols, string key, object value)
        {
            if (!htCols.ContainsKey(key) || htCols[key] == DBNull.Value) htCols[key] = value;
        }

        // similar to Oracle' NVL.  Returns the 2nd if the first is null
        public static object NVL(object first, object second)
        {
            if (first == null || first == DBNull.Value) return second;
            return first;
        }

        public static T NVL2<T>(object first, T second)
        {
            if ((first == null) || (first == DBNull.Value))
            {
                return second;
            }

            return (T)first;
        }

        /// <summary>
        /// Test if object is empty.  Blank space is not empty.
        /// </summary>
        /// <param name="v">object</param>
        /// <returns>true if object is empty</returns>
        public static bool IsEmpty(object v)
        {
            return (v == null || v == DBNull.Value || v.ToString() == "");
        }
        /// <summary>
        /// Test if object is empty.  Blank space is considered empty.
        /// </summary>
        /// <param name="v">object</param>
        /// <returns>true if object is empty</returns>
        public static bool IsEmpty2(object v)
        {
            return (v == null || v == DBNull.Value || v.ToString().Trim().Length == 0);
        }
        public static bool IsEmptyDecimal(object v)
        {
            return (v == null || v == DBNull.Value || v.ToString().Trim().Length == 0 || DecimalParse(v.ToString()) == (decimal)0.0);
        }
        public static bool IsLstSelEmpty(System.Windows.Forms.ComboBox lst)
        {
            if (lst.SelectedIndex != -1)
            {
                if (!IsEmpty(lst.SelectedValue))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool IsLstSelEmpty(System.Windows.Forms.ListBox lst)
        {
            if (lst.SelectedIndex != -1)
            {
                foreach (DataRowView el in lst.SelectedItems)
                {
                    if (!IsEmpty(el.Row[0]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static string Chr(int p_intByte)
        {
            if ((p_intByte < 0) || (p_intByte > 255))
            {
                throw new ArgumentOutOfRangeException("p_intByte", p_intByte, "Must be between 1 and 255.");
            }
            byte[] bytBuffer = new byte[] { (byte)p_intByte };
            return System.Text.Encoding.GetEncoding(1252).GetString(bytBuffer);
        }
        public static int Asc(string p_strChar)
        {
            if ((p_strChar.Length == 0) || (p_strChar.Length > 1))
            {
                throw new ArgumentOutOfRangeException("p_strChar", p_strChar, "Must be a single character.");
            }
            char[] chrBuffer = { Convert.ToChar(p_strChar) };
            byte[] bytBuffer = System.Text.Encoding.GetEncoding(1252).GetBytes(chrBuffer);
            return (int)bytBuffer[0];
        }

        public static DateTime LastDayOfMonth(DateTime endPeriod)
        {
            return endPeriod.AddMonths(1).AddDays(-1);
        }

        public static string WhereEqual(DataRow dr, string fldName)
        {
            if (dr[fldName] == null || dr[fldName] == DBNull.Value)
            {
                return fldName + " is null";
            }
            if (dr[fldName].GetType() == typeof(System.Decimal))
            {
                return fldName + " = " + dr[fldName].ToString();
            }
            return fldName + "='" + dr[fldName].ToString() + "'";
        }

        public static void SetColToNull(DataRow dr, string colName)
        {
            if (colName != null && colName != "") dr[colName] = DBNull.Value;
        }

        public static bool GetBoolValue(object v)
        {
            if (IsEmpty(v))
            {
                return false;
            }
            else
            {
                return Convert.ToBoolean(v);
            }
        }

        //public static bool GetBoolValue(object v)
        //{
        //    if (IsEmpty(v))
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return Convert.ToBoolean(v);
        //    }
        //}

        /// <summary>
        /// Returns TRUE if the given string is a valid Byte. If not, returns the default.
        /// </summary>
        /// <param name="string">String to check.</param>
        /// <param name="default">Default value to return.</param>
        /// <returns>Value of string as an Byte.</returns>
        public static byte GetByte(object obj, byte @default)
        {
            byte v = 0;
            if (!IsEmpty(obj))
            {
                v = Convert.ToByte(obj);
            }
            return v;
        }

        /// <summary>
        /// Returns an Int value or zero if null
        /// </summary>
        /// <param name="colVal"></param>
        /// <returns></returns>
        public static int GetInt32(object value)
        {
            int result;
            if (!IsEmpty(value) && int.TryParse(value.ToString(), out result))
            {
                return result;
            }
            return 0;
        }

        /// <summary>
        /// Returns an unsigned Int value or zero if null
        /// </summary>
        /// <param name="colVal"></param>
        /// <returns></returns>
        public static uint GetUInt32(object colVal)
        {
            uint v = 0;
            if (!IsEmpty(colVal))
            {
                v = Convert.ToUInt32(colVal);
            }
            return v;
        }

        public static bool IsPrimeNumber(Int64 number)
        {
            // Test whether the parameter is a prime number.
            if ((number & 1) == 0)
            {
                return number == 2;
            }

            // Note:
            // ... This version was changed to test the square.
            // ... Original version tested against the square root.
            // ... Also we exclude 1 at the end.
            for (int i = 3; (i * i) <= number; i += 2)
            {
                if ((number % i) == 0)
                {
                    return false;
                }
            }
            return number != 1;
        }

        /// <summary>
        /// Returns a long value.  Return zero if null
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static long GetInt64(object val)
        {
            long v = 0;
            if (!IsEmpty(val))
            {
                v = long.Parse(val.ToString(),  NumberStyles.Any);
            }
            return v;
        }

        /// <summary>
        /// Returns unsingled long value.  Return zero if null
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static ulong GetUInt64(object val)
        {
            ulong v = 0;
            if (!IsEmpty(val))
            {
                v = ulong.Parse(val.ToString(), NumberStyles.Any);
            }
            return v;
        }

        /// <summary>
        /// returns a short value.  Return zero if null
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static short GetInt16(object val)
        {
            short v = 0;
            if (!IsEmpty(val))
            {
                v = short.Parse(val.ToString(), NumberStyles.Any);
            }
            return v;
        }

        /// <summary>
        /// returns unsingned short, or zero if null.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static ushort GetUInt16(object val)
        {
            ushort v = 0;
            if (!IsEmpty(val))
            {
                v = ushort.Parse(val.ToString(), NumberStyles.Any);
            }
            return v;
        }

        /// <summary>
        /// returns a Double value.  Return zero if null
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static double GetDoubleValue(object val)
        {
            double v = 0;
            if (!IsEmpty(val))
            {
                v = double.Parse(val.ToString(), NumberStyles.Any);
            }
            return v;
        }

        /// <summary>
        /// Returns an date or null
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime? GetDateTimeValue(object value)
        {
            DateTime? v = null;
            if (!IsEmpty(value))
            {
                v = Convert.ToDateTime(value);
            }
            return v;
        }

        public static decimal DecimalParse(string v)
        {
            decimal d;
            try
            {
                d = Decimal.Parse(v, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign);
            }
            catch (Exception)
            {
                d = Decimal.Parse(v, System.Globalization.NumberStyles.Number);
            }
            return d;

        }

        public static decimal ConvertToDecimal(string val)
        {
            string dec = val.Trim();
            string aChar = dec.Substring(0, 1);
            if (aChar != "." && !(char.IsDigit(aChar, 0)))
            {
                dec = dec.Replace(aChar, "");
            }
            dec = dec.Replace("%", "");
            return GetDecimalValue(dec);

        }

        public static decimal GetDecimalValue(object colVal)
        {
            decimal v = 0;
            if (!IsEmpty(colVal))
            {
                v = DecimalParse(colVal.ToString());
            }
            return v;

        }

        public static decimal GetDecimalValue(object colVal, CultureInfo ci)
        {
            decimal v = 0;
            if (!IsEmpty(colVal))
            {
                v = Decimal.Parse(colVal.ToString(), NumberStyles.Any, ci);
            }
            return v;
        }

        /// <summary>
        /// Gets a string representation of bytes.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string FormatBytes(float bytes)
        {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = 0;
            for (i = 0; (int)(bytes / 1024) > 0; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }
            return String.Format("{0:0.00} {1}", dblSByte, Suffix[i]);
        }

        /// <summary>
        /// Gets all cultures available on the current system.
        /// </summary>
        /// <returns></returns>
        public static List<CultureInfo> GetCultures()
        {
            List<CultureInfo> list = new List<CultureInfo>(10);
            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures))
            {
                try
                {
                    CultureInfo ciTest = new CultureInfo(ci.Name);
                    list.Add(ci);
                }
                catch
                {
                }
            }
            return list;
        }

        /// <summary>
        /// Sets the thread to a particular culture.
        /// </summary>
        /// <param name="cultureCode">Culture code.</param>
        public static bool SetCulture(string cultureCode)
        {
            try
            {
                CultureInfo ci = new CultureInfo(cultureCode);
                System.Threading.Thread.CurrentThread.CurrentCulture = ci;		// set culture for formatting			
                System.Threading.Thread.CurrentThread.CurrentUICulture = ci;	// set culture for resources
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void RenameHashKey(Hashtable ht, string oldName, string newName)
        {
            ht[newName] = ht[oldName];
            ht.Remove(oldName);
        }

        public static Hashtable HashDataRow(DataRow dr)
        {
            Hashtable ht = new Hashtable(dr.Table.Columns.Count);
            foreach (DataColumn dc in dr.Table.Columns)
            {
                ht[dc.ColumnName.ToUpper()] = dr[dc.ColumnName];
            }
            return ht;
        }

        public static bool IsInsertStatus(object col)
        {
            if (col == null || col == DBNull.Value) return false;
            if (col.ToString() == "I") return true;
            return false;
        }

        public static DataColumn DupDataColumn(DataColumn dc)
        {
            DataColumn newDC = new DataColumn();
            newDC.ColumnName = dc.ColumnName;
            newDC.DataType = dc.DataType;
            return newDC;
        }
        public static DataColumn DupDataColumn(DataColumn dc, string newName)
        {
            DataColumn newDC = DupDataColumn(dc);
            newDC.ColumnName = newName;
            return newDC;
        }

        //public static void NotifySplash(string msg, Form parentfrm)
        //{
        //    NotifySplash(msg, parentfrm, -1);
        //}

        //public static void NotifySplash(string msg, Form parentfrm, int timerIntervalMilliSeconds)
        //{
        //    System.Windows.Forms.Label c_msgLabel = new System.Windows.Forms.Label();
        //    c_msgLabel.BackColor = System.Drawing.Color.Transparent;
        //    c_msgLabel.ForeColor = System.Drawing.Color.White;
        //    c_msgLabel.Location = new System.Drawing.Point(10, 10);
        //    c_msgLabel.Name = "c_msgLabel";
        //    c_msgLabel.Size = new System.Drawing.Size(200, 88);
        //    c_msgLabel.TabIndex = 2;
        //    c_msgLabel.Text = msg;
        //    c_msgLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;

        //    System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreCostingShared.Properties.Resources));
        //    System.Windows.Forms.PictureBox c_picture = new System.Windows.Forms.PictureBox();
        //    c_picture.BackColor = System.Drawing.Color.Transparent;
        //    c_picture.Image = ((System.Drawing.Image)(resources.GetObject("Info")));
        //    c_picture.Location = new System.Drawing.Point(3, 50);
        //    c_picture.Margin = new System.Windows.Forms.Padding(0);
        //    c_picture.Name = "c_picture";
        //    c_picture.Padding = new System.Windows.Forms.Padding(1);
        //    c_picture.Size = new System.Drawing.Size(35, 35);
        //    c_picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
        //    c_picture.TabIndex = 4;
        //    c_picture.TabStop = false;

        //    SplashPanel c_splashPanel = new SplashPanel();
        //    c_splashPanel.DiscreetLocation = new System.Drawing.Point(0, 0);
        //    c_splashPanel.Location = new System.Drawing.Point(8, 16);
        //    c_splashPanel.Name = "c_splashPanel";
        //    c_splashPanel.Size = new System.Drawing.Size(220, 90);
        //    c_splashPanel.SlideStyle = Syncfusion.Windows.Forms.Tools.SlideStyle.FadeIn;
        //    c_splashPanel.TabIndex = 27;
        //    c_splashPanel.Visible = false;
        //    c_splashPanel.Controls.Add(c_picture);
        //    c_splashPanel.Controls.Add(c_msgLabel);
        //    parentfrm.Controls.Add(c_splashPanel);

        //    c_splashPanel.DesktopAlignment = SplashAlignment.Center;
        //    if (timerIntervalMilliSeconds < 0) c_splashPanel.TimerInterval = 500;
        //    else c_splashPanel.TimerInterval = timerIntervalMilliSeconds;
        //    c_splashPanel.AnimationSpeed = 20;

        //    c_splashPanel.ShowSplash(Point.Empty, parentfrm, false);
        //}

        public static string NullToEmptyString(object p)
        {
            if (p == null || p == DBNull.Value) return "";
            return p.ToString();
        }
        public static string DBNullToNull(object p)
        {
            if (p == null || p == DBNull.Value) return "null";
            return p.ToString();
        }

        public static object EmptyStringToDBNull(string s)
        {
            if (s != null && s.Trim().Length > 0) return s.Trim();
            return DBNull.Value;
        }

        public static List<string> ColsChanged(DataRow dr, Dictionary<string, int> dicFldDecLenScale)
        {
            List<string> colsChanged = new List<string>(dr.Table.Columns.Count / 3); // assume a third of them will be changed.
            for (int i = 0; i < dr.Table.Columns.Count; i++)
            {
                string fldName = dr.Table.Columns[i].ColumnName.ToUpper();

                object original = (dr.RowState == DataRowState.Added || dr.RowState == DataRowState.Detached) ? DBNull.Value : dr[i, DataRowVersion.Original];
                object current = dr[i, DataRowVersion.Default];
                if (Utility.ColChanged(original, current))
                {
                    if (original.GetType() == typeof(System.Decimal) && original != DBNull.Value && current != DBNull.Value)
                    {
                        int decimals = dicFldDecLenScale[fldName.ToLower()];
                        decimal org = Utility.DecimalParse(original.ToString());
                        decimal cur = Utility.DecimalParse(current.ToString());
                        org = Math.Round(org, decimals, MidpointRounding.AwayFromZero);
                        cur = Math.Round(cur, decimals, MidpointRounding.AwayFromZero);
                        if (org != cur) colsChanged.Add(fldName);
                    }
                    else
                    {
                        colsChanged.Add(fldName);
                    }
                }
            }
            return colsChanged;
        }

        public static CultureInfo AppCultureInfo
        {
            get
            {
                CultureInfo ci = new CultureInfo("en-US");
                ci.DateTimeFormat.ShortDatePattern = "M/d/yyyy";
                ci.DateTimeFormat.ShortTimePattern = "h:mm:ss tt";
                ci.DateTimeFormat.DateSeparator = @"/";
                ci.DateTimeFormat.TimeSeparator = ":";
                return ci;
            }
        }

        public static string SqlString(string sql, object[] parms)
        {
            try
            {
                const string oracleDateTimeFormat = "MM/DD/YYYY HH:MI:SS AM";

                char[] sqlchar = sql.ToCharArray();
                System.Text.StringBuilder sSql = new System.Text.StringBuilder(sql.Length);
                int parmCnt = 0, quoteCnt = 0;

                for (int i = 0; i < sql.Length; i++)
                {
                    if (sqlchar[i] == '\'')
                    {
                        sSql.Append(sqlchar[i]);

                        if (quoteCnt == 0)
                        {
                            quoteCnt++;
                        }
                        else
                        {
                            if (i < sqlchar.Length - 1)
                            {
                                if (sqlchar[i + 1] == '\'')   // double single quote together means a single quote
                                {
                                    sSql.Append(sqlchar[i]);
                                    i++; // skip that
                                }
                                else
                                {
                                    quoteCnt--;
                                }
                            }
                        }
                    }
                    else
                    {
                        if ((sqlchar[i] == ':' || sqlchar[i] == '@') && quoteCnt == 0)
                        {
                            bool oracle = (sqlchar[i] != '@');

                            for (i++; i < sqlchar.Length; i++)
                            {
                                if (sqlchar[i] >= 'a' && sqlchar[i] <= 'z' || sqlchar[i] >= '0' && sqlchar[i] <= '9' || sqlchar[i] >= 'A' && sqlchar[i] <= 'Z' || sqlchar[i] == '_')
                                {// skip name of parm
                                }
                                else break;
                            }
                            object p = parms[parmCnt++];
                            if (p == null || p == DBNull.Value)
                            {
                                sSql.Append("null");
                            }
                            else
                            {
                                Type t = p.GetType();
                                if (t == typeof(string))
                                {
                                    sSql.Append("'" + Utility.AddQuote2(p.ToString()) + "'");
                                }
                                else
                                {
                                    if (t == typeof(decimal))
                                    {
                                        sSql.Append(Utility.GetDecimalValue(p).ToString());
                                    }
                                    else
                                    {
                                        if (t == typeof(DateTime))
                                        {
                                            DateTime dt = (DateTime)p;
                                            if (oracle)
                                            {
                                                sSql.Append("TO_DATE('" + dt.ToString(Utility.AppCultureInfo) + "', '" + oracleDateTimeFormat + "')");
                                            }
                                            else
                                            {
                                                sSql.Append("CONVERT(datetime, " + "'" + dt.ToString(Utility.AppCultureInfo) + "')");
                                            }
                                        }
                                        else sSql.Append("'" + Utility.AddQuote2(p.ToString()) + "'");

                                    }
                                }
                            }
                            i--;
                        }
                        else
                        {
                            sSql.Append(sqlchar[i]);
                        }
                    }
                }
                return sSql.ToString();
            }
            catch (Exception)
            {
                return sql;
            }
        }

        public static string GetStringValue(object v)
        {
            if (IsEmpty(v) || IsEmpty2(v))
            {
                return string.Empty;
            }
            return v.ToString().Trim();
        }

        public static object EmptyToDBNull(object p)
        {
            if (IsEmpty2(p)) return DBNull.Value;
            return p;
        }

        public static string ReplaceOrAddEntryInDelimitedString(string ctrlString, string entryName, string newVal, string delimiter)
        {
            string newCtrlString = "";

            bool foundEntry = false;

            string[] ctrlParts = ctrlString.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < ctrlParts.Length; i++)
            {
                string onePart = ctrlParts[i];

                string partName = onePart;
                if (partName.Contains("="))
                {
                    string[] partParts = onePart.Split(new char[] { '=' });
                    if (entryName.Equals(partParts[0]))
                    {
                        ctrlParts[i] = entryName + "=" + newVal;
                        foundEntry = true;
                    }
                    //should be inside "=" check
                    //to ignore values without "="
                    newCtrlString += (newCtrlString.Length > 0 ? delimiter : "") + ctrlParts[i];
                }


            }

            if (!foundEntry) newCtrlString += (newCtrlString.Length > 0 ? delimiter : "") + entryName + "=" + newVal;

            return newCtrlString;
        }

        public static DataTable DataRowListToDataTable(List<DataRow> selRows)
        {
            if (selRows.Count == 0) return null;

            DataTable dt = selRows[0].Table.Clone();

            for (int i = 0; i < selRows.Count; i++)
            {
                DataRow drSel = selRows[i];
                DataRow drAdd = dt.NewRow();

                drAdd.ItemArray = drSel.ItemArray;

                dt.Rows.Add(drAdd);
            }

            return dt;
        }

        private static bool ColumnEqual(object A, object B)
        {

            // Compares two values to see if they are equal. Also compares DBNULL.Value.
            // Note: If your DataTable contains object fields, then you must extend this
            // function to handle them in a meaningful way if you intend to group on them.

            if (A == DBNull.Value && B == DBNull.Value) //  both are DBNull.Value
                return true;
            if (A == DBNull.Value || B == DBNull.Value) //  only one is DBNull.Value
                return false;
            return (A.Equals(B));  // value type standard comparison
        }

        public static DataTable SelectDistinct(string TableName, DataTable SourceTable, string FieldName)
        {
            DataSet ds = null;
            DataTable dt = new DataTable(TableName);
            dt.Columns.Add(FieldName, SourceTable.Columns[FieldName].DataType);

            object LastValue = null;
            foreach (DataRow dr in SourceTable.Select("", FieldName))
            {
                if (LastValue == null || !(ColumnEqual(LastValue, dr[FieldName])))
                {
                    LastValue = dr[FieldName];
                    dt.Rows.Add(new object[] { LastValue });
                }
            }
            if (ds != null)
                ds.Tables.Add(dt);
            return dt;
        }
        #endregion

        /// <summary>
        /// Extract a string from an enclosed pair of char. e.g. 'Y' will return Y
        /// If one of the enclosure pair is not found, the original string will be returned.
        /// </summary>
        /// <param name="value">The string value</param>
        /// <param name="leftEnclosure">The left part of enclosure. e.g. "(" </param>
        /// <param name="rightEnclosure">The right part of enclosure. e.g. ")"</param>
        /// <returns>The string inside the enclosure</returns>
        public static string ExtractStringEnclosedBy(string value, string leftEnclosure, string rightEnclosure)
        {
            int startP = value.IndexOf(leftEnclosure);
            int endP = value.LastIndexOf(rightEnclosure);
            if (startP < 0 || endP < 0) return value;
            return value.Substring(startP + 1, endP - startP - 1).Trim();
        }

        /// <summary>
        /// Return a list of string for all values enclosed in the specified enclousers
        /// It is used for column name extraction, thus, it removes datarowstate [colname, DataRowState] will return colname only
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="leftEnclosure"></param>
        /// <param name="rightEnclosure"></param>
        /// <returns></returns>
        public static List<string> ExtractColumnNames(string expression, string leftEnclosure, string rightEnclosure)
        {
            int i = 0;
            string colName = "";
            //string s = parmString + ",";
            //int m = -1; // points to the first char of parm
            //int n = 0; // points to the last char of parm
            //int q = 0; // quote count
            List<string> lstCols = new List<string>(5);

            int startP = expression.IndexOf(leftEnclosure);
            int endP = expression.IndexOf(rightEnclosure);

            while (startP < endP)
            {
                colName = expression.Substring(startP + 1, endP - startP - 1).Trim();
                i = endP + 1;

                if (colName.IndexOf(",") > -1)
                {
                    colName = colName.Substring(0, colName.IndexOf(","));
                }
                if (!lstCols.Contains(colName)) lstCols.Add(colName.Trim());

                startP = expression.IndexOf(leftEnclosure, i);
                endP = expression.IndexOf(rightEnclosure, i);
            }

            return lstCols;
        }

        public static string ExtractStringDelimitedBy(string value, string delimiter, int startPos)
        {
            int delLen = delimiter.Length;
            int startP = value.LastIndexOf(delimiter, startPos);
            int endP = value.IndexOf(delimiter, startPos);
            if (startP < 0) startP = 0; else startP += delLen;
            if (endP < 0) endP = value.Length - 1; else endP -= 1;
            return value.Substring(startP, endP - startP + 1).Trim();
        }
        /// <summary>
        /// Extract a string from an enclosed pair of char. e.g. 'Y' will return Y, or equals(True) will return True
        /// If one of the enclosure pair is not found, the original string will be returned.
        /// </summary>
        /// <param name="value">The string value</param>
        /// <param name="leftEnclosure">The left part of enclosure. e.g. "(" </param>
        /// <param name="rightEnclosure">The right part of enclosure. e.g. ")"</param>
        /// <returns>The string inside the enclosure</returns>
        public static string ExtractParameterValues(string value, string leftEnclosure, string rightEnclosure)
        {
            int startP = value.IndexOf(leftEnclosure);
            startP += leftEnclosure.Length;

            int endP = value.IndexOf(rightEnclosure, startP);
            if (startP < 0 || endP < 0) return value;
            return value.Substring(startP, endP - startP).Trim();
        }

        ///// <summary>
        ///// Give a list of parms separated by a comma, returns the parms with proper type casted in an object array.
        ///// </summary>
        ///// <param name="parmString">e.g. '1234',455,'Levi''s Strauss', 'abc', 123.0</param>
        ///// <param name="parmValues">object[] '1234',455,'Levi's','abc',123.0.  Notice changing from 2 single quotes to 1 quote </param>
        //public static void ExtractLiteralParms(string parmString, ref object[] parmValues)
        //{
        //    int i = 0;
        //    string s = parmString + ",";
        //    int m = -1; // points to the first char of parm
        //    int n = 0; // points to the last char of parm
        //    int q = 0; // quote count

        //    List<object> parms = new List<object>(5);

        //    for (; i < s.Length; i++)
        //    {
        //        if (m == -1 && (s[i] >= 'A' && s[i] <= 'Z' || s[i] >= 'a' && s[i] <= 'z' || s[i] >= '0' && s[i] <= '9' || s[i] == '\'' || s[i] == '+' || s[i] == '-')) // legal first char
        //        {
        //            m = i;
        //            if (s[i] == '\'') q++;  // a starting quote, thus ignore all chars 
        //        }
        //        else
        //        {
        //            if (q == 0 && s[i] == ',') // delimiter
        //            {
        //                parms.Add(ConvertParm(s.Substring(m, i - m)));
        //                m = -1;
        //            }
        //            else
        //            {
        //                if (s[i] == '\'')
        //                {
        //                    if (q > 0) q = 0; else q++;
        //                }
        //            }
        //        }
        //    }
        //    parmValues = parms.ToArray();
        //}

        //public static object ConvertParm(string s)
        //{
        //    if (s == "false")
        //    {
        //        return false;
        //    }

        //    if (s.StartsWith("'"))
        //    {
        //        return s.Substring(1, s.Length - 2).Replace("''", "'");
        //    }
        //    else
        //    {
        //        if (s.IndexOf('.') > 0)
        //        {
        //            return Utility.DecimalParse(s);
        //        }
        //        else
        //        {
        //            if (s == string.Empty)
        //            {
        //                return null;
        //            }
        //            else
        //            {
        //                return Int32.Parse(s);
        //            }
        //        }
        //    }
        //}

        //public static int GetMethodEndPosition(string stringWithMethod, int startingPosition)
        //{
        //    int p = 0; // parenthesis count
        //    int i = startingPosition;
        //    for (; i < stringWithMethod.Length; i++)
        //    {
        //        if (stringWithMethod[i] == ')')
        //        {
        //            if (--p == 0) break;
        //        }
        //        else
        //        {
        //            if (stringWithMethod[i] == '(') p++;
        //        }
        //    }
        //    return i;
        //}
        /// <summary>
        /// Splits parm string like this {PDC}='LSUS'|{BRAND}='LEVIS' into a dictionary of value pairs
        /// </summary>
        /// <param name="dropDownParms">the parm string</param>
        /// <returns>the Dictionary of parm and value pair</returns>
        public static Dictionary<string, string> SplitSqlParms(string dropDownParms)
        {
            string[] ddp = dropDownParms.Split('|');
            Dictionary<string, string> dDropDownParms = new Dictionary<string, string>(ddp.Length);

            foreach (string s in ddp)
            {
                if (s.StartsWith("{"))  // split {BRAND}='LEVIS'.  Use string lookup instead of split to minimize dependency on value contents
                {
                    int j = s.IndexOf("}");
                    string subVar = s.Substring(0, j + 1).Trim();
                    j = s.IndexOf("=", j);
                    string subVal = s.Substring(j + 1).Trim();
                    dDropDownParms.Add(subVar, subVal);
                }
            }
            return dDropDownParms;
        }

        /// <summary>
        /// Splits parm string like this [prod_name:ABC,EFG]|[spec_code:MINE,Yours], etc
        /// </summary>
        /// <param name="dropDownParms">the parm string</param>
        /// <returns>the Dictionary of parm and value pair</returns>
        public static Dictionary<string, string> SplitRequiredFieldParms(string fieldVals, string leftEnclosure, string rightEnclosure, string assignmentChar)
        {
            string[] flds = fieldVals.Split('|');
            Dictionary<string, string> dFields = new Dictionary<string, string>(flds.Length);

            foreach (string s in flds)
            {
                if (s.Trim().StartsWith(leftEnclosure))  // split {BRAND}='LEVIS'.  Use string lookup instead of split to minimize dependency on value contents
                {
                    int j = s.Trim().IndexOf(rightEnclosure);
                    int v = s.Trim().IndexOf(":");
                    string subVar = s.Trim().Substring(1, v - 1).Trim();//Get Field name without the square brackets []
                    string subVal = s.Trim().Substring(v + 1, j - (v + 1)).Trim();
                    dFields.Add(subVar, subVal);
                }
            }
            return dFields;
        }

        /// <summary>
        /// Splits parm string like this [prod_name:ABC,EFG]|[spec_code:MINE,Yours], etc
        /// </summary>
        /// <param name="dropDownParms">the parm string</param>
        /// <returns>the Dictionary of parm and value pair</returns>
        public static Dictionary<string, List<string>> GetRequiredFieldParms(string fieldVals, string leftEnclosure, string rightEnclosure, string assignmentChar)
        {
            string[] flds = fieldVals.Split('|');
            Dictionary<string, List<string>> dFields = new Dictionary<string, List<string>>(flds.Length);

            foreach (string s in flds)
            {
                if (s.Trim().StartsWith(leftEnclosure))  // split {BRAND}='LEVIS'.  Use string lookup instead of split to minimize dependency on value contents
                {
                    List<string> argVals = new List<string>(1);
                    int j = s.Trim().IndexOf(rightEnclosure);
                    int v = s.Trim().IndexOf(":");
                    string subVar = s.Trim().Substring(1, v - 1).Trim();//Get Field name without the square brackets []
                    string subVal = s.Trim().Substring(v + 1, j - (v + 1)).Trim();
                    string[] subVals = subVal.Split(',');
                    //get a list of the argument values for this particular argument
                    foreach (string argValue in subVals)
                    {
                        if (!argVals.Contains(argValue))
                        {
                            argVals.Add(argValue);
                        }
                        else
                        {
                            string error = string.Format("Argument value {0} is redundant for the following metaword condition: {1}", argValue, fieldVals);
                            throw new Exception(error);
                        }
                    }//end foreach
                    dFields.Add(subVar, argVals);
                    //free argVals for the next iteration
                    //argVals.Clear();
                }
            }
            return dFields;
        }

        /// <summary>
        ///  Returns a list of function arguments and their values
        /// </summary>
        /// <param name="dropDownParms">the parm string</param>
        /// <returns>the Dictionary of arguments and List of values pair</returns>
        public static Dictionary<string, List<string>> SplitRequiredFieldParmsList(string fieldVals, string leftEnclosure, string rightEnclosure, string assignmentChar)
        {
            string[] flds = fieldVals.Split('|');
            Dictionary<string, List<string>> dFields = new Dictionary<string, List<string>>(flds.Length);

            foreach (string s in flds)
            {
                if (s.Trim().StartsWith(leftEnclosure))  // split {BRAND}='LEVIS'.  Use string lookup instead of split to minimize dependency on value contents
                {
                    int j = s.Trim().IndexOf(rightEnclosure);
                    int v = s.Trim().IndexOf(":");
                    string subVar = s.Trim().Substring(1, v - 1).Trim();//Get Field name without the square brackets []
                    string subVal = s.Trim().Substring(v + 1, j - (v + 1)).Trim();
                    string[] subVals = subVal.Split(',');

                    dFields.Add(subVar, subVals.ToList());
                }
            }
            return dFields;
        }

        /// <summary>
        ///  Returns a Dictionary of function arguments and a List of their values
        /// </summary>
        /// <returns>the Dictionary of arguments and List of values pair</returns>
        public static Dictionary<string, List<string>> SplitRequiredFieldParmsList(string fieldVals, string leftEnclosure, string rightEnclosure, char valSeparationCharacter, char assignmentChar)
        {
            string[] argsColsAndValues = fieldVals.Split(valSeparationCharacter); //i.e. ','
            Dictionary<string, List<string>> dFields = new Dictionary<string, List<string>>(argsColsAndValues.Length);

            int i = argsColsAndValues.Length;
            //The commented out code logic is handled in the invoking class.
            //int remainder = 0;
            //Math.DivRem(i, 2, out remainder);

            //if (remainder > 0)
            //{
            //    string error = string.Format("Readonlyif formula syntax error. Please check arguments and correct the problem");
            //    throw new Exception(error);
            //}

            for (int k = 0; k < i; k++)
            {
                string arg = argsColsAndValues[k];
                arg = arg.Substring(arg.IndexOf(leftEnclosure) + 1, arg.LastIndexOf(rightEnclosure) - (1 + arg.IndexOf(leftEnclosure)));

                string val = argsColsAndValues[k + 1];
                val = val.Substring(val.IndexOf(assignmentChar) + 1, val.LastIndexOf(assignmentChar) - (1 + val.IndexOf(assignmentChar)));
                List<string> valList = new List<string>(1);

                if (dFields.ContainsKey(arg)) //if field contains primary key already, add the value to the list
                {
                    List<string> lst = dFields[arg];
                    if (!lst.Contains(val)) lst.Add(val);
                }
                else
                {
                    valList.Add(val);
                    dFields.Add(arg, valList);
                }
                k++;
            }
            return dFields;
        }//end method        

        /// <summary>
        ///  Returns a list of function arguments columns
        /// </summary>
        /// <returns>the Dictionary of arguments and List of values pair</returns>
        public static List<string> GetArgumentFieldsList(string fieldVals, string leftEnclosure, string rightEnclosure, char valSeparationCharacter, char assignmentChar)
        {
            string[] argsColsAndValues = fieldVals.Split(valSeparationCharacter); //i.e. ','
            List<string> argFields = new List<string>(1);

            int i = argsColsAndValues.Length;

            //
            for (int k = 0; k < i; k++)
            {
                string arg = argsColsAndValues[k];
                arg = arg.Substring(arg.IndexOf(leftEnclosure) + 1, arg.LastIndexOf(rightEnclosure) - (1 + arg.IndexOf(leftEnclosure)));
                argFields.Add(arg);

                //increment so u get an odd value (field name) rather than an even value(field value)
                k++;
            }
            return argFields;
        }

        /// <summary>
        /// Resets a DataTable column error.  i.e. no more error on the column
        /// </summary>
        /// <param name="dr">The Data Row</param>
        /// <param name="colName">The Column name</param>
        public static bool RemoveColumnError(DataRow dr, string columnName, string columnErrorMsg)
        {
            string currentError = dr.GetColumnError(columnName);

            if (currentError.ToLower().Contains(columnErrorMsg.ToLower()))
            {
                // IF only that error on the cell THEN reset the column and row errors
                if (currentError.ToLower() == columnErrorMsg.ToLower())
                {
                    dr.SetColumnError(columnName, null);
                }
                else
                {
                    //IMPORTANT: if more than one error with the same desc on the same col
                    int currentErrorIdx = currentError.IndexOf(columnErrorMsg);
                    int errorDelimIdx;
                    int errorDelimLastIdx;
                    if (currentError.Contains(columnErrorMsg))
                    {
                        errorDelimIdx = currentError.IndexOf(columnErrorMsg);
                        columnErrorMsg = ExtractStringDelimitedBy(currentError, KCS.Common.Shared.Constants.ErrorMessageDelimiter, errorDelimIdx);
                        //check if error Delimiter in front of the error
                        string tempError = currentError.Substring(0, currentErrorIdx);
                        errorDelimIdx = tempError.IndexOf(KCS.Common.Shared.Constants.ErrorMessageDelimiter);
                        errorDelimLastIdx = currentError.LastIndexOf(KCS.Common.Shared.Constants.ErrorMessageDelimiter); //get last delimiter

                        //check if columnErrorMsg is not the first error message in the error string ('Some error | columnErrorMsg ')
                        if (errorDelimIdx > -1 && errorDelimIdx < currentErrorIdx)
                        {
                            string error = currentError.Replace((KCS.Common.Shared.Constants.ErrorMessageDelimiter + columnErrorMsg), String.Empty);
                            dr.SetColumnError(columnName, error);
                        }
                        else //it's the first error in the error string
                        {
                            if (!currentError.Contains(KCS.Common.Shared.Constants.ErrorMessageDelimiter))
                            {
                                string error = currentError.Replace(columnErrorMsg, String.Empty);
                                dr.SetColumnError(columnName, error.Trim());
                            }
                            else
                            {
                                string error = currentError.Replace(columnErrorMsg + KCS.Common.Shared.Constants.ErrorMessageDelimiter, String.Empty);
                                dr.SetColumnError(columnName, error.Trim());
                            }
                        }//end if..else
                    }//end if..else
                }//end if..else
                return true;
            }

            return false;
        }

        public static void ClearColumnErrors(DataRow dr, string columnName)
        {
            dr.SetColumnError(columnName, string.Empty);
        }

        /// <summary>
        /// Appends an error message to a DataRow's Column Error Message (if any).
        /// </summary>
        /// <param name="dr">The Data Row</param>
        /// <param name="columnName">The Column name</param>
        /// <param name="columnErrorMsg">The Error Message</param>
        /// <param name="bizEntityName">Business entity name, for proper formatting.</param>
        public static void SetColumnError(DataRow dr, string columnName, string columnErrorMsg, string bizEntityName)
        {
            string currentError = dr.GetColumnError(columnName);
            string keyToProcess = String.Empty;
            //int currErrorLength = currentError.Length;
            //int begIdx = 0;

            int beginKeyIdx = columnErrorMsg.IndexOf("{");
            int endKeyIdx = columnErrorMsg.IndexOf("}");

            if ((beginKeyIdx > -1 && endKeyIdx > -1) && (beginKeyIdx < endKeyIdx)) //keyed message (from Function or Stored Procedure)
            {
                keyToProcess = columnErrorMsg.Substring(beginKeyIdx, (endKeyIdx - beginKeyIdx) + 1);
                UpdateCurrentErrorMessage(dr, columnName, currentError, columnErrorMsg, keyToProcess);
            }
            else  //non-key message (not from Function or Stored Procedure)
            {
                if (!currentError.ToLower().Contains(columnErrorMsg.ToLower()))
                {
                    if (!string.IsNullOrEmpty(currentError))
                    {
                        columnErrorMsg = string.Format("{0}{1}{2}", currentError, Constants.ErrorMessageDelimiter, columnErrorMsg);
                    }
                    dr.SetColumnError(columnName, columnErrorMsg);
                }
            }//end if

            #region Commented Out
            //if (!string.IsNullOrEmpty(currentError))
            //{
            //    //Get Key
            //    //int newBeginIdx = 0; //the index from which we continue after extracting a key
            //    string colErrorToProcess = String.Empty;
            //    string keyToProcess = String.Empty;

            //    GetNewErrorWithKey(columnErrorMsg, ref colErrorToProcess, ref keyToProcess);

            //    //check if key in currentError and replace that message with the new one 
            //    if (!string.IsNullOrEmpty(keyToProcess))
            //    {
            //        UpdateCurrentErrorMessage(dr, columnName, currentError, colErrorToProcess, keyToProcess);
            //    }
            //}
            //else
            //{ //no key errors
            //else//currenError is empty, overwrite with the error
            //{
            //    dr.SetColumnError(columnName, columnErrorMsg);
            //}
            //}
            ////OLD
            ////remove colErrorToProcess from columnErrorMsg and repeat the process
            //if (!currentError.ToLower().Contains(columnErrorMsg.ToLower()))
            //{
            //    if (!string.IsNullOrEmpty(currentError))
            //    {
            //        columnErrorMsg = string.Format("{0}{1}{2}", currentError, Constants.ErrorMessageDelimiter, columnErrorMsg);
            //    }
            //    dr.SetColumnError(columnName, columnErrorMsg);
            //    //dr.RowError = string.Format(Constants.RowErrorMessage, bizEntityName);
            //}

            ////dr.SetColumnError(columnName, columnErrorMsg);
            ////dr.RowError = string.Format(Constants.RowErrorMessage, bizEntityName); 
            #endregion
        }//end method

        /// <summary>
        /// Replaces specific keyed error message w. a new one
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="columnName"></param>
        /// <param name="currentError">current column error</param>
        /// <param name="colErrorToProcess">the new error message</param>
        /// <param name="key">the new error message unique key</param>
        private static void UpdateCurrentErrorMessage(DataRow dr, string columnName, string currentError, string colErrorToProcess, string key)
        {
            if (!string.IsNullOrEmpty(key) && currentError.Contains(key))
            {
                string errorToReplace = String.Empty;
                int keyIdx = currentError.IndexOf(key);
                int beginKeyIdx = currentError.IndexOf(key);
                int endKeyIdx = beginKeyIdx + key.Length;

                //Get error message w. Key
                int begDelimiterIdx = currentError.Substring(0, endKeyIdx).LastIndexOf(Constants.ErrorMessageDelimiter);//columnErrorMsg.IndexOf(Constants.ErrorMessageDelimiter);
                int endDelimiterIdx = currentError.Substring(endKeyIdx).IndexOf(Constants.ErrorMessageDelimiter);
                if (endDelimiterIdx > -1)
                {
                    endDelimiterIdx = (endKeyIdx + 1) + endDelimiterIdx;
                }

                //it is a "middle" message
                if (begDelimiterIdx > -1 && begDelimiterIdx < beginKeyIdx && endDelimiterIdx > endKeyIdx)
                {
                    errorToReplace = currentError.Substring(begDelimiterIdx, (endDelimiterIdx + Constants.ErrorMessageDelimiter.Length));
                    currentError = currentError.Replace(errorToReplace, colErrorToProcess);
                    dr.SetColumnError(columnName, colErrorToProcess);
                }
                else
                {
                    if (begDelimiterIdx == -1) //that's the only message
                    {
                        dr.SetColumnError(columnName, colErrorToProcess);
                    }
                    else
                    {
                        if (endDelimiterIdx == -1) //it's the last message
                        {
                            errorToReplace = currentError.Substring(begDelimiterIdx);
                            currentError = currentError.Replace(errorToReplace, colErrorToProcess);
                            dr.SetColumnError(columnName, colErrorToProcess);
                        }
                    }//end if
                }//end if              

                //int begDelimiterIdx = currentError.Substring(0, keyIdx).LastIndexOf(Constants.ErrorMessageDelimiter);
                //int endDelmiterIdx = currentError.Substring(keyIdx).IndexOf(Constants.ErrorMessageDelimiter);

                //string errorToReplace = currentError.Substring(begDelimiterIdx, endDelmiterIdx + Constants.ErrorMessageDelimiter.Length);
                //currentError = currentError.Replace(errorToReplace, colErrorToProcess);
                //dr.SetColumnError(columnName, currentError);
            }
            else//append the error
            {
                if (!string.IsNullOrEmpty(currentError))
                {
                    currentError = string.Format("{0}{1}{2}", currentError, Constants.ErrorMessageDelimiter, colErrorToProcess);
                }
                else
                {
                    currentError = colErrorToProcess;
                }
                dr.SetColumnError(columnName, currentError);
            }
        }//end method

        ///// <summary>
        ///// Get an error message with its key
        ///// </summary>
        ///// <param name="columnErrorMsg"></param>
        ///// <param name="colErrorToProcess"></param>
        ///// <param name="keyToProcess"></param>
        //private static void GetNewErrorWithKey(string columnErrorMsg, ref string colErrorToProcess, ref string keyToProcess)
        //{
        //    int beginKeyIdx = columnErrorMsg.IndexOf("{");
        //    int endKeyIdx = columnErrorMsg.IndexOf("}");

        //    //check new error for key and get the key plus the new message
        //    if (beginKeyIdx > -1 && endKeyIdx > -1)
        //    {
        //        keyToProcess = columnErrorMsg.Substring(beginKeyIdx, endKeyIdx + 1);

        //        //Get error message w. Key
        //        int begDelimiterIdx = columnErrorMsg.Substring(0, endKeyIdx + 1).LastIndexOf(Constants.ErrorMessageDelimiter);//columnErrorMsg.IndexOf(Constants.ErrorMessageDelimiter);
        //        int endDelimiterIdx = columnErrorMsg.Substring(endKeyIdx + 1).IndexOf(Constants.ErrorMessageDelimiter);
        //        if (endDelimiterIdx > -1)
        //        {
        //            endDelimiterIdx = (endKeyIdx + 1) + endDelimiterIdx;
        //        }

        //        //is a "middle" message
        //        if (begDelimiterIdx > -1 && begDelimiterIdx < beginKeyIdx && endDelimiterIdx > endKeyIdx)
        //        {
        //            colErrorToProcess = columnErrorMsg.Substring(begDelimiterIdx, (endDelimiterIdx + Constants.ErrorMessageDelimiter.Length));
        //            //newBeginIdx = (endDelimiterIdx + Constants.ErrorMessageDelimiter.Length);
        //        }
        //        else
        //        {
        //            if (begDelimiterIdx == -1) //that's the only message
        //            {
        //                colErrorToProcess = columnErrorMsg;
        //                //newBeginIdx = -1; //nothing left to process
        //                //colErrorToProcess = columnErrorMsg.Substring(begDelimiterIdx, (endDelimiterIdx + Constants.ErrorMessageDelimiter.Length));
        //            }
        //            else
        //            {
        //                if (endDelimiterIdx == -1) //it's the last message
        //                {
        //                    colErrorToProcess = columnErrorMsg.Substring(begDelimiterIdx);
        //                    //newBeginIdx = (endDelimiterIdx + Constants.ErrorMessageDelimiter.Length);
        //                }
        //            }//end if
        //        }//end if                 
        //    }//end if
        //}//end method

        public static void SetDefaultValue(Hashtable hs, string key, object value)
        {
            if (KCS.Common.Shared.Utility.IsEmpty(hs[key])) hs[key] = value;
        }


        /// <summary>
        /// return True if a column in a DataRow is changed (from the original value)
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="colName"></param>
        /// <returns></returns>
        public static bool DataChanged(DataRow dr, string colName)
        {
            object original = (dr.RowState == DataRowState.Added || dr.RowState == DataRowState.Detached) ? DBNull.Value : dr[colName, DataRowVersion.Original];
            object current = dr[colName, DataRowVersion.Default];
            return ColChanged2(original, current);
        }

        /// <summary>
        /// Extra codes from a list
        /// </summary>
        /// <param name="inList">string in format: desc1(code1), desc2(code2)...</param>
        /// <param name="codeList">Returns a list in this format: 'code1','code2'...</param>
        /// <param name="codes">Returns a string array like this: array[0] = 'code1', array[1] = 'code2'...</param>
        public static void ExtractCodesFromList(string inList, out string codeList, out string[] codes)
        {
            codeList = "";
            codes = null;

            inList = inList.Trim();
            if (!inList.EndsWith(")")) return;
            inList += ",";
            int j = 0;
            int i = inList.IndexOf("),");
            while (i >= 0)
            {
                j = inList.LastIndexOf('(', i);
                if (j < 0) throw new ApplicationException("Codes List has missing '(' in list:" + inList);
                codeList += "'" + inList.Substring(j + 1, i - j - 1) + "',";
                i = inList.IndexOf("),", i + 1);
            }
            codeList = codeList.TrimEnd(',');
            codes = codeList.Split(',');
        }

        public static void ExtractCodesFromList(string inList, out string codeList, out string[] codes, bool includeQuote)
        {
            codeList = "";
            codes = null;
            string quoteStr = "";
            if (includeQuote)
            {
                quoteStr = "'";
            }

            inList = inList.Trim();
            if (!inList.EndsWith(")")) return;
            inList += ",";
            int j = 0;
            int i = inList.IndexOf("),");
            while (i >= 0)
            {
                j = inList.LastIndexOf('(', i);
                if (j < 0) throw new ApplicationException("Codes List has missing '(' in list:" + inList);
                codeList += quoteStr + inList.Substring(j + 1, i - j - 1) + quoteStr + ",";
                i = inList.IndexOf("),", i + 1);
            }
            codeList = codeList.TrimEnd(',');
            codes = codeList.Split(',');
        }


        /// <summary>
        /// Extra codes from a list
        /// </summary>
        /// <param name="inList">string in format: description1(code1), description2(code2)...</param>
        /// <param name="descList">Returns a list in this format: array[0] = description1, array[1] = description2...</param>
        /// <param name="valueList">Returns a string array like this: array[0] = code1, array[1] = code2...</param>
        public static void ExtractCodesFromList(string inList, out string[] descList, out string[] valueList)
        {
            descList = null;
            valueList = null;

            inList = inList.Trim();
            if (!inList.EndsWith(")")) return;
            inList += ",";
            int j = 0;
            List<string> lDescList = new List<string>(5);
            List<string> lValueList = new List<string>(5);

            int k = 0;
            int i = inList.IndexOf("),");
            while (i >= 0)
            {
                j = inList.LastIndexOf('(', i);
                if (j < 0) throw new ApplicationException("Codes List has missing '(' in list:" + inList);
                lValueList.Add(inList.Substring(j + 1, i - j - 1));
                lDescList.Add(inList.Substring(k, j - k).Trim());
                k = i + 2;
                i = inList.IndexOf("),", i + 1);
            }
            descList = lDescList.ToArray();
            valueList = lValueList.ToArray();
        }


        /// <summary>
        /// Translate a value to another.  Using same syntax similar the Oracle's Decode function
        /// </summary>
        public static void Translate(Hashtable ht, string keyName, params object[] ifThen)
        {
            if (!ht.ContainsKey(keyName) || ifThen.Length == 0) return;

            object ifValue = null;
            bool gotIfValue = false;

            for (int i = 0; i < ifThen.Length; i++)
            {
                if (i % 2 == 0)
                {
                    ifValue = ifThen[i];
                    gotIfValue = true;
                }
                else
                {
                    if (!Utility.ColChanged(ht[keyName], ifValue)) // found a match
                    {
                        ht[keyName] = ifThen[i];
                        return;
                    }
                    gotIfValue = false;
                }
            }

            if (gotIfValue)  // the default
            {
                ht[keyName] = ifValue;
            }
        }

        public static bool AssignIfChanged(DataRow dr, string colName, object newValue)
        {
            if (ColChanged2(dr[colName], newValue))
            {
                if (dr.Table.Columns[colName].DataType == typeof(Decimal))
                {
                    dr[colName] = GetDecimalValue(newValue);
                }
                else
                {
                    dr[colName] = newValue;
                }
                return true;
            }
            return false;
        }

        public static bool AssignIfChanged(DataRowView drv, string colName, object newValue)
        {
            if (ColChanged2(drv[colName], newValue))
            {
                if (drv.Row.Table.Columns[colName].DataType == typeof(Decimal) && newValue != DBNull.Value)
                {
                    drv[colName] = GetDecimalValue(newValue);
                }
                else
                {
                    drv[colName] = newValue;
                }
                return true;
            }
            return false;
        }

        public static bool AssignIfChanged(DataRow dr, int colIndex, object newValue)
        {
            if (ColChanged2(dr[colIndex], newValue))
            {
                if (dr.Table.Columns[colIndex].DataType == typeof(Decimal))
                {
                    dr[colIndex] = GetDecimalValue(newValue);
                }
                else
                {
                    dr[colIndex] = newValue;
                }
                return true;
            }
            return false;
        }

        public static bool IsRowReallyChanged(DataRow dr)
        {
            if (dr.RowState == DataRowState.Unchanged) return false;
            if (!dr.HasVersion(DataRowVersion.Original)) return true;

            for (int i = 0; i < dr.Table.Columns.Count; i++)
            {
                if (ColChanged2(dr[i, DataRowVersion.Original], dr[i, DataRowVersion.Default])) return true;
            }
            return false;
        }
        public static void SplitDataTableChanges(DataTable dtMain, out DataTable dtCurrent, out DataTable dtOriginal)
        {
            dtCurrent = dtMain.Clone();
            dtOriginal = dtMain.Clone();

            foreach (DataRow dr in dtMain.Rows)
            {
                if (!IsRowReallyChanged(dr)) continue;

                DataRow drCurrent = dtCurrent.NewRow();
                DataRow drOriginal = dtOriginal.NewRow();

                drCurrent.ItemArray = dr.ItemArray;

                if (dr.HasVersion(DataRowVersion.Original))
                {
                    drOriginal.BeginEdit();
                    for (int columnCount = 0; columnCount < dtMain.Columns.Count; columnCount++)
                    {
                        drOriginal[columnCount] = dr[columnCount, DataRowVersion.Original];
                    }
                    drOriginal.EndEdit();
                }
                dtCurrent.Rows.Add(drCurrent);
                dtOriginal.Rows.Add(drOriginal);
            }
        }

        public static void MergeDataTableChanges(DataTable dtMain, DataTable dtResult)
        {
            int i = -1;
            foreach (DataRow dr in dtMain.Rows)
            {
                if (!IsRowReallyChanged(dr))
                {
                    continue;
                }

                i++;
                for (int j = 0; j < dtMain.Columns.Count; j++)
                {
                    AssignIfChanged(dr, j, dtResult.Rows[i][j]);
                }


                if (!string.IsNullOrEmpty(dtResult.Rows[i].RowError))
                {
                    dr.RowError = dtResult.Rows[i].RowError;
                    foreach (DataColumn column in dtResult.Rows[i].GetColumnsInError())
                    {

                        dr.SetColumnError(column.ColumnName, dtResult.Rows[i].GetColumnError(column));

                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(dr.RowError) && string.IsNullOrEmpty(dtResult.Rows[i].RowError))
                    {
                        dr.RowError = null;
                    }
                }
            }
        }

        public static DataTable CombineDataTableChanges(DataTable dtOriginal, DataTable dtCurrent)
        {
            dtOriginal.AcceptChanges();

            for (int i = 0; i < dtOriginal.Rows.Count; i++)
            {
                for (int j = 0; j < dtOriginal.Columns.Count; j++)
                {
                    if (ColChanged2(dtOriginal.Rows[i][j], dtCurrent.Rows[i][j]))
                    {
                        dtOriginal.Rows[i][j] = dtCurrent.Rows[i][j];
                    }
                }
            }
            return dtOriginal;
        }

        /// <summary>
        /// Copy from one hashtable to another the fromKeys fields and also renamed the field names with the corresponding toKeys fields
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="sourceKeys"></param>
        /// <param name="targetKeys"></param>
        public static void CopyHashtableItems(Hashtable source, Hashtable target, string[] sourceKeys, string[] targetKeys)
        {
            for (int i = 0; i < sourceKeys.Length; i++)
            {
                if (source.ContainsKey(sourceKeys[i]))
                {
                    target[targetKeys[i]] = source[sourceKeys[i]];
                }
            }
        }

        public static bool HashtableContainKeys(Hashtable ht, params string[] fromKeys)
        {
            for (int i = 0; i < fromKeys.Length; i++)
            {
                if (ht.ContainsKey(fromKeys[i])) return true;
            }
            return false;
        }


        public static string GetMethodName(string methodString)
        {
            int i = methodString.IndexOf("(");
            if (i > 0)
            {
                int j = methodString.LastIndexOf(" ", i);
                return methodString.Substring(j + 1, i - j - 1);
            }
            return "";
        }

        /// <summary>
        /// Not very efficient... use with care.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="value"></param>
        public static void AddValueToList(IEnumerable<string> list, string value)
        {
            if (value != string.Empty && !list.Contains(value)) list.Concat(new[] {value});
        }

        /// <summary>
        /// Get the Default Views DataRowView for a DataRow
        /// </summary>
        /// <param name="dr">The DataRow</param>
        /// <returns>The DataRowView, else null if doesn't exist</returns>
        public static DataRowView GetDataRowView(DataRow dr)
        {
            if (string.IsNullOrEmpty(dr.Table.DefaultView.RowFilter))
            {
                return dr.Table.DefaultView[dr.Table.Rows.IndexOf(dr)];  // convert to a drv
            }

            foreach (DataRowView drv in dr.Table.DefaultView)
            {
                if (drv.Row == dr) return drv;
            }

            return null;
        }

        /// <summary>
        /// Combine new DataTable to existing Datable both the column and Data.
        /// Note primary Keys of dtNewData will not be merged.
        /// 
        /// </summary>
        /// <param name="DataSourceDataTable"></param>
        /// <param name="dtColData"></param>
        public static void MergeDataTable(DataTable dtTarget, DataTable dtNewData)
        {
            bool colsAdded = false;
            Dictionary<int, object> savedValues = new Dictionary<int, object>(dtTarget.Columns.Count);

            foreach (DataRow dr in dtTarget.Rows)
            {
                if (!(dr.RowState == DataRowState.Modified || dr.RowState == DataRowState.Unchanged)) continue; // can only merge changed or not changed rows

                DataColumn[] priKeys = dtNewData.PrimaryKey;
                object[] priKey = new object[priKeys.Length];
                for (int i = 0; i < priKeys.Length; i++)
                {
                    string keyCol = priKeys[i].ColumnName;
                    priKey[i] = dr[keyCol];
                }
                DataRow dr2 = dtNewData.Rows.Find(priKey);
                if (dr2 != null)
                {
                    if (!colsAdded)
                    {
                        foreach (DataColumn dc in dtNewData.Columns)
                        {
                            if (!priKeys.Contains(dc) && !dtTarget.Columns.Contains(dc.ColumnName))
                            {
                                dtTarget.Columns.Add(dc.ColumnName, dc.DataType);
                            }
                        }
                        colsAdded = true;
                    }

                    bool modified = (dr.RowState == DataRowState.Modified);

                    if (modified) // save the changed columns for later restore after acceptedchanges
                    {
                        savedValues.Clear();
                        for (int i = 0; i < dtTarget.Columns.Count; i++)
                        {
                            if (Utility.ColChanged(dr[i, DataRowVersion.Original], dr[i, DataRowVersion.Current]))
                            {
                                savedValues.Add(i, dr[i, DataRowVersion.Current]);
                                dr[i] = dr[i, DataRowVersion.Original];
                            }
                        }
                    }
                    // put in the new changes
                    foreach (DataColumn dc in dtNewData.Columns)
                    {
                        if (!priKeys.Contains(dc))
                        {
                            dr[dc.ColumnName] = dr2[dc.ColumnName];
                        }
                    }

                    dr.AcceptChanges();

                    if (modified)  // now restore the changed values
                    {
                        foreach (int i in savedValues.Keys)
                        {
                            dr[i] = savedValues[i];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Copies both DataRow and Cell errors from one DataRow to another
        /// </summary>
        /// <param name="originalDataRow"></param>
        /// <param name="dataRowCopy"></param>
        public static void CopyErrors(DataRow originalDataRow, DataRow dataRowCopy)
        {
            if (originalDataRow.HasErrors)
            {
                DataColumn copyColumn;
                string error;

                // second handle column errors from copy to orginal data row
                foreach (DataColumn originalDataColumn in originalDataRow.GetColumnsInError())
                {
                    copyColumn = dataRowCopy.Table.Columns[originalDataColumn.ColumnName];
                    error = originalDataRow.GetColumnError(originalDataColumn);
                    dataRowCopy.SetColumnError(copyColumn, error);
                }

                error = originalDataRow.RowError;
                if ((error != null) && (error != String.Empty))
                {
                    dataRowCopy.RowError = error;
                }
            }
        }

        public static DataTable DupColumn(DataTable dtGridData, string colName)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(DupDataColumn(dtGridData.Columns[colName]));
            foreach (DataRow dr in dtGridData.Rows)
            {
                DataRow dr2 = dt.NewRow();
                dr2[colName] = dr[colName];
                dt.Rows.Add(dr2);
            }
            return dt;
        }

        /// <summary>
        /// Assign a DataRow column value without marking the column as change (Original and Current value will be the same) when a row 
        /// was Modified.  Otherwise a normal assignment will take place.
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="colName"></param>
        /// <param name="value"></param>
        public static void AssignColumnWOChange(DataRow dr, string colName, object value)
        {
            switch (dr.RowState)
            {
                case DataRowState.Unchanged:
                    dr[colName] = value;
                    dr.AcceptChanges();
                    break;
                case DataRowState.Modified:

                    DataRow dr2 = dr.Table.NewRow();

                    dr2.ItemArray = dr.ItemArray;  // dup values
                    CopyErrors(dr, dr2); // dup error msgs
                    dr2[colName] = value; // set the 

                    dr.RejectChanges();  // get rid of modified values

                    dr.BeginEdit();
                    // copy all the image columns (byte[]) back
                    foreach (DataColumn dc in dr.Table.Columns)
                    {
                        if (dr[dc].GetType() == typeof(byte[])) dr[dc] = dr2[dc];
                    }

                    dr[colName] = value; // assign to "original" the new value
                    dr.EndEdit();
                    dr.AcceptChanges(); // now new value in "original"

                    dr.BeginEdit();

                    // put back all changes
                    foreach (DataColumn dc in dr.Table.Columns)
                    {
                        if (ColChanged(dr[dc], dr2[dc]))
                        {
                            dr[dc] = dr2[dc];
                            //Console.WriteLine(dc.ColumnName + ":" + dr[dc.ColumnName, DataRowVersion.Original].ToString() + ":" + dr[dc.ColumnName].ToString());
                        }
                    }

                    CopyErrors(dr2, dr);

                    dr.EndEdit();
                    break;
                default:
                    dr[colName] = value;
                    break;
            }
        }

        // reads a CSV File into a DataTable
        public static DataTable ReadCSV(string fileName)
        {
            System.IO.StreamReader file = new System.IO.StreamReader(fileName);
            string line = "";
            List<string> rec = null;
            DataTable dt = new DataTable("CSVData");
            int i;
            while ((line = file.ReadLine()) != null)
            {
                rec = ExtractCSVLine(line);
                for (i = dt.Columns.Count; i < rec.Count; i++)
                {
                    dt.Columns.Add("COL" + i.ToString());
                }
                DataRow dr = dt.NewRow();
                for (i = 0; i < rec.Count; i++)
                {
                    dr[i] = rec[i];
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        public static DataTable ReadCSV(Stream stream)
        {
            System.IO.StreamReader file = new System.IO.StreamReader(stream);
            string line = "";
            List<string> rec = null;
            DataTable dt = new DataTable("CSVData");
            int i;
            while ((line = file.ReadLine()) != null)
            {
                rec = ExtractCSVLine(line);
                for (i = dt.Columns.Count; i < rec.Count; i++)
                {
                    dt.Columns.Add("COL" + i.ToString());
                }
                DataRow dr = dt.NewRow();
                for (i = 0; i < rec.Count; i++)
                {
                    dr[i] = rec[i];
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        private static List<string> ExtractCSVLine(string line)
        {
            List<string> rec = new List<string>(20);
            char[] chs = line.Trim().ToCharArray();
            StringBuilder data = new StringBuilder(chs.Length);
            int dataIdx = -1;
            char ch;
            bool inQuote = false;

            for (int i = 0; i < chs.Length; i++)
            {
                ch = chs[i];
                switch (ch)
                {
                    case '"':
                        if (inQuote && i < chs.Length - 1)
                        {
                            if (chs[i + 1] == '"')
                            {
                                data.Append(ch);
                                i++;
                            }
                            else
                            {
                                inQuote = false;
                            }
                        }
                        else
                        {
                            if (inQuote)
                            {
                                rec.Add(data.ToString());
                                data.Remove(0, data.Length);
                                inQuote = false;
                            }
                            else
                            {
                                inQuote = true;
                            }
                        }
                        break;
                    case ',':
                        if (inQuote)
                        {
                            data[++dataIdx] = ch;
                        }
                        else
                        {
                            rec.Add(data.ToString());
                            data.Remove(0, data.Length);
                        }
                        break;
                    default:
                        data.Append(ch);
                        break;
                }
            }
            rec.Add(data.ToString());
            return rec;
        }
    }
}
