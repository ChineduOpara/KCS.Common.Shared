using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Contains all Enumerations for this project.
    /// </summary>
    public static class Enumerations
    {
        public enum MessageType
        {
            Unknown = 0,
            Information,
            Warning,
            Denied,
            Error,
            Success,
            AuditSuccess,
            AuditFailure,
            Trace
        }

        public enum DatabaseAction
        {
            Nothing,
            [Description("INSERT")]
            Insert,
            [Description("UPDATE")]
            Update,
            [Description("DELETE")]
            Delete,
            [Description("SELECT")]
            Select
        }

        public enum WindowsVersions
        {
            [Description("Unknown")]
            Unknown,
            [Description("Windows 95")]
            Win95,
            [Description("Windows 98")]
            Win98,
            [Description("Windows 98 Second Edition")]
            Win98SE,
            [Description("Windows Me")]
            Me,
            [Description("Windows XP")]
            XP,
            [Description("Windows 2000")]
            Win2000,
            [Description("Windows NT 3.51")]
            NT351,
            [Description("Windows NT 4.0")]
            NT40,
            [Description("Windows 7")]
            Win7
        }

        /// <summary>
		/// Generic sort direction.
		/// </summary>
        public enum SortDirection
        {
            [Description("ASC")]
            Ascending,
            [Description("DESC")]
            Descending
        }

        /// <summary>
        /// HTTP request methods
        /// </summary>
        public enum HttpRequestMethod
        {
            [Description("GET")]
            Get,
            [Description("POST")]
            Post
        }

		/// <summary>
		/// Expansion direction. First used for the CollapsiblePanel control.
		/// </summary>
		public enum ExpandDirection
		{
			/// <summary>
			/// Panel contents expand from left to right, to the right of the expand/collapse button.
			/// </summary>
			Right,

			/// <summary>
			/// Panel contents expand from right to left, to the left of the expand/collapse button.
			/// </summary>
			Left,

			/// <summary>
			/// Panel contents expand from top to bottom, below the expand/collapse button.
			/// </summary>
			Bottom,

			/// <summary>
			/// Panel contents expand from bottom to top, above the expand/collapse button.
			/// </summary>
			Top
		}
    }
}
