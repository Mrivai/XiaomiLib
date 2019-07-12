
using System.Runtime.InteropServices;

namespace Mrivai.Pelitabangsa.Modul
{
    /// <summary>
    /// Manage Sahara packet
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct sahara_packet
    {
        /// <summary>
        /// Command
        /// </summary>
        public uint Command;
        /// <summary>
        /// length
        /// </summary>
        public uint Length;
    }
}
