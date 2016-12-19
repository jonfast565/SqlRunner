using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using SqlRunner.Exceptions;

namespace SqlRunner.Utilities
{
    internal class WindowsIdentityImpersonator
    {
        public const int Logon32LogonInteractive = 2;
        public const int Logon32ProviderDefault = 0;
        // in case that multiple impersonation attempts are running on the same thread...
        public static object ImpersonationLock = new object();

        private WindowsImpersonationContext _impersonationContext;

        [DllImport("advapi32.dll")]
        public static extern int LogonUserA(string lpszUserName,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int DuplicateToken(IntPtr hToken,
            int impersonationLevel,
            ref IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RevertToSelf();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);

        public void ImpersonateUserAndRunAction(string domain, string username, string password,
            Action actionToBeRunInImpersonatedContext)
        {
            lock (ImpersonationLock)
            {
                if (ImpersonateValidUserInternal(username, domain, password))
                {
                    actionToBeRunInImpersonatedContext();
                    UndoImpersonationInternal();
                }
                else
                {
                    throw new SqlRunnerException($"Impersonation of {domain}\\{username} with {password} failed");
                }
            }
        }

        private bool ImpersonateValidUserInternal(string userName, string domain, string password)
        {
            var token = IntPtr.Zero;
            var tokenDuplicate = IntPtr.Zero;

            if (RevertToSelf())
            {
                if (LogonUserA(userName, domain, password, Logon32LogonInteractive,
                    Logon32ProviderDefault, ref token) != 0)
                {
                    if (DuplicateToken(token, 2, ref tokenDuplicate) != 0)
                    {
                        var tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                        _impersonationContext = tempWindowsIdentity.Impersonate();
                        if (_impersonationContext != null)
                        {
                            CloseHandle(token);
                            CloseHandle(tokenDuplicate);
                            return true;
                        }
                    }
                }
            }
            if (token != IntPtr.Zero)
                CloseHandle(token);
            if (tokenDuplicate != IntPtr.Zero)
                CloseHandle(tokenDuplicate);
            return false;
        }

        private void UndoImpersonationInternal()
        {
            _impersonationContext.Undo();
        }
    }
}