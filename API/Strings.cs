using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Linq;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace KCS.Common.Shared
{
	/// <summary>
	/// Utility methods for strings.
	/// </summary>
	public static class Strings
	{
        public static Regex AlphanumericRegex = new Regex("^[a-zA-Z0-9]*$");

        public static string StripInvalidXmlChars(string text)
        {
            var validXmlChars = text.Where(ch => XmlConvert.IsXmlChar(ch)).ToArray();
            text = new string(validXmlChars);
            text = text.Replace("#", string.Empty);
            return text;
        }

        /// <summary>
        /// Attempts to convert any object to a string. The default is am empty string.
        /// </summary>
        /// <param name="obj">Object to convert.</param>
        /// <returns>A string.</returns>
        public static string ConvertToString(object obj)
        {
            try
            {
                if (obj == null) return string.Empty;
                if (Convert.IsDBNull(obj)) return string.Empty;
                return Convert.ToString(obj).Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Builds a list from from a collection of IDs.
        /// </summary>
        /// <param name="idList">Comma-separated list of IDs.</param>
        public static List<long> CsvToListIn64(string idList)
        {
            return CsvToListIn64(idList, ',');
        }

        /// <summary>
        /// Builds a list from from a collection of IDs.
        /// </summary>
        /// <param name="idList">Comma-separated list of IDs.</param>
        /// <param name="separator">Character to use as the separator.</param>
        public static List<long> CsvToListIn64(string idList, char separator)
        {
            List<long> list = new List<long>();
            string[] parts = idList.Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string id in parts)
            {
                list.Add(Convert.ToInt64(id));
            }
            return list;
        }

        /// <summary>
        /// Performs a case-insensitive string replacement.
        /// </summary>
        /// <param name="source">Input string.</param>
        /// <param name="oldValue">String to look for. Supports regular expressions.</param>
        /// <param name="newValue">New string.</param>
        /// <returns>Modified string.</returns>
        public static string Replace(string source, string oldValue, string newValue)
        {
            Regex re = new Regex(oldValue, RegexOptions.IgnoreCase);
            return re.Replace(source, newValue);
        }

        /// <summary>
        /// Truncates a string to a certain length.
        /// </summary>
        /// <param name="original">Original string.</param>
        /// <param name="maxLength">Maximum length of string.</param>
        /// <param name="trailer">String to add to the end of truncated original.</param>
        /// <returns>Truncated string, with optional trailer.</returns>
        public static string Truncate(string original, int maxLength, string trailer)
        {
            string truncated = string.Empty;

            if (string.IsNullOrEmpty(original))
            {
                return original;
            }

            // Default the trailer
            if (string.IsNullOrEmpty(trailer))
            {
                trailer = "...";
            }

            // Original string might not need truncation
            if (original.Length <= maxLength)
            {
                return original;
            }

            original = original.Substring(0, maxLength - trailer.Length);
            return string.Format("{0}{1}", original, trailer);
        }

        /// <summary>
        /// Verifies that a password is strong.
        /// </summary>
        /// <param name="password">Password string.</param>
        /// <returns>True if the password is considered strong.</returns>
        static public bool IsPasswordStrong(string password)
        {
            bool containsDigit = false;
            bool containsLetter = false;

            // Verify that password contains at least one letter and at least one number.
            foreach(char c in password.ToCharArray())
            {
                if (Char.IsDigit(c)) containsDigit = true;
                if (Char.IsLetter(c)) containsLetter = true;
                if (containsDigit && containsLetter)
                    return true;
            }

            return false;
        }

		///// <summary>
		///// Hashes a string and returns the hash.
		///// </summary>
		///// <param name="password">Password to hash.</param>
		///// <returns>Hashed password.</returns>
		//static public string HashPassword(string password) 
		//{ 
		//    return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(password, "md5");
		//} 

        /// <summary>
        /// Creates a string hash of a stream.
        /// </summary>
        /// <param name="stream">Stream containing source of hash.</param>
        /// <returns>String.</returns>
        public static String CreateHash(System.IO.Stream stream)
        {
            byte[] hash = MD5.Create().ComputeHash(stream);
            return Convert.ToBase64String(hash); 
        }

		/// <summary>
		/// Returns TRUE if the given string is a valid email address.
		/// </summary>
		/// <param name="str">String to check.</param>
		/// <returns>True or False.</returns>
		public static Boolean IsValidEmail(String str)
		{
            str = str.Trim();
			return Regex.IsMatch(str, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
		}		

		/// <summary>
		/// Returns TRUE if the given string is a valid URL.
		/// </summary>
		/// <param name="str">String to check.</param>
		/// <returns>True or False.</returns>
		public static bool IsValidUrl(String str)
		{
			// For some reason, the Regular Expressions below return false for
			// URLs like "http://p00-svr-web/OPCAgent". Since I'm not that good with RegEx,
			// I took the simpler route.
			str = str.Trim();
			return (str.StartsWith("http://") || str.StartsWith("https://") ||
				str.StartsWith("ftp://"));
			//return Regex.IsMatch(str, @"^((ht|f)tp(s?))\://([0-9a-zA-Z\-]+\.)+[a-zA-Z]{2,6}(\:[0-9]+)?(/\S*)?$");
            //return Regex.IsMatch(str, @"http://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");
		}

		public static bool IsValidHexNumber(string str)
		{
			return System.Text.RegularExpressions.Regex.IsMatch(str, @"\A\b[0-9a-fA-F]+\b\Z");
		}

        /// <summary>
        /// Checks a filename to see if it has the extension of a common-web file.
        /// </summary>
        /// <param name="filename">Filename to check.</param>
        /// <returns>TRUE if extension matches a common web image.</returns>
        public static bool IsWebImageFile(string filename)
        {
            string ext = System.IO.Path.GetExtension(filename);
            if (ext == null || ext.Length == 0) return false;
            ext = ext.ToLower();
            return (ext == ".jpg" || ext == ".jpeg" || ext == ".gif" || ext == ".png");
        }

        /// <summary>
        /// Checks to see if an object is a number of any kind.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsNumeric(object data)
        {
            if (data == null)
            {
                return false;
            }
            else
            {
                double OutValue;
                return double.TryParse(data.ToString().Trim(),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.CurrentCulture,
                    out OutValue);
            }
        }

        public static bool IsAlphaNumericOnly(string @string)
        {
            return AlphanumericRegex.IsMatch(@string);
        }

        /// <summary>
        /// Checks to see if an object is a boolean.
        /// </summary>
        /// <param name="ObjectToTest"></param>
        /// <returns></returns>
        public static bool IsBoolean(object ObjectToTest)
        {
            if (ObjectToTest == null)
            {
                return false;
            }
            else
            {
                bool OutValue;
                return bool.TryParse(ObjectToTest.ToString().Trim(), out OutValue);
            }
        }

		/// <summary>
		/// Returns TRUE if the given string is a valid Int. If not, returns 0.
		/// </summary>
		/// <param name="string">String to check.</param>
		/// <returns>Value of string as an Int.</returns>
		public static int ConvertToInt(String @string)
		{
            try
            {
                return Int32.Parse(@string, System.Globalization.NumberStyles.Any);
            }
            catch
            {
                return 0;
            }
		}

        /// <summary>
        /// Returns TRUE if the given string is a valid Byte. If not, returns the default.
        /// </summary>
        /// <param name="string">String to check.</param>
        /// <param name="default">Default value to return.</param>
        /// <returns>Value of string as an Byte.</returns>
        public static byte ConvertToByte(String @string, byte @default)
        {
            try
            {
                return byte.Parse(@string, System.Globalization.NumberStyles.Any);
            }
            catch
            {
                return @default;
            }
        }

        /// <summary>
        /// Returns TRUE if the given object is a valid bool. If not, returns false.
        /// </summary>
        /// <param name="value">Object to check.</param>
        /// <returns>Value of string as a bool.</returns>
        public static bool? ConvertToBoolean(object value)
        {
            bool result = false;
            long numericValue;

            if (value == null) return null;
            if (Convert.IsDBNull(value)) return null;

            if (IsBoolean(value))
            {
                if (bool.TryParse(value.ToString(), out result))
                    return result;
                else
                    return false;
            }

            if (IsNumeric(value))
            {
                numericValue = Convert.ToInt64(value);
                return numericValue > 0;
            }

            // Last resort
            if (bool.TryParse(value.ToString(), out result))
                return result;
            else
                return false;    
        }

		/// <summary>
		/// Returns a value converted to Long. If not, returns 0.
		/// </summary>
		/// <param name="string">String to check.</param>
		/// <returns>Value of string as a Long.</returns>
		public static long ConvertToLong(String @string)
		{
            try
            {
                return Int64.Parse(@string, System.Globalization.NumberStyles.Any);
            }
            catch
            {
                return 0;
            }
		}

        /// <summary>
        /// Returns a value converted to Decimal. If not, returns  the second parameter.
        /// </summary>
        /// <param name="string">String to check.</param>
        /// <param name="default">Default value.</param>
        /// <returns>Value of string as a Long.</returns>
        public static decimal ConvertToDecimal(string @string, decimal @default)
        {
            try
            {
                return decimal.Parse(@string, System.Globalization.NumberStyles.Any);
            }
            catch
            {
                return @default;
            }
        }

        /// <summary>
        /// Returns a Long if the given string is a valid number. If not, returns a default number.
        /// </summary>
        /// <param name="string">String to check.</param>
        /// <param name="default">Default number.</param>
        /// <returns>True or False.</returns>
        public static int ConvertToInt(String @string, int @default)
        {
			try
			{
				return Int32.Parse(@string, System.Globalization.NumberStyles.Any);
			}
			catch
			{
				return @default;
			}
        }

        /// <summary>
        /// Returns TRUE if the given string is a valid Short. If not, returns 0.
        /// </summary>
        /// <param name="string">String to check.</param>
        /// <returns>Value of string as a short.</returns>
        public static Int16 ConvertToShort(String @string)
        {
            try
            {
                return Int16.Parse(@string, System.Globalization.NumberStyles.Any);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns a Date if the given string is a valid Long. If not, returns DateTime.MinValue.
        /// </summary>
        /// <param name="string">String to check.</param>
        /// <returns>Value of string as a Long.</returns>
        public static DateTime ConvertToDateTime(String @string)
        {
            try
            {
                return DateTime.Parse(@string);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

		/// <summary>
		/// Attempts to strip a string of all HTML tags.
		/// </summary>
		/// <param name="string">String to process.</param>
		/// <returns>A string that is stripped of all HTML tags.</returns>
		public static String StripHtml(String @string)
		{
			Regex re = new Regex(@"<[\w/]+[^<>]*>", RegexOptions.IgnoreCase);
			return re.Replace(@string, "");
		}

        /// <summary>
        /// Removes all non-digit and non-character characters from a string.
        /// </summary>
        /// <param name="string">String to strip.</param>
        /// <returns>Stripped and fixed string.</returns>
        static public String StripNonLettersOrDigits(String @string)
        {
            return StripNonLettersOrDigits(@string, true, false);
        }

        /// <summary>
        /// Removes all characters in a string that might be a delimiter.
        /// </summary>
        /// <param name="string">String to strip.</param>
        /// <param name="allowWhiteSpacesAndPunctuation">If true, leaves whitespaces alone..</param>
        /// <returns>Stripped and fixed string.</returns>
        static public String StripNonLettersOrDigits(String @string, bool allowWhiteSpaces, bool allowPunctuation)
        {
            StringBuilder sb = new StringBuilder();
            char[] chars = @string.ToCharArray();
            foreach (char c in chars)
            {
                if (allowWhiteSpaces || allowPunctuation)
                {
					if (allowWhiteSpaces)
					{
						if (Char.IsLetterOrDigit(c) || Char.IsWhiteSpace(c))
							sb.Append(c);
						continue;
					}
					if (allowPunctuation)
					{
						if (Char.IsLetterOrDigit(c) || Char.IsWhiteSpace(c) || Char.IsPunctuation(c))
							sb.Append(c);
					}
                }
                else
                {
                    if (Char.IsLetterOrDigit(c))
                        sb.Append(c);
                }

            }
            return sb.ToString();
        }

        /// <summary>
        /// Removes all non-digit characters from a string.
        /// </summary>
        /// <param name="string">String to strip.</param>
        /// <returns>Stripped and fixed string.</returns>
        static public String StripNonDigits(String @string)
        {
            StringBuilder sb = new StringBuilder();
            char[] chars = @string.ToCharArray();
            foreach(char c in chars)
            {
                if (Char.IsDigit(c)) sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Removes all invalid Path characters from a filename.
        /// </summary>
        /// <param name="filename">Filename to strip.</param>
        /// <returns>Stripped and fixed filename.</returns>
        static public String StripInvalidPathChars(String filename)
        {
            string name = filename.Trim();
            //char[] chars = ShellIO.INVALIDCHARSPATH.ToCharArray();
            char[] chars = System.IO.Path.GetInvalidFileNameChars();
            foreach(char c in chars)
            {
                name = name.Replace(c.ToString(), string.Empty);
            }
            return name;
        }

        /// <summary>
        /// Removes all invalid Path characters from a filename.
        /// </summary>
        /// <param name="filename">Filename to strip.</param>
        /// <returns>Stripped and fixed filename.</returns>
        static public String StripInvalidDirectoryChars(string directory)
        {
            string name = directory.Trim();
            char[] chars = System.IO.Path.GetInvalidPathChars();
            foreach (char c in chars)
            {
                name = name.Replace(c.ToString(), string.Empty);
            }
            return name;
        }

        static public string StripTrailingString(string @string, string trailer)
        {
            if (@string.EndsWith(trailer, true, null))
            {
                int lastIndex = @string.LastIndexOf(trailer);
                @string = @string.Substring(0, lastIndex);
            }

            return @string;
        }

        /// <summary>
        /// Replace those characters disallowed in XML documents
        /// </summary>
        /// <param name="input">Data to be formatted.</param>
        /// <returns>String without any characters illegal in XML.</returns>
        public static string FormatForXML(object input)
        {
            string data = input.ToString();      // cast the input to a string

            data = data.Replace("&", "&amp;");
            data = data.Replace("\"", "&quot;");
            data = data.Replace("'", "&apos;");
            data = data.Replace("<", "&lt;");
            data = data.Replace(">", "&gt;");

            return data;
        }

        /// <summary>
        /// Attempts to formats a number as a phone number.
        /// </summary>
        /// <param name="string">String to format.</param>
        /// <returns>Stripped and fixed string.</returns>
        static public String FormatPhoneNumber(string @string)
        {
            if (string.IsNullOrEmpty(@string))
                return string.Empty;

            StringBuilder sb = new StringBuilder(StripNonLettersOrDigits(@string));
            sb = sb.Replace(" ", "");
            if (sb.Length > 3)
            {
                sb = sb.Insert(0, "(");
                sb = sb.Insert(4, ")");
            }

            if (sb.Length > 5)
            {
                sb = sb.Insert(5, " ");
            }

            if (sb.Length > 9)
            {
                sb = sb.Insert(9, "-");
            }

            if (sb.Length > 14)
            {
                sb = sb.Insert(14, " x");
            }

            return sb.ToString();
        }

//		/// <summary>
//		/// Confirms that a string is XHTML-compliant.
//		/// </summary>
//		/// <param name="string">String to process.</param>
//		/// <returns>True if the string is XHTML-compliant.</returns>
//		public static Boolean IsXHMTL(String @string)
//		{
//			try
//			{
//				XmlDocument doc = new XmlDocument();
//				XmlDocumentFragment frag = doc.CreateDocumentFragment();
//				frag.AppendChild(@string);
//				return true;
//			}
//			catch(Exception ex)
//			{
//				string s = ex.Message;
//				return false;
//			}
//		}

//		/// <summary>
//		/// Used to encode an object for output to an HTML page. It encodes any HTML special characters as
//		/// literals instead of letting the browser interpret them. In addition, it replaces multiple spaces,
//		/// tabs, and line breaks with their HTML equivalents thus preserving the layout of the specified text.
//		/// The size of expanded tab characters can be altered using the TabSize property. Set it to the
//		/// number of non-breaking spaces that should replace the tab character. The default is four.
//		/// </summary>
//		/// <param name="objText"></param>
//		/// <param name="bEncodeLinks"></param>
//		/// <returns></returns>
//		public static string HTMLEncode(Object objText, Boolean bEncodeLinks)
//		{
//			StringBuilder strTemp = new StringBuilder(256);
//			string strExpTab = null;		// This should be a property.
//			int tabSize = 4;				// Should be a constant
//
//			if(objText is System.DBNull)
//				strTemp.Append("&nbsp;");
//			else
//			{
//				// Create tab expansion string if not done already
//				if(strExpTab == null)
//					strExpTab = new String(' ', tabSize).Replace(" ", "&nbsp;");
//
//				strTemp.Append(HttpUtility.HtmlEncode( objText.ToString()));
//				strTemp.Replace("  ", "&nbsp;&nbsp;");  // Two spaces
//				strTemp.Replace("\t", strExpTab);
//				strTemp.Replace("\r", "");
//				strTemp.Replace("\n", "<br>");
//
//				if(strTemp.Length == 0 || (strTemp.Length == 1 &&
//					strTemp[0] == ' '))
//					strTemp.Remove(0, strTemp.Length).Append("&nbsp;");
//			}
//
//			if(!bEncodeLinks)
//				return strTemp.ToString();
//
//			// Try to convert URLs, UNCs, and e-mail addresses to links
//			return EncodeLinks(strTemp.ToString());
//		}
//
//		/// <summary>
//		/// Takes the passed string and finds all URLs, UNCs, and e-mail addresses and converts them to
//		/// clickable hyperlinks suitable for rendering in an HTML page. For UNC paths, it will include any
//		/// text up to the first whitespace character. If the path contains spaces, you can enclose the
//		/// entire path in angle brackets (i.e., <\\Server\Folder\Name With Spaces>) and the encoder will
//		/// include all text between the angle brackets in the hyperlink. The angle brackets will not
//		/// appear in the encoded hyperlink.
//		/// </summary>
//		/// <param name="strText"></param>
//		/// <returns></returns>
//		public static string EncodeLinks(string strText)
//		{
//			// We'll create these on first use and keep them around
//			// for subsequent calls to save resources.
//			if(reURL == null)
//			{
//				reURL = new Regex(@"(((http|ftp|https)://)|(www\.))+" +
//					@"[\w]+(.[\w]+)([\w\-\.@?^=%&:/~\+#]*[\w\-" +
//					@"\@?^=%&/~\+#])?", RegexOptions.IgnoreCase);
//				reUNC = new Regex(@"(\\{2}\w+(\\((&.{2,8};|" +
//					@"[\w\-\.,@?^=%&:/~\+#\$])*[\w\-\@?^=%&" +
//					@"/~\+#\$])?)*)|((\<|\<)\\{2}\w+(\\((" +
//					@"&.{2,8};|[\w\-\.,@?^=%&:/~\+#\$ ])*)?)*" +
//					@"(\>|\>))", RegexOptions.IgnoreCase);
//				reEMail = new Regex(@"([a-zA-Z0-9_\-])([a-zA-Z0-9_" +
//					@"\-\.]*)@(\[((25[0-5]|2[0-4][0-9]|1[0-9][0-9]|" +
//					@"[1-9][0-9]|[0-9])\.){3}|((([a-zA-Z0-9\-]+)\.)" +
//					@"+))([a-zA-Z]{2,}|(25[0-5]|2[0-4][0-9]|1[0-9]" +
//					@"[0-9]|[1-9][0-9]|[0-9])\])",
//					RegexOptions.IgnoreCase);
//				reTSURL = new Regex(@"((&\#\d{1,3}|&\w{2,8}))+$");
//				reTSUNC = new Regex(@"\.?((&\#\d{1,3}|&\w{2,8})" +
//					@";((&\#\d{1,3}|&\w{2,8}))?)+\w*$");
//
//				URLMatchEvaluator = new MatchEvaluator(OnURLMatch);
//				UNCMatchEvaluator = new MatchEvaluator(OnUNCMatch);
//			}
//
//			// Do the replacements
//			strText = reURL.Replace(strText, URLMatchEvaluator);
//			strText = reUNC.Replace(strText, UNCMatchEvaluator);
//			strText = reEMail.Replace(strText,
//				@"<a href='mailto:$&'>$&</a>");
//
//			return strText;
//		}
//
//		/// <summary>
//		/// Replace a URL with a link to the URL.  This checks for a missing protocol and adds it if
//		/// necessary.
//		/// </summary>
//		/// <param name="match"></param>
//		/// <returns></returns>
//		private static string OnURLMatch(Match match)
//		{
//			StringBuilder strLink = new StringBuilder("<a href='", 256);
//			string strURL = match.Value;
//
//			// Use default HTTP protocol if one wasn't specified
//			if(strURL.IndexOf("://") == -1)
//				strLink.Append("http://");
//
//			// Move trailing special characters outside the link
//			Match m = reTSURL.Match(strURL);
//			if(m.Success == true)
//				strURL = reTSURL.Replace(strURL, "");
//
//			strLink.Append(strURL);
//			strLink.Append("' target='_BLANK'>");
//			strLink.Append(strURL);
//			strLink.Append("</a>");
//
//			if(m.Success == true)
//				strLink.Append(m.Value);
//
//			return strLink.ToString();
//		}
//
//		/// <summary>
//		/// Replace a UNC with a link to the UNC.  This strips off any containing brackets (plain or
//		/// encoded) and flips the slashes.
//		/// </summary>
//		/// <param name="match"></param>
//		/// <returns></returns>
//		private static string OnUNCMatch(Match match)
//		{
//			StringBuilder strLink = new StringBuilder("<a href='file:", 256);
//			string strUNC = match.Value;
//
//			// Strip brackets if found.  If it has encoded brackets,
//			// strip them too.
//			if(strUNC[0] == '<')
//				strUNC = strUNC.Substring(1, strUNC.Length - 2);
//			else
//				if(strUNC.StartsWith("<"))
//				strUNC = strUNC.Substring(4, strUNC.Length - 8);
//
//			// Move trailing special characters outside the link
//			Match m = reTSUNC.Match(strUNC);
//			if(m.Success == true)
//				strUNC = reTSUNC.Replace(strUNC, "");
//
//			strLink.Append(strUNC);
//			strLink.Append("' target='_BLANK'>");
//
//			// Replace backslashes with forward slashes
//			strLink.Replace('\\', '/');
//
//			strLink.Append(strUNC);
//			strLink.Append("</a>");
//
//			if(m.Success == true)
//				strLink.Append(m.Value);
//
//			return strLink.ToString();
//		}
	}
}
