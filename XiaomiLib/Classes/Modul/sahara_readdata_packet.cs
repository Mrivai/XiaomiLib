

using System.Runtime.InteropServices;

namespace Mrivai.Pelitabangsa.Modul
{
    /// <summary>
    /// Manage sahara read packet data
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct sahara_readdata_packet
    {
        /// <summary>
        /// commamd
        /// </summary>
        public uint Command;
        /// <summary>
        /// length
        /// </summary>
        public uint Length;
        /// <summary>
        /// image id
        /// </summary>
        public uint Image_id;
        /// <summary>
        /// offset
        /// </summary>
        public uint Offset;
        /// <summary>
        /// slength
        /// </summary>
        public uint SLength;
    }
}

