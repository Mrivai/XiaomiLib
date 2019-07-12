


using System.Runtime.InteropServices;

namespace Mrivai.Pelitabangsa.Modul
{
    /// <summary>
    /// Define Storage type
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct Storage
    {
        /// <summary>
        /// ufs
        /// </summary>
        public static string ufs = "ufs";
        /// <summary>
        /// emmc
        /// </summary>
        public static string emmc = "emmc";
    }
}
