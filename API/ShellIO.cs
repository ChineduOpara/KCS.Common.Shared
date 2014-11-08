using System;
using System.Security;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Permissions;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace KCS.Common.Shared
{
	/// <summary>
	/// Wrapper for P/INVOKE methods in shell32.dll
	/// </summary>
	public static class ShellIO
	{ 
		/// <summary>
		/// Enumerates the icon sizes.
		/// </summary>
		public enum FileIconSize 
		{
			/// <summary>
			/// 16x16 pixels
			/// </summary>
			Small,

			/// <summary>
			/// 32x32 pixels
			/// </summary>
			Large
		}

		/// <summary>
		/// Wildcard filter for all files.
		/// </summary>
		public const string AllFilesDialogFilter = "All Files (*.*)";

		/// <summary>
		/// Wildcard filter for all bitmap image files.
		/// </summary>
		public const string ImagesDialogFilter = "All Images (*.jpg,*.jpeg,*.gif,*.png,*.tif,*.tiff,*.bmp)|JPEG (*.jpg, *.jpeg)|CompuServe Graphics Interchange (*.gif)|Portable Network Graphics (*.png)|Tagged Image Format (*.tif, *.tiff)|Windows Bitmap (*.bmp)|All Files (*.*)";

		/// <summary>
		/// Wildcard filter for common office formats.
		/// </summary>
		public const string CommonDocumentsDialogFilter = "All Common Documents Formats (*.doc,*.rtf,*.xls,*.ppt,*.txt,*.csv,*.zip,*.mdb,*.pdf)|Microsoft Word Documents (*.doc,*.rtf)|Microsoft Excel Workbooks (*.xls)|Microsoft PowerPoint Presentations (*.ppt)|Plain Text (*.txt,*.csv)|Zip Compressed (*.zip)|Microsoft Access Databases (*.mdb)|Adobe PDF (*.pdf)|All Files (*.*)";

		/// <summary>
		/// Copies a file from one place to another, using the Shell file copying routines.
		/// </summary>
		/// <param name="form">Parent form.</param>
		/// <param name="source">Source path.</param>
		/// <param name="destination">Destination path.</param>
		/// <param name="aborted">Was the operation aborted?</param>
		public static void CopyFile(Form form, string source, string destination, ref bool aborted)
		{
			CopyFile(form.Handle, source, destination, "Copying File " + System.IO.Path.GetFileName(source) + "...", ref aborted);
		}

		/// <summary>
		/// Copies a file from one place to another, using the Shell file copying routines.
		/// </summary>
		/// <param name="formHandle">Parent form's handle.</param>
		/// <param name="source">Source path.</param>
		/// <param name="destination">Destination path.</param>
		/// <param name="progressTitle">Title of progress bar.</param>
		/// <param name="aborted">Was the operation aborted?</param>
		public static void CopyFile(IntPtr formHandle, String source, String destination, String progressTitle, ref Boolean aborted)
		{
			int retval = 0;
			Win32API.Shell32.SHFILEOPSTRUCT shf = new Win32API.Shell32.SHFILEOPSTRUCT();
			shf.wFunc = Win32API.Shell32.FO_COPY;
			shf.fFlags = Win32API.Shell32.FOF_SIMPLEPROGRESS;
			shf.lpszProgressTitle = progressTitle;
			shf.hwnd = formHandle;
			shf.pFrom = source + "\0";
			shf.pTo = destination + "\0";
			shf.fAnyOperationsAborted = false;
			retval = Win32API.Shell32.SHFileOperation(ref shf);
			aborted = (retval != 0) || shf.fAnyOperationsAborted;
		}

		/// <summary>
		/// Deletes files.
		/// </summary>
		/// <param name="form">Parent form.</param>
		/// <param name="paths">Full paths to files or folders (no mixing and matching).</param>
		/// <param name="aborted">Was the operation aborted?</param>
        public static void DeleteFiles(IntPtr windowHandle, IEnumerable<string> paths, String progressTitle, ref Boolean aborted)
		{
            var delimitedPaths = string.Join("\0", paths.ToArray());
            
			int retval = 0;
			Win32API.Shell32.SHFILEOPSTRUCT shf = new Win32API.Shell32.SHFILEOPSTRUCT();
			shf.wFunc = Win32API.Shell32.FO_DELETE;
			shf.fFlags = Win32API.Shell32.FOF_NOCONFIRMATION;
            shf.lpszProgressTitle = progressTitle;
            if (windowHandle != IntPtr.Zero)
            {
                shf.hwnd = windowHandle;
            }
            shf.pFrom = delimitedPaths + "\0";
			shf.fAnyOperationsAborted = false;
			retval = Win32API.Shell32.SHFileOperation(ref shf);
			aborted = (retval != 0) || shf.fAnyOperationsAborted;
		}

		/// <summary>
		/// Gets the icon associated with a given file. This overload assumes that we want the small icon size.
		/// </summary>
		/// <param name="path">Path to file.</param>
		/// <returns>Icon associated with the given file.</returns>
		public static Icon GetFileIcon(String path) 
		{
			return GetFileIcon(path, FileIconSize.Small);
		}

		/// <summary>
		/// Gets the icon associated with a given file.
		/// </summary>
		/// <param name="fullpath">Path to file.</param>
		/// <param name="iconSize">Size of icon to return.</param>
		/// <returns>Icon associated with the given file.</returns>
		public static Icon GetFileIcon(String fullpath, FileIconSize iconSize) 
		{
			int retval = 0;
			Win32API.Shell32.SHFILEINFO info = new Win32API.Shell32.SHFILEINFO();
			uint flags = Win32API.Shell32.SHGFI_USEFILEATTRIBUTES | Win32API.Shell32.SHGFI_ICON;

			if (iconSize == FileIconSize.Small)
			{
				flags |= Win32API.Shell32.SHGFI_SMALLICON;
			} 
			else if (iconSize == FileIconSize.Large)
			{
				flags |= Win32API.Shell32.SHGFI_LARGEICON;
			}

			retval = Win32API.Shell32.SHGetFileInfo(fullpath, Win32API.Shell32.FILE_ATTRIBUTE_NORMAL, ref info, Marshal.SizeOf(info), flags);
			if (retval == 0) return null;

			return Icon.FromHandle(info.hIcon);
		}		

		/// <summary>
		/// Gets the document type of a file.
		/// </summary>
		/// <param name="fullpath">Full path to file.</param>
		/// <returns>Document type of file.</returns>
		public static String GetFileType(String fullpath)
		{
			int retval = 0;
			Win32API.Shell32.SHFILEINFO info = new Win32API.Shell32.SHFILEINFO(true);

			retval = Win32API.Shell32.SHGetFileInfo(fullpath, 0, ref info, Marshal.SizeOf(info), Win32API.Shell32.SHGFI_TYPENAME);
			if (retval == 0)
				return "Unknown";		// Error occured
			else
				return info.szTypeName;			
		}

		/// <summary>
		/// Gets the path to the icon associated with a file.
		/// </summary>
		/// <param name="filePath">Path to document that we need an image for.</param>
		public static String GetFileIconPath(string filePath)
		{
			string tempPath = Path.GetDirectoryName(Path.GetTempFileName());
			return GetFileIconPath(filePath, tempPath);
		}

		/// <summary>
		/// Gets the path to the icon associated with a file.
		/// </summary>
		/// <param name="filePath">Path to document that we need an image for.</param>
		/// <param name="targetDirectory">Place in which to put the image.</param>
		/// <remarks>
		/// When the icon for a new file type is needed, add it to the DocIcons folder
		/// and recompile the library.
		/// </remarks>
		public static String GetFileIconPath(String filePath, String targetDirectory)
		{
			string path = Path.Combine(targetDirectory, GetFileType(filePath));
			Stream stream = null;
			Bitmap bitmap = null;
			Type type = typeof(ShellIO);
			Assembly ass = null;
			string[] names = null;
			string filename = string.Empty;

			// Now get the path to the correct icon.
			path = Path.ChangeExtension(path, System.Drawing.Imaging.ImageFormat.Gif.ToString());
			if (!File.Exists(path))
			{	
				filename = Path.GetFileName(path);
				ass = Assembly.GetAssembly(type);
				names = ass.GetManifestResourceNames();

				stream = ass.GetManifestResourceStream(type.Namespace + ".DocIcons." + filename.ToLower());
				if (stream == null) stream = ass.GetManifestResourceStream(type.Namespace + ".DocIcons.missing file icon.gif");

				bitmap = Image.FromStream(stream) as Bitmap;
				bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Gif);
				bitmap.Dispose();
				bitmap = null;
			}
			return path;
		}

		[DllImport("shell32.dll", EntryPoint = "FindExecutable")]
		private static extern long FindExecutableA(string lpFile, string lpDirectory, StringBuilder lpResult);

		public static string FindExecutable(string fullPath)
		{
			StringBuilder objResultBuffer = new StringBuilder(1024);
			long lngResult = 0;

			lngResult = FindExecutableA(fullPath, string.Empty, objResultBuffer);

			if (lngResult >= 32)
			{
				return objResultBuffer.ToString();
			}

			return string.Format("Error: ({0})", lngResult);
		}

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        static extern bool ShellExecuteEx(ref Win32API.Shell32.SHELLEXECUTEINFO lpExecInfo);

        private const int SW_SHOW = 5;
        private const uint SEE_MASK_INVOKEIDLIST = 12;
        public static bool ShowFileProperties(string filePath)
        {
            var info = new Win32API.Shell32.SHELLEXECUTEINFO();
            info.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(info);
            info.lpVerb = "properties";
            info.lpFile = filePath;
            info.nShow = SW_SHOW;
            info.fMask = SEE_MASK_INVOKEIDLIST;
            return ShellExecuteEx(ref info);
        }

        /// <summary>
        /// Runs the command processor, passing parameters.
        /// </summary>
        /// <param name="fullpath">Path to file to run/open.</param>
        /// <param name="errorDialogParentHandle">Handle of parent window for error message.</param>
        static public Process RunCommandProcess(string fullpath, string arguments = "", System.IntPtr? errorDialogParentHandle = null)
        {
            var proc = new Process();
            proc.StartInfo.WorkingDirectory = Path.GetDirectoryName(fullpath);
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.Arguments = string.Format("/K \"{0}\" {1}", fullpath, Strings.ConvertToString(arguments));
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            if (errorDialogParentHandle.HasValue)
            {
                proc.StartInfo.ErrorDialog = true;
                proc.StartInfo.ErrorDialogParentHandle = errorDialogParentHandle.Value;
            }
            proc.Start();

            return proc;
        }

		/// <summary>
		/// Runs a process using the shell.
		/// </summary>
		/// <param name="fullpath">Path to file to run/open.</param>
		/// <param name="errorDialogParentHandle">Handle of parent window for error message.</param>
        static public Process RunShellProcess(string fullpath, string arguments = "", System.IntPtr? errorDialogParentHandle = null)
		{
			var proc = new Process();
            proc.StartInfo.WorkingDirectory = Path.GetDirectoryName(fullpath);
			proc.StartInfo.UseShellExecute = true;
			proc.StartInfo.FileName = "\"" + fullpath + "\"";
            proc.StartInfo.Arguments = Strings.ConvertToString(arguments);
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            if (errorDialogParentHandle.HasValue)
            {
                proc.StartInfo.ErrorDialog = true;
                proc.StartInfo.ErrorDialogParentHandle = errorDialogParentHandle.Value;
            }
			proc.Start();

            return proc;
		}

		///// <summary>
		///// Closes processes that start with a particular name.
		///// </summary>
		///// <param name="name">Process name.</param>
		///// <param name="timeoutSeconds">Time to give each instance to close.</param>
		///// <returns></returns>
		//public static int CloseProcesses(string name, uint timeoutSeconds)
		//{
		//    return CloseOrKillProcesses(name, timeoutSeconds, false);
		//}

		/// <summary>
		/// Closes or kills all processes that start with a particular name.
		/// </summary>
		/// <param name="name">Process name.</param>
		public static int KillProcesses(string name, int excludeProcessID)
		{
			return CloseOrKillProcesses(name, excludeProcessID, 0, true);
		}

		/// <summary>
		/// Closes or kills all processes that start with a particular name.
		/// </summary>
		/// <param name="name">Process name.</param>
		/// <param name="timeoutSeconds">Time to give each instance to close. 0 means wait indefinitely.</param>
		/// <param name="force">If true, process is immediately terminated, with extreme prejudice.</param>
		/// <returns>Number of processes killed.</returns>
		private static int CloseOrKillProcesses(string name, int excludeProcessID, uint timeoutSeconds, bool force)
		{
			int counter = 0;
			name = name.ToLower();

			Process[] arr = Process.GetProcesses();
			for (int i = 0; i < arr.Length; i++ )
			{
				Process p = arr[i];
				if (excludeProcessID > 0 && p.Id == excludeProcessID) continue;
				if (p.ProcessName.ToLower().StartsWith(name) && p.MainModule.ModuleName.ToLower() == name + ".exe")
				{
					if (force)
					{
						p.Kill();
					}
					else
					{
						// Haven't gotten this section to work properly yet.
						//p.CloseMainWindow();
						//p.WaitForExit();
						//int timeout = timeoutSeconds == 0 ? int.MaxValue : 1000 * (int)timeoutSeconds;						

						//if (timeout == int.MaxValue)			// This means wait indefinitely.
						//{
						//    while (!p.HasExited)
						//    {
						//        Application.DoEvents();
						//        System.Threading.Thread.Sleep(500);
						//    }
						//}
						//else
						//{
						//    DateTime dateStart = DateTime.Now;
						//    while (!p.HasExited && (DateTime.Now - dateStart).TotalMilliseconds < timeout)
						//    {
						//        Application.DoEvents();
						//        System.Threading.Thread.Sleep(500);
						//    }
						//}
					}
				}
				counter++;
			}

			return counter;
		}

        /// <summary>
        /// Return a file as a byte array, without deleting the original.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <returns>Byte array.</returns>
        public static byte[] GetFileBytes(string path)
        {
            bool deleted;
            return GetFileBytes(path, false, out deleted);
        }

        /// <summary>
        /// Return the file as a byte array, optionally deleting the original.
        /// </summary>
        /// <param name="path">Complete path to the file.</param>
        /// <param name="deleteOriginal">If true, deletes the original file.</param>
        /// <returns>The raw bytes of the file.</returns>
        public static byte[] GetFileBytes(string path, bool deleteOriginal, out bool deleted)
        {
            deleted = false;
            byte[] bytes = File.ReadAllBytes(path);

            if (deleteOriginal)
            {
                try
                {
                    File.Delete(path);
                    deleted = true;
                }
                catch
                {
                    // Don't do anything right now.
                }
            }

            return bytes;
        }

		/// <summary>
		/// Confirms that the characters in a Directory path are good, by attempting to get information
		/// on the directory.
		/// </summary>
		/// <param name="path">Directory path to check.</param>
		/// <returns>True if the characters are good.</returns>
		static public Boolean ValidateDirectoryNameChars(String path)
		{
			try
			{
				DirectoryInfo d = new DirectoryInfo(path);
				d = null;
				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Confirms that the characters in a File path are good, by attempting to get information
		/// on the File.
		/// </summary>
		/// <param name="path">File path to check.</param>
		/// <returns>True if the characters are good.</returns>
		static public Boolean ValidateFileNameChars(String path)
		{
			try
			{
				FileInfo f = new FileInfo(path);
				f = null;
				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Confirms that the directory is writable.
		/// </summary>
		/// <param name="path">Directory path to check.</param>
		/// <returns>True if the directory is writable are good.</returns>
		static public Boolean IsDirectoryWritable(string path)
		{
			FileStream stream = null;
			try
			{
				path = Path.Combine(path, Path.GetFileName(Path.GetTempFileName()));
				stream = File.Create(path);
				stream.Close();
				File.Delete(path);
				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
				if (stream != null) stream.Close();
			}
		}
		
		public static bool CanReadAndWrite(string directoryOrFilePath)
		{
			System.IO.FileStream fs = null;
			try
			{
                if (File.Exists(directoryOrFilePath))
                {
                    fs = System.IO.File.Open(directoryOrFilePath, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
                }
                else
                {
                    if (Directory.Exists(directoryOrFilePath))
                    {
                        string tempFile = Path.GetTempFileName();
                        directoryOrFilePath = Path.Combine(directoryOrFilePath, Path.GetFileName(Path.GetTempFileName()));
                        fs = System.IO.File.Open(directoryOrFilePath, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write, System.IO.FileShare.None);
                        fs.Close();
                        File.Delete(tempFile);
                        File.Delete(directoryOrFilePath);
                    }
                }			
				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
				if (fs != null)
				{
					fs.Close();
                    fs = null;
				}
			}
		}

		/// <summary>
		/// Attempts to connect a Network Drive. This method needs to handle the error messages returned.
		/// </summary>
		/// <param name="localDriveLetter"></param>
		/// <param name="folder"></param>
		/// <returns></returns>
		public static string ConnectNetworkDrive(string folder)
		{
			string driveLetter = GetNextAvailableDriveLetter();
			Win32API.Mpr.NETRESOURCE nr;

			if (!driveLetter.EndsWith(":"))
			{
				driveLetter += ":";
			}
			if (folder.EndsWith("\\"))
			{
				folder = folder.TrimEnd('\\');
			}

			nr = new Win32API.Mpr.NETRESOURCE();
			nr.dwType = Win32API.Mpr.RESOURCETYPE_DISK;
			nr.lpLocalName = driveLetter;
			nr.lpRemoteName = folder;
			nr.lpProvider = null;

			uint result = Win32API.Mpr.WNetAddConnection2(ref nr, null, null, 0);
			return (result == 0) ? driveLetter : string.Empty;
		}

		/// <summary>
		/// Disconnects the network drive.
		/// </summary>
		/// <param name="localDriveLetter"></param>
		/// <returns></returns>
		public static bool DisconnectNetworkDrive(string localDriveLetter)
		{
			if (!localDriveLetter.EndsWith(":"))
			{
				localDriveLetter += ":";
			}

			uint result = Win32API.Mpr.WNetCancelConnection2(localDriveLetter, Win32API.Mpr.CONNECT_UPDATE_PROFILE, true);
			return result == 0;
		}

		public static string GetNextAvailableDriveLetter()
		{
			// build a string collection representing the alphabet
			List<char> alphabet = new List<char>(26); 

			int lowerBound = Convert.ToInt16('a');
			int upperBound = Convert.ToInt16('z');
			for(int i = lowerBound; i < upperBound; i++)
			{
				alphabet.Add((char)i);
			}

			// get all current drives
			DriveInfo[] drives = DriveInfo.GetDrives();
			foreach (DriveInfo drive in drives)
			{
				alphabet.Remove(Convert.ToChar(drive.Name.Substring(0, 1).ToLower())); 
			}

			if (alphabet.Count > 0)
			{
				return alphabet[0].ToString();
			}
			else
			{
				return string.Empty;
			}
		}        

		#region Taken from http://msdn.microsoft.com/en-us/magazine/cc163851.aspx
		public static void CopyFile(FileInfo source, FileInfo destination)
		{
			CopyFile(source, destination, CopyFileOptions.None);
		}

		public static void CopyFile(FileInfo source, FileInfo destination,
			CopyFileOptions options)
		{
			CopyFile(source, destination, options, null);
		}

		public static void CopyFile(FileInfo source, FileInfo destination,
			CopyFileOptions options, CopyFileCallback callback)
		{
			CopyFile(source, destination, options, callback, null);
		}

		public static void CopyFile(FileInfo source, FileInfo destination,
			CopyFileOptions options, CopyFileCallback callback, object state)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (destination == null)
				throw new ArgumentNullException("destination");
			if ((options & ~CopyFileOptions.All) != 0)
				throw new ArgumentOutOfRangeException("options");

			new FileIOPermission(
				FileIOPermissionAccess.Read, source.FullName).Demand();
			new FileIOPermission(
				FileIOPermissionAccess.Write, destination.FullName).Demand();

			CopyProgressRoutine cpr = callback == null ?
				null : new CopyProgressRoutine(new CopyProgressData(
					source, destination, callback, state).CallbackHandler);

			bool cancel = false;
			if (!CopyFileEx(source.FullName, destination.FullName, cpr,
				IntPtr.Zero, ref cancel, (int)options))
			{
				throw new IOException(new Win32Exception().Message);
			}
		}

		private class CopyProgressData
		{
			private FileInfo _source = null;
			private FileInfo _destination = null;
			private CopyFileCallback _callback = null;
			private object _state = null;

			public CopyProgressData(FileInfo source, FileInfo destination,
				CopyFileCallback callback, object state)
			{
				_source = source;
				_destination = destination;
				_callback = callback;
				_state = state;
			}

			public int CallbackHandler(
				long totalFileSize, long totalBytesTransferred,
				long streamSize, long streamBytesTransferred,
				int streamNumber, int callbackReason,
				IntPtr sourceFile, IntPtr destinationFile, IntPtr data)
			{
				return (int)_callback(_source, _destination, _state,
					totalFileSize, totalBytesTransferred);
			}
		}

		private delegate int CopyProgressRoutine(
			long totalFileSize, long TotalBytesTransferred, long streamSize,
			long streamBytesTransferred, int streamNumber, int callbackReason,
			IntPtr sourceFile, IntPtr destinationFile, IntPtr data);

		[SuppressUnmanagedCodeSecurity]
		[DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool CopyFileEx(
			string lpExistingFileName, string lpNewFileName,
			CopyProgressRoutine lpProgressRoutine,
			IntPtr lpData, ref bool pbCancel, int dwCopyFlags);

		public delegate CopyFileCallbackAction CopyFileCallback(
			FileInfo source, FileInfo destination, object state,
			long totalFileSize, long totalBytesTransferred);

		public enum CopyFileCallbackAction
		{
			Continue = 0,
			Cancel = 1,
			Stop = 2,
			Quiet = 3
		}

		[Flags]
		public enum CopyFileOptions
		{
			None = 0x0,
			FailIfDestinationExists = 0x1,
			Restartable = 0x2,
			AllowDecryptedDestination = 0x8,
			All = FailIfDestinationExists | Restartable | AllowDecryptedDestination
		}
		#endregion

		//		/// <summary>
//		/// Retrieves the size of a directory.
//		/// </summary>
//		/// <param name="directoryPath">Directory to analyze.</param>
//		/// <returns>Size of the given directory.</returns>
//		public static long GetDirectorySize(string directoryPath)
//		{
//			return GetDirectorySize(path, new string[]{});
//		}

//		/// <summary>
//		/// Retrieves the size of a directory, but excluding certain files.
//		/// </summary>
//		/// <param name="path">Directory to analyze.</param>
//		/// <param name="excludeExtensions">Extensions to exclude from the size calculation.</param>
//		/// <returns>Size of the given directory.</returns>
//		public static long GetDirectorySize(string path, String[] excludeExtensions) 
//		{
//			long folderSize = 0;
//			string pattern = string.Empty;
//			string str = string.Empty;
//			DirectoryInfo dir = new DirectoryInfo(path);
//			bool idDir = false;
//
//			// Prepare the search pattern
//			foreach(string ext in excludeExtensions)
//			{				
//				if (ext == WildCardAllFiles) continue;
//			
//				str = ext;
//				if (str.StartsWith("*")) str = str.Substring(1);
//				pattern = pattern + "\\" + str + "$|\\";
//			}
//			if (pattern.Length > 0)
//			{
//				pattern = pattern.Substring(0, pattern.Length - 2);
//				pattern = pattern.Replace("\\\\", "\\");
//			}
//
//			Regex r = new Regex(pattern, RegexOptions.IgnoreCase|RegexOptions.Compiled); 
//			FileSystemInfo[] entries = dir.GetFileSystemInfos(); 
//			foreach(FileSystemInfo f in entries)
//			{
//				isDir = ((f.Attributes & FileAttributes.Directory) == FileAttributes.Directory);
//				if (isDir)
//				{
//				}
//				else
//				{
//					if (pattern.Length == 0 || r.IsMatch(f.Extension))
//						folderSize += File.get
//				}
//			}
//
//			return folderSize;
//		}
	}
}

