

using System.Runtime.InteropServices;

namespace Mrivai.Pelitabangsa.Modul
{
    /// <summary>
    /// Manage sagara hello packet
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct sahara_hello_packet
    {
        /// <summary>
        /// Command
        /// </summary>
        public uint Command;
        /// <summary>
        /// length
        /// </summary>
        public uint Length;
        /// <summary>
        /// version
        /// </summary>
        public uint Version;
        /// <summary>
        /// min Version
        /// </summary>
        public uint Version_min;
        /// <summary>
        /// next Command max
        /// </summary>
        public uint Max_Command_Length;
        /// <summary>
        /// mode
        /// </summary>
        public uint Mode;
        /// <summary>
        /// resrved
        /// </summary>
        public uint[] Reserved;
    }
}
