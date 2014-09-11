using System;
using System.Diagnostics;
using System.Runtime;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace KCS.Common.Shared
{
    public static class OSEnvironment
    {
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        public static extern bool AttachConsole(int processID);

        [DllImport("kernel32.dll")]
        public static extern int FreeConsole();

        /// <summary>
        /// Gets the current Website's folder.
        /// </summary>
        /// <returns></returns>
        public static string GetMappedApplicationPath()
        {
            string APP_PATH = System.Web.HttpContext.Current.Request.ApplicationPath.ToLower();
            if (APP_PATH == "/")      //a site
                APP_PATH = "/";
            else if (!APP_PATH.EndsWith(@"/")) //a virtual
                APP_PATH += @"/";

            string it = System.Web.HttpContext.Current.Server.MapPath(APP_PATH);
            if (!it.EndsWith(@"\"))
                it += @"\";
            return it;
        }

        /// <summary>
        /// Checks that a DLL is JIT-Optimized
        /// </summary>
        /// <param name="path">Full path to DLL or EXE</param>
        /// <returns>True if the assembly is JIT-optimized.</returns>
        public static bool IsJITOptimized(string path)
        {
            //var ass = Assembly.LoadFile(path);
            var asm = Assembly.ReflectionOnlyLoadFrom(path);
            var attribs = asm.GetReferencedAssemblies()
                .Select(Assembly.Load)
                .Where(a => a.IsDefined(typeof(DebuggableAttribute), false))
                .ToList();
            //var attribs = asm.GetCustomAttributes(typeof(DebuggableAttribute), false);
            //var attribs = asm.GetCustomAttributesData().ToList();

            // If the 'DebuggableAttribute' is not found then it is definitely an OPTIMIZED build
            if (attribs.Count > 0)
            {
                // Just because the 'DebuggableAttribute' is found doesn't necessarily mean
                // it's a DEBUG build; we have to check the JIT Optimization flag
                // i.e. it could have the "generate PDB" checked but have JIT Optimization enabled
                var da = attribs.First();
                return true;

                //var debuggableAttribute = attribs.First().AttributeType as DebuggableAttribute;
                //bool isJITOptimized = false;
                //if (debuggableAttribute != null)
                //{
                //    isJITOptimized = !debuggableAttribute.IsJITOptimizerDisabled;
                //}
                //return isJITOptimized;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Attempts to get the PUBLISH version of an application. If that fails, gets the Assembly version.
        /// </summary>
        /// <returns>System.Version.</returns>
        public static Version GetApplicationVersion()
        {
            string v = "Version=";
            try
            {
                if (AppDomain.CurrentDomain.ApplicationIdentity != null)
                {
                    string appName = AppDomain.CurrentDomain.ApplicationIdentity.FullName;
                    int startPos = appName.IndexOf(v) + v.Length;
                    int endPos = appName.IndexOf(",", startPos);
                    string versionString = appName.Substring(startPos, endPos - startPos);

                    return new Version(versionString);
                }
                else
                {
                    return Assembly.GetEntryAssembly().GetName().Version;
                }
            }
            catch
            {
                return GetApplicationVersion(Assembly.GetEntryAssembly());
            }
        }

        public static Version GetApplicationVersion(Assembly asm)
        {
            return asm.GetName().Version;
        }

        /// <summary>
        /// Retrieves the Windows operating system version.
        /// </summary>
        /// <returns></returns>
        public static Enumerations.WindowsVersions GetWindowsVersion()
        {
            Enumerations.WindowsVersions version = Enumerations.WindowsVersions.Unknown;
            System.OperatingSystem osInfo = System.Environment.OSVersion;

            // Determine the platform.
            switch (osInfo.Platform)
            {
                // Platform is Windows 95, Windows 98, 
                // Windows 98 Second Edition, or Windows Me.
                case System.PlatformID.Win32Windows:

                    switch (osInfo.Version.Minor)
                    {
                        case 0:
                            version = Enumerations.WindowsVersions.Win95;
                            break;
                        case 10:
                            if (osInfo.Version.Revision.ToString() == "2222A")
                                version = Enumerations.WindowsVersions.Win98SE;
                            else
                                version = Enumerations.WindowsVersions.Win98;
                            break;
                        case 90:
                            version = Enumerations.WindowsVersions.Me;
                            break;
                    }
                    break;

                // Platform is Windows NT 3.51, Windows NT 4.0, Windows 2000,
                // or Windows XP.
                case System.PlatformID.Win32NT:

                    switch (osInfo.Version.Major)
                    {
                        case 3:
                            version = Enumerations.WindowsVersions.NT351;
                            break;
                        case 4:
                            version = Enumerations.WindowsVersions.NT40;
                            break;
                        case 5:
                            if (osInfo.Version.Minor == 0)
                            {
                                version = Enumerations.WindowsVersions.Win2000;
                            }
                            if (osInfo.Version.Minor == 1)
                            {
                                version = Enumerations.WindowsVersions.XP;
                            }
                            break;
                        case 7:
                            version = Enumerations.WindowsVersions.Win7;        // Untested
                            break;
                    }
                    break;
            }

            return version;
        }
    }
}
