using System;
using System.Text;
using System.Runtime.InteropServices;

namespace KCS.Common.Shared
{
	/// <summary>
	/// Struct for various Win32 API types and constants.
	/// </summary>
	public struct Win32API
	{
        //[DllImport("winmm.dll")]
        //private static extern long mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);

        //public void PlayFile(string path)
        //{
        //    mciSendString("open \"" + path + "\" type mpegvideo alias MediaFile", null, 0, IntPtr.Zero);
        //    mciSendString("play MediaFile", null, 0, IntPtr.Zero);
        //}

		public enum Messages : int
		{
			/// <summary>
			/// W_PAINT
			/// </summary>
			Paint = 0x000F,
			/// <summary>
			/// WM_CUT
			/// </summary>
			Cut = 0x0301,
			/// <summary>
			/// WM_COPY
			/// </summary>
			Copy = 0x0300,
			/// <summary>
			/// WM_PASTE
			/// </summary>
			Paste = 0x0302
		}
		
		[DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
		public static extern IntPtr CreateRoundRectRgn
		(
			int nLeftRect, // x-coordinate of upper-left corner
			int nTopRect, // y-coordinate of upper-left corner
			int nRightRect, // x-coordinate of lower-right corner
			int nBottomRect, // y-coordinate of lower-right corner
			int nWidthEllipse, // height of ellipse
			int nHeightEllipse // width of ellipse
		);

		/// <summary>
		/// Class for various functions in Mpr.dll
		/// </summary>
		public class Mpr
		{
			[DllImport("mpr.dll")]
			public static extern UInt32 WNetAddConnection2(ref NETRESOURCE lpNetResource,
			string lpPassword, string lpUsername, uint dwFlags);

			[DllImport("mpr.dll")]
			public static extern UInt32 WNetAddConnection3(IntPtr hWndOwner, ref NETRESOURCE
			lpNetResource, string lpPassword, string lpUserName, uint dwFlags);

			[DllImport("mpr.dll")]
			public static extern uint WNetCancelConnection2(string lpName, uint dwFlags, bool bForce);

			[StructLayout(LayoutKind.Sequential)]
			public struct NETRESOURCE
			{
				public uint dwScope;
				public uint dwType;
				public uint dwDisplayType;
				public uint dwUsage;
				public string lpLocalName;
				public string lpRemoteName;
				public string lpComment;
				public string lpProvider;
			}

			public const uint RESOURCETYPE_DISK = 1;

			/// <summary>
			/// If this flag is set, the operating system is instructed to remember the mapping of the drive
			/// letter in the user's profile. This means that if the user logs off, when they log on again
			/// at a later date, an attempt to restore the connection will be made.
			/// </summary>
			internal const uint CONNECT_UPDATE_PROFILE = 0x1;

			/// <summary>
			/// When this flag is set, the operating system is permitted to ask the user for authentication
			/// information before attempting to map the drive letter.
			/// </summary>
			const uint CONNECT_INTERACTIVE = 0x8;

			/// <summary>
			/// When set, this flag indicates that any default user name and password credentials will not
			/// be used without first giving the user the opportunity to override them. This flag is ignored
			/// if CONNECT_INTERACTIVE is not also specified.
			/// </summary>
			const uint CONNECT_PROMPT = 0x10;

			/// <summary>
			/// This flag forces the redirection of a local device when making the connection. For the
			/// functionality described in this article the flag has no effect. It is included here for
			/// completeness.
			/// </summary>
			const uint CONNECT_REDIRECT = 0x80;

			/// <summary>
			/// This flag indicates that if the operating system needs to ask for a user name and password,
			/// it should do so using the command line rather than by using dialog boxes. This flag is
			/// ignored if CONNECT_INTERACTIVE is not also specified. It is not available to Windows 2000 or
			/// earlier versions of the operating system.
			/// </summary>
			const uint CONNECT_COMMANDLINE = 0x800;

			/// <summary>
			/// If set, this flag specifies that any credentials entered by the user will be saved. If it
			/// is not possible to save the credentials or the CONNECT_INTERACTIVE is not also specified then
			/// the flag is ignored.
			/// </summary>
			const uint CONNECT_CMD_SAVECRED = 0x1000;
		}						

		/// <summary>
		/// C# representation of the IMalloc interface.
		/// </summary>
		[InterfaceType ( ComInterfaceType.InterfaceIsIUnknown ), Guid ( "00000002-0000-0000-C000-000000000046" )]
		public interface IMalloc
		{
			/// <summary>
			/// Alloc.
			/// </summary>
			/// <param name="cb"></param>
			/// <returns></returns>
			[PreserveSig] IntPtr Alloc ( [In] int cb );
			/// <summary>
			/// Realloc.
			/// </summary>
			/// <param name="pv"></param>
			/// <param name="cb"></param>
			/// <returns></returns>
			[PreserveSig] IntPtr Realloc ( [In] IntPtr pv, [In] int cb );
			/// <summary>
			/// Free.
			/// </summary>
			/// <param name="pv"></param>
			[PreserveSig] void   Free ( [In] IntPtr pv );
			/// <summary>
			/// GetSize.
			/// </summary>
			/// <param name="pv"></param>
			/// <returns></returns>
			[PreserveSig] int    GetSize ( [In] IntPtr pv );
			/// <summary>
			/// DidAlloc.
			/// </summary>
			/// <param name="pv"></param>
			/// <returns></returns>
			[PreserveSig] int    DidAlloc ( IntPtr pv );
			/// <summary>
			/// HeapMinimize.
			/// </summary>
			[PreserveSig] void   HeapMinimize ( );
		}

		/// <summary>
		/// Class for various functions in User32.dll
		/// </summary>
		public class User32
		{
			[StructLayout(LayoutKind.Sequential)]
			public struct FLASHWINFO
			{
				public UInt32 cbSize;
				public IntPtr hwnd;
				public Int32 dwFlags;
				public UInt32 uCount;
				public Int32 dwTimeout;
			}

			public enum MouseEvents
			{
				LeftDown = 0x02,
				LeftUp = 0x04,
				RightDown = 0x08,
				RightUp = 0x10,
				LeftClick = LeftDown | LeftUp,
				RightClick = RightDown | RightUp
			}

			public enum FlashWindow : uint
			{
				// stop flashing
				Stop = 0,

				// flash the window title 
				Caption = 1,

				// flash the taskbar button
				Taskbar = 2,

				// 1 | 2
				All = 3,

				// flash continuously 
				Continous = 4,

				// flash until the window comes to the foreground 
				UntilForeground = 12
			}

			/// <summary>
			/// Used for GetLastInputInfo.
			/// </summary>
			public struct LASTINPUTINFO
			{
				public uint cbSize;
				public uint dwTime;
			}

			/// <summary>
			/// GetActiveWindow.
			/// </summary>
			/// <returns></returns>
			[DllImport("User32.DLL")]
			public static extern IntPtr GetActiveWindow();

            /// <summary>
            /// FindWindowByCaption
            /// </summary>
            /// <param name="ZeroOnly"></param>
            /// <param name="lpWindowName"></param>
            /// <returns></returns>
            [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
            public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

            [DllImport("user32.dll")]
            public static extern bool SetWindowText(IntPtr hWnd, string text);

			/// <summary>
			/// GetShellWindow,
			/// </summary>
			/// <returns></returns>
			[DllImport("User32.DLL")]
			public extern static IntPtr GetShellWindow();

			[DllImport("user32.dll")]
			public static extern Int32 FlashWindowEx(ref FLASHWINFO pwfi);

			[DllImport("user32.dll", CharSet = CharSet.Auto)]
			public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

			[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
			public static extern void MouseEvent(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);

			[DllImport("user32.dll")]
			public static extern bool LockWindowUpdate(IntPtr hWndLock);

			[DllImport("user32.dll")]
			public static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
		}

		/// <summary>
		/// Class for various functions in Shlwapi.dll
		/// </summary>
		public static class Shlwapi
		{
			[DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
			private static extern long StrFormatByteSize(long fileSize,
			[MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer, int bufferSize);

			public static string FormatByteSize(long fileSize)
			{
				StringBuilder sbBuffer = new StringBuilder(20);
				StrFormatByteSize(fileSize, sbBuffer, 20);
				return sbBuffer.ToString();
			}
		}

		/// <summary>
		/// Class for various functions in Shell32.dll
		/// </summary>
		public static class Shell32
		{
			#region Constants for SHFileOperation
			internal const int FO_DELETE = 3;
			internal const int FO_COPY = 2;
			internal const int FOF_ALLOWUNDO = 0x40;
			internal const int FOF_NOCONFIRMATION = 0x10; //Don't prompt the user.;
			internal const int FOF_SIMPLEPROGRESS = 0x100;
			#endregion

			#region Constants for SHGetFileInfo
			internal const uint SHGFI_ICON = 0x100;                         // get icon
			internal const uint SHGFI_DISPLAYNAME = 0x200;                  // get display name
			internal const uint SHGFI_TYPENAME = 0x000000400;               // get type name
			internal const uint SHGFI_ATTRIBUTES = 0x800;                   // get attributes
			internal const uint SHGFI_ICONLOCATION = 0x1000;                // get icon location
			internal const uint SHGFI_EXETYPE = 0x2000;                     // return exe type
			internal const uint SHGFI_SYSICONINDEX = 0x4000;                // get system icon index
			internal const uint SHGFI_LINKOVERLAY = 0x8000;                 // put a link overlay on icon
			internal const uint SHGFI_SELECTED = 0x10000;                   // show icon in selected state
			internal const uint SHGFI_LARGEICON = 0x0;                      // get large icon
			internal const uint SHGFI_SMALLICON = 0x1;                      // get small icon
			internal const uint SHGFI_OPENICON = 0x2;                       // get open icon
			internal const uint SHGFI_SHELLICONSIZE = 0x4;                  // get shell size icon
			internal const uint SHGFI_PIDL = 0x8;                           // pszPath is a pidl
			internal const uint SHGFI_USEFILEATTRIBUTES = 0x10;             // use passed dwFileAttribute
			internal static uint FILE_ATTRIBUTE_NORMAL = 0x80;
			#endregion

			/// <summary>
			/// SHFILEOPSTRUCT
			/// </summary>
			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
			internal struct SHFILEOPSTRUCT
			{
				/// <summary>
				/// Window handle.
				/// </summary>
				public IntPtr hwnd;
				/// <summary>
				/// wFunc.
				/// </summary>
				[MarshalAs(UnmanagedType.U4)]
				public int wFunc;
				/// <summary>
				/// Source path.
				/// </summary>
				[MarshalAs(UnmanagedType.LPTStr)]
				public string pFrom;
				/// <summary>
				/// Destination path.
				/// </summary>
				[MarshalAs(UnmanagedType.LPTStr)]
				public string pTo;
				/// <summary>
				/// Flags.
				/// </summary>
				public short fFlags;
				/// <summary>
				/// Was opertaion aborted?
				/// </summary>
				[MarshalAs(UnmanagedType.Bool)]
				public bool fAnyOperationsAborted;
				/// <summary>
				/// Name mappings.
				/// </summary>
				IntPtr hNameMappings;
				/// <summary>
				/// Title of progress bar.
				/// </summary>
				[MarshalAs(UnmanagedType.LPTStr)]
				public string lpszProgressTitle;
			}

			/// <summary>
			/// SHFILEINFO.
			/// </summary>
			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
			internal struct SHFILEINFO
			{
				/// <summary>
				/// Icon handle.
				/// </summary>
				public IntPtr hIcon;
				/// <summary>
				/// Icon index.
				/// </summary>
				public int iIcon;
				/// <summary>
				/// Attributes.
				/// </summary>
				public uint dwAttributes;
				/// <summary>
				/// Display name.
				/// </summary>
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
				public string szDisplayName;
				/// <summary>
				/// Document type name.
				/// </summary>
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
				public string szTypeName;

				/// <summary>
				/// Constructor.
				/// </summary>
				/// <param name="dummy">Dummy parameter</param>
				public SHFILEINFO(bool dummy)
				{
					hIcon = IntPtr.Zero;
					iIcon = 0;
					dwAttributes = 0;
					szDisplayName = string.Empty;
					szTypeName = string.Empty;
				}
			}

			/// <summary>
			/// Styles used in the BROWSEINFO.ulFlags field.
			/// </summary>
			[Flags]
			public enum BffStyles
			{
				/// <summary>
				/// BIF_RETURNONLYFSDIRS
				/// </summary>
				RestrictToFilesystem = 0x0001,
				/// <summary>
				/// BIF_DONTGOBELOWDOMAIN
				/// </summary>
				RestrictToDomain =     0x0002,
				/// <summary>
				/// BIF_RETURNFSANCESTORS
				/// </summary>
				RestrictToSubfolders = 0x0008,
				/// <summary>
				/// BIF_EDITBOX
				/// </summary>
				ShowTextBox =          0x0010,
				/// <summary>
				/// BIF_VALIDATE
				/// </summary>
				ValidateSelection =    0x0020,
				/// <summary>
				/// BIF_NEWDIALOGSTYLE
				/// </summary>
				NewDialogStyle =       0x0040,
				/// <summary>
				/// BIF_BROWSEFORCOMPUTER
				/// </summary>
				BrowseForComputer =    0x1000,
				/// <summary>
				/// BIF_BROWSEFORPRINTER
				/// </summary>
				BrowseForPrinter =     0x2000,
				/// <summary>
				/// BIF_BROWSEINCLUDEFILES
				/// </summary>
				BrowseForEverything =  0x4000,
			}

			/// <summary>
			/// Delegate type used in BROWSEINFO.lpfn field. 
			/// </summary>
			public delegate int BFFCALLBACK ( IntPtr hwnd, int uMsg, IntPtr lParam, IntPtr lpData );

			/// <summary>
			/// BROWSEINFO.
			/// </summary>
			[StructLayout ( LayoutKind.Sequential, Pack=8 )]
			public struct BROWSEINFO
			{
				/// <summary>
				/// Handle of owner window.
				/// </summary>
				public IntPtr       hwndOwner;
				/// <summary>
				/// ID of root folder.
				/// </summary>
				public IntPtr       pidlRoot;
				/// <summary>
				/// Display name.
				/// </summary>
				public IntPtr       pszDisplayName;
				/// <summary>
				/// Dialog title.
				/// </summary>
				[MarshalAs ( UnmanagedType.LPTStr )]
				public string       lpszTitle;
				/// <summary>
				/// Flags.
				/// </summary>
				public int          ulFlags;
				/// <summary>
				/// Callback function pointer.
				/// </summary>
				[MarshalAs ( UnmanagedType.FunctionPtr )]
				public BFFCALLBACK  lpfn;
				/// <summary>
				/// lParam.
				/// </summary>
				public IntPtr       lParam;
				/// <summary>
				/// Image.
				/// </summary>
				public int          iImage;
			}

			/// <summary>
			/// Performs a file operation using the Shell functions.
			/// </summary>
			/// <param name="lpFileOp">Pointer to SHFILEOPSTRUCT structure.</param>
			/// <returns>Error code.</returns>
			[DllImport("shell32.dll", CharSet = CharSet.Auto)]
			internal static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

			[DllImport("Shell32.dll", CharSet = CharSet.Auto)]
			internal static extern int SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, int cbfileInfo, uint uFlags); 

			/// <summary>
			/// SHGetMalloc.
			/// </summary>
			/// <param name="ppMalloc"></param>
			/// <returns></returns>
			[DllImport ( "Shell32.DLL" )]
			public static extern int SHGetMalloc ( out IMalloc ppMalloc );

			/// <summary>
			/// SHGetSpecialFolderLocation.
			/// </summary>
			/// <param name="hwndOwner"></param>
			/// <param name="nFolder"></param>
			/// <param name="ppidl"></param>
			/// <returns></returns>
			[DllImport ( "Shell32.DLL" )]
			public static extern int SHGetSpecialFolderLocation (IntPtr hwndOwner, int nFolder, out IntPtr ppidl );

			/// <summary>
			/// SHGetPathFromIDList
			/// </summary>
			/// <param name="pidl"></param>
			/// <param name="Path"></param>
			/// <returns></returns>
			[DllImport ( "Shell32.DLL" )]
			public static extern int SHGetPathFromIDList (IntPtr pidl, StringBuilder Path );

			/// <summary>
			/// SHBrowseForFolder.
			/// </summary>
			/// <param name="bi"></param>
			/// <returns></returns>
			[DllImport ( "Shell32.DLL", CharSet=CharSet.Auto )]
			public static extern IntPtr SHBrowseForFolder ( ref BROWSEINFO bi );            
		}
	}
}
