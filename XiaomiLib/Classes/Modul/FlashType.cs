

using System.Runtime.InteropServices;

namespace Mrivai.Pelitabangsa.Modul
{
    /// <summary>
    /// Manage Flash type
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct FlashType
    {
        /// <summary>
        /// Flash type Clean All
        /// </summary>
        public static string CleanAll = "flash_all.bat";
        /// <summary>
        /// Flash type Save User Data
        /// </summary>
        public static string SaveUserData = "flash_all_except_storage.bat";
        /// <summary>
        /// Flash type Clean and Lock
        /// </summary>
        public static string CleanAllAndLock = "flash_all_lock.bat";
    }
}
