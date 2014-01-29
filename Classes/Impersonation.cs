using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Security.Principal;
using System.Security.Permissions;
using System.Runtime.InteropServices;

namespace KCS.Common.Shared
{
    /// <summary>
    ///  Account Impersonation code that allows user to run a portion of a code under another account. Works on Windows XP. 
    ///  Make sure to call UndoImpersonation()
    ///  Original code has been taken from MSDN and modified to fit DV2 needs
    /// </summary>
    [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
    #region Current Code
    public class Impersonation
    {
        /// <summary>
        /// When impersonation fails i.e. returns False, returns the error code 
        /// </summary>
        public int ErrorCode { get; set; }
        public Exception Exception { get; set; }
        #region Impersonation Related Code
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        //This parameter causes LogonUser to create a primary token
        private const int LOGON32_LOGON_INTERACTIVE = 2;

        //private static WindowsImpersonationContext _impersonationContext;
        private WindowsImpersonationContext _impersonationContext = null;

        [DllImport("advapi32.dll")]
        private static extern int LogonUserA(String lpszUserName,
            String lpszDomain,
            String lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int DuplicateToken(IntPtr hToken,
            int impersonationLevel,
            ref IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool RevertToSelf();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool CloseHandle(IntPtr handle);

        public bool ImpersonateValidUser(String userName, String domain, String password)
        {
            if (!Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["isImpersonate"]))
            {
                return true;
            }

            bool isImpersonated = false;
            WindowsIdentity tempWindowsIdentity;
            IntPtr token = IntPtr.Zero;
            IntPtr tokenDuplicate = IntPtr.Zero;

            try
            {
                if (RevertToSelf())
                {
                    if (LogonUserA(userName, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref token) != 0)
                    {
                        if (DuplicateToken(token, 2, ref tokenDuplicate) != 0)
                        {
                            tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                            _impersonationContext = tempWindowsIdentity.Impersonate();
                            if (_impersonationContext != null)
                            {
                                CloseHandle(token);
                                CloseHandle(tokenDuplicate);

                                isImpersonated = true;
                                return isImpersonated;
                            }
                        }
                    }
                    else
                    {
                        //TODO - log on error
                        //int ret = Marshal.GetLastWin32Error();
                        //Console.WriteLine("LogonUser failed with error code : {0}", ret);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorCode = Marshal.GetLastWin32Error();
                this.Exception = ex;
            }
            finally
            {
                if (token != IntPtr.Zero)
                    CloseHandle(token);
                if (tokenDuplicate != IntPtr.Zero)
                    CloseHandle(tokenDuplicate);
            }
            return isImpersonated;
        }

        public void UndoImpersonation()
        {
            //If Impersonation fails, skip Undo()
            if (_impersonationContext != null)
            {
                _impersonationContext.Undo();
            }
        }
        #endregion
    }

    #endregion

    #region Test
    //public class Impersonation
    //{
    //    WindowsIdentity wi = null;
    //    WindowsImpersonationContext _impersonationContext = null;
    //    /// <summary>
    //    /// When impersonation fails i.e. returns False, returns the error code 
    //    /// </summary>
    //    public int ErrorCode { get; set; }
    //    public Exception Exception { get; set; }


    //    public bool ImpersonateValidUser(String userName, String domain, String password)
    //    {
    //        bool isImpersonated = false;
    //        try
    //        {
    //            string usrDomain = string.Format("{0}@{1}", userName, domain);
    //            usrDomain = "9OPARA7@levi";
    //            wi = new WindowsIdentity(usrDomain);
    //            _impersonationContext = wi.Impersonate();
    //            if (_impersonationContext != null) isImpersonated = true;

    //        }
    //        catch (Exception ex)
    //        {
    //            this.Exception = ex;
    //            if (_impersonationContext != null) _impersonationContext.Undo();
    //        }

    //        return isImpersonated;
    //    }

    //    public void UndoImpersonation()
    //    {
    //        //If Impersonation fails, skip Undo()
    //        if (_impersonationContext != null)
    //        {
    //            _impersonationContext.Undo();
    //        }
    //    }
    //} 
    #endregion
}
