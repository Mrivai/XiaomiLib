using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Mrivai
{
    public class Driver
    {
        public Driver()
        {
        }

        [DllImport("kernel32.dll", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
        public static extern uint GetWindowsDirectory(StringBuilder path, int pathLen);

        [DllImport("setupapi.dll", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
        private static extern bool SetupCopyOEMInf(string SourceInfFileName, string OEMSourceMediaLocation, OemSourceMediaType OEMSourceMediaType, OemCopyStyle CopyStyle, StringBuilder DestinationInfFileName, int DestinationInfFileNameSize, int RequiredSize, out string DestinationInfFileNameComponent);

        public static string SetupOEMInf(string infPath, out string destinationInfFileName, out string destinationInfFileNameComponent, out bool success)
        {
            string message = "";
            StringBuilder stringBuilder = new StringBuilder(260);
            success = Driver.SetupCopyOEMInf(infPath, "", OemSourceMediaType.SPOST_PATH, OemCopyStyle.SP_COPY_NEWER, stringBuilder, stringBuilder.Capacity, 0, out destinationInfFileNameComponent);
            if (!success)
            {
                message = (new Win32Exception(Marshal.GetLastWin32Error())).Message;
            }
            destinationInfFileName = stringBuilder.ToString();
            return message;
        }

        [DllImport("setupapi.dll", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
        private static extern bool SetupUninstallOEMInf(string InfFileName, SetupUOInfFlags Flags, IntPtr Reserved);

        public static string UninstallInf(string infFileName, out bool success)
        {
            string message = "";
            success = SetupUninstallOEMInf(infFileName, SetupUOInfFlags.SUOI_FORCEDELETE, IntPtr.Zero);
            if (!success)
            {
                message = (new Win32Exception(Marshal.GetLastWin32Error())).Message;
            }
            return message;
        }

        private static bool SetupUninstallOEMInf(string infFileName, object sUOI_FORCEDELETE, IntPtr zero)
        {
            throw new NotImplementedException();
        }

        public static string UninstallInfByText(string text, out bool success)
        {
            success = false;
            StringBuilder stringBuilder = new StringBuilder(256);
            if (Driver.GetWindowsDirectory(stringBuilder, stringBuilder.Capacity) == 0)
            {
                int lastWin32Error = Marshal.GetLastWin32Error();
                return string.Concat("UninstallInfByText: GetWindowsDirectory failed with system error code ", lastWin32Error.ToString());
            }
            string str = string.Concat(stringBuilder.ToString(), "\\inf");
            string[] files = Directory.GetFiles(str, "*.inf");
            string str1 = "";
            string[] strArrays = files;
            for (int i = 0; i < (int)strArrays.Length; i++)
            {
                string str2 = strArrays[i];
                if (File.ReadAllText(str2).Contains(text))
                {
                    string str3 = str2.Remove(0, str2.LastIndexOf('\\') + 1);
                    if (Driver.SetupUninstallOEMInf(str3, SetupUOInfFlags.SUOI_FORCEDELETE, IntPtr.Zero))
                    {
                        success = true;
                    }
                    else
                    {
                        string str4 = str1;
                        string[] strArrays1 = new string[] { str4, "UninstallInfByText: SetupUninstallOEMInf failed with code ", null, null, null };
                        strArrays1[2] = Marshal.GetLastWin32Error().ToString();
                        strArrays1[3] = " for file ";
                        strArrays1[4] = str3;
                        str1 = string.Concat(strArrays1);
                    }
                }
            }
            if (str1.Length > 0)
            {
                return str1;
            }
            return null;
        }

        public enum SetupUOInfFlags : uint
        {
            NONE,
            SUOI_FORCEDELETE
        }
        public enum OemSourceMediaType
        {
            SPOST_NONE,
            SPOST_PATH,
            SPOST_URL,
            SPOST_MAX
        }
        public enum OemCopyStyle
        {
            SP_COPY_DELETESOURCE = 1,
            SP_COPY_REPLACEONLY = 2,
            SP_COPY_NEWER = 4,
            SP_COPY_NEWER_OR_SAME = 4,
            SP_COPY_NOOVERWRITE = 8,
            SP_COPY_NODECOMP = 16,
            SP_COPY_LANGUAGEAWARE = 32,
            SP_COPY_SOURCE_ABSOLUTE = 64,
            SP_COPY_SOURCEPATH_ABSOLUTE = 128,
            SP_COPY_IN_USE_NEEDS_REBOOT = 256,
            SP_COPY_FORCE_IN_USE = 512,
            SP_COPY_NOSKIP = 1024,
            SP_FLAG_CABINETCONTINUATION = 2048,
            SP_COPY_FORCE_NOOVERWRITE = 4096,
            SP_COPY_FORCE_NEWER = 8192,
            SP_COPY_WARNIFSKIP = 16384,
            SP_COPY_NOBROWSE = 32768,
            SP_COPY_NEWER_ONLY = 65536,
            SP_COPY_SOURCE_SIS_MASTER = 131072,
            SP_COPY_OEMINF_CATALOG_ONLY = 262144,
            SP_COPY_REPLACE_BOOT_FILE = 524288,
            SP_COPY_NOPRUNE = 1048576
        }
    }
}
