using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Specialized;

namespace KCS.Common.Shared
{
    public static class ReflectionExtensions
    {
        public static T GetAssemblyAttribute<T>(this Assembly ass) where T : Attribute
        {

            if (ass == null) return null;

            object[] attributes = ass.GetCustomAttributes(typeof(T), true);

            if (attributes == null) return null;

            if (attributes.Length == 0) return null;

            return (T)attributes[0];
        }

        public static Guid GetAssemblyGuid(this Assembly ass)
        {
            GuidAttribute ga = ass.GetAssemblyAttribute<GuidAttribute>();

            return ga == null ? new Guid() : new Guid(ga.Value);
        }

        /// <summary>
        /// Gets an enumeration based on the matching Description attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumType"></param>
        /// <param name="enumDesc"></param>
        /// <returns></returns>
        public static T FromDescription<T>(this Type enumType, string enumDesc)
        {
            if (!(typeof(T).IsEnum && enumType.IsEnum))
            {
                throw new NotSupportedException("This method can only be called on Enumeration types.");
            }

            T ret = default(T);
            foreach (T item in Enum.GetValues(typeof(T)))
            {
                FieldInfo fi = typeof(T).GetField(item.ToString());
                DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
                string compareDesc = attributes.Length == 0 ? item.ToString() : attributes[0].Description;
                if (string.Compare(enumDesc, compareDesc, true) == 0)
                {
                    return item;
                }
            }

            return ret;
        }


        /// <summary>
        /// Gets the Description attribute of any enumeration.
        /// </summary>
        /// <param name="en">Enumeration item.</param>
        /// <returns>Human-readable description.</returns>
        public static string GetDescription(this Enum en)
        {
            FieldInfo fi = en.GetType().GetField(en.ToString());
            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
            return attributes.Length == 0 ? en.ToString() : attributes[0].Description;
        }

        /// <summary>
        /// Gets a custom attribute of any enumeration.
        /// </summary>
        /// <param name="en">Enumeration item.</param>
        /// <returns>Human-readable description.</returns>
        public static T GetAttribute<T>(this Enum en)
        {
            FieldInfo fi = en.GetType().GetField(en.ToString());
            object[] attributes = fi.GetCustomAttributes(typeof(T), false);
            if (attributes.Length == 0)
            {
                return default(T);
            }
            else
            {
                return (T)Convert.ChangeType(attributes[0], typeof(T));
            }
        }

        /// <summary>
        /// Gets a custom attribute.
        /// </summary>
        /// <param name="en">Enumeration item.</param>
        /// <returns>Human-readable description.</returns>
        public static T GetAttribute<T>(this FieldInfo fi)
        {
            object[] attributes = fi.GetCustomAttributes(typeof(T), false);
            if (attributes.Length == 0)
            {
                return default(T);
            }
            else
            {
                return (T)Convert.ChangeType(attributes[0], typeof(T));
            }
        }

        /// <summary>
        /// Gets a custom attribute.
        /// </summary>
        /// <param name="en">Enumeration item.</param>
        /// <returns>Human-readable description.</returns>
        public static T GetAttribute<T>(this MemberInfo mi)
        {
            object[] attributes = mi.GetCustomAttributes(typeof(T), false);
            if (attributes.Length == 0)
            {
                return default(T);
            }
            else
            {
                return (T)Convert.ChangeType(attributes[0], typeof(T));
            }
        }

        /// <summary>
        /// Gets a custom attribute of a Method.
        /// </summary>
        /// <returns>Human-readable description.</returns>
        public static T GetAttribute<T>(this MethodInfo mi)
        {
            object[] attributes = mi.GetCustomAttributes(typeof(T), true);
            if (attributes.Length == 0)
            {
                return default(T);
            }
            else
            {
                return (T)Convert.ChangeType(attributes[0], typeof(T));
            }
        }

        /// <summary>
        /// Gets a custom attribute of a Property.
        /// </summary>
        /// <returns>Human-readable description.</returns>
        public static T GetAttribute<T>(this PropertyInfo mi)
        {
            object[] attributes = mi.GetCustomAttributes(typeof(T), true);
            if (attributes.Length == 0)
            {
                return default(T);
            }
            else
            {
                return (T)Convert.ChangeType(attributes[0], typeof(T));
            }
        }

        /// <summary>
        /// Gets a custom attribute of any Type.
        /// </summary>
        /// <param name="en">Enumeration item.</param>
        /// <returns>Human-readable description.</returns>
        public static T GetAttribute<T>(this Type type, bool inherit)
        {
            object[] attributes = type.GetCustomAttributes(typeof(T), inherit);
            if (attributes.Length == 0)
            {
                return default(T);
            }
            else
            {
                return (T)Convert.ChangeType(attributes[0], typeof(T));
            }
        }

        public static T GetAttribute<T>(this Type type)
        {
            return type.GetAttribute<T>(true);
        }

        public static T GetMethodAttribute<T>(this Type type, string methodName)
        {
            MethodInfo[] methods = type.GetMethods();
            var method = methods.Where(m => m.Name.CompareTo(methodName) == 0).FirstOrDefault();
            if (method == null)
            {
                return default(T);
            }

            var attribute = method.GetCustomAttributes(typeof(T), true).FirstOrDefault();
            if (attribute == null)
            {
                return default(T);
            }
            else
            {
                return (T)Convert.ChangeType(attribute, typeof(T));
            }
        }

        public static IEnumerable<MethodInfo> GetMethodsByAttribute<T>(this Type type) where T : Attribute
        {
            if (!type.IsClass)
            {
                throw new ArgumentException("Type is not Class.", type.Name);
            }

            List<MethodInfo> list = new List<MethodInfo>();
            foreach (MethodInfo mi in type.GetMethods())
            {
                var match = mi.GetCustomAttributes(true).Where(a => a.GetType().Equals(typeof(T)));
                if (match.Count() > 0)
                {
                    list.Add(mi);
                }
            }

            return list;
        }

        /// <summary>
        /// Gets all properties tagged with SettingPropertyAttribute.
        /// </summary>
        /// <returns>Dictionary of PropertyInfo and SettingPropertyAttribute.</returns>
        public static Dictionary<PropertyInfo, T> GetTaggedProperties<T>(this Type type)
        {
            var dic = new Dictionary<PropertyInfo, T>();

            foreach (PropertyInfo prop in type.GetProperties())
            {
                var attr = prop.GetAttribute<T>();
                if (attr != null)
                {
                    dic.Add(prop, attr);
                }
            }

            return dic;
        }

        public static FieldInfo[] GetConstants(this Type type)
        {
            List<FieldInfo> constants = new List<FieldInfo>();

            FieldInfo[] fieldInfos = type.GetFields();

            // Go through the list and only pick out the constants
            foreach (FieldInfo fi in fieldInfos)
                // IsLiteral determines if its value is written at 
                //   compile time and not changeable
                // IsInitOnly determine if the field can be set 
                //   in the body of the constructor
                // for C# a field which is readonly keyword would have both true 
                //   but a const field would have only IsLiteral equal to true
                if (fi.IsLiteral && !fi.IsInitOnly)
                    constants.Add(fi);

            // Return an array of FieldInfos
            return constants.ToArray();
        }

        /// <summary>
        /// Returns the numeric representation of an Enumeration member.
        /// </summary>
        /// <param name="en">Enumeration item.</param>
        /// <returns>Short representation of the item.</returns>
        public static T ToNumber<T>(this Enum en)
        {
            if (!typeof(T).IsNumeric())
            {
                throw new Exception("The requested type must be Numeric!");
            }
            return (T)Convert.ChangeType(en, typeof(T));
        }

        public static int ToNumber(this Enum en)
        {
            return en.ToNumber<int>();
        }

        /// <summary>
        /// Converts an enumeration to a table.
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns>Table with 2 columns: Key and Value</returns>
        public static DataTable ToDataTable(this Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new InvalidCastException("This method is only for Enum types.");
            }

            DataTable dt = new DataTable(enumType.Name);
            dt.EnsureColumn("Key", typeof(int));
            dt.EnsureColumn("Value");

            foreach (Enum e in Enum.GetValues(enumType))
            {
                dt.Rows.Add(e.ToNumber<int>(), e.GetDescription());
            }

            return dt;
        }

        public static IEnumerable<KeyValuePair<string, string>> ToNameValuePairs(this NameValueCollection collection)
        {
            return collection.Cast<string>().Select(key => new KeyValuePair<string, string>(key, collection[key]));
        }

        /// <summary>
        /// Converts an enumeration to a Dictionary.
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns>Dictionary.</returns>
        public static Dictionary<T, string> ToDictionary<T>(this Type enumType, bool getDescriptions = true)
        {
            if (!typeof(T).IsNumeric())
            {
                throw new Exception("The requested type must be Numeric!");
            }

            if (!enumType.IsEnum)
            {
                throw new InvalidCastException("This method is only for Enum types.");
            }

            Dictionary<T, string> dic = new Dictionary<T, string>();

            foreach (Enum e in Enum.GetValues(enumType))
            {
                dic.Add(e.ToNumber<T>(), getDescriptions ? e.GetDescription() : e.ToString());
            }

            return dic;
        }

        /// <summary>
        /// Constructs and populates the StackFrames class.
        /// </summary>
        /// <param name="ex"></param>
        public static List<StackFrame> GetStackFrames(this Exception ex)
        {
            List<StackFrame> list = new List<StackFrame>();
            StackTrace st = new StackTrace(ex, true);
            list.AddRange(st.GetFrames());

            return list;
        }

        /// <summary>
        /// Constructs and populates the StackFrames class.
        /// </summary>
        /// <param name="ex"></param>
        public static string GetStackFramesString(this Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            List<StackFrame> list = new List<StackFrame>();
            StackTrace st = new StackTrace(ex, true);
            var frames = st.GetFrames();
            if (frames != null)
            {
                list.AddRange(frames);
            }

            foreach (StackFrame sf in list)
            {
                sb.AppendFormat("File: {0}\r\nMethod: {1}\r\nRow {2}, Column {3}", sf.GetFileName(), sf.GetMethod().Name, sf.GetFileLineNumber(), sf.GetFileColumnNumber());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Recursively gets all the nested exceptions.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static List<Exception> GetAllExceptions(this Exception ex)
        {
            List<Exception> list = new List<Exception>(1);
            list.Add(ex);
            if (ex.InnerException != null)
            {
                list.AddRange(ex.InnerException.GetAllExceptions());
            }
            return list;
        }

        public static string GetAllExceptionsString(this Exception ex, string delimiter = "\r\n\r\n")
        {
            var list = ex.GetAllExceptions();
            var messages = list.Select(x => x.Message).ToArray();
            var values = string.Join(delimiter, messages);

            return values;
        }
    }
}
