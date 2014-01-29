using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
	/// <summary>
	/// Project constants.
	/// </summary>
    public class Constants
    {
		/// <summary>
		/// Form-specific property names.
		/// </summary>
        public class FormProperties
        {
			/// <summary>
			/// Top property name.
			/// </summary>
            public const string Top = "Top";

			/// <summary>
			/// Left property name.
			/// </summary>
            public const string Left = "Left";

			/// <summary>
			/// Width property name.
			/// </summary>
            public const string Width = "Width";

			/// <summary>
			/// Height property name.
			/// </summary>
            public const string Height = "Height";            
        }

        //public const string RowErrorMessage = "There are error(s) in this Product. See red-lined cells for details.";		
		public const string RowErrorMessage = "There are error(s) in this {0}. See highlited cells for details.";
		public const string ErrorMessageDelimiter = " | ";

        /// <summary>
        /// To Capture Ctrl F
        /// </summary>
        public const int WM_KEYDOWN = 0x100;
        public const int WM_SYSKEYDOWN = 0x104;
    }
}
