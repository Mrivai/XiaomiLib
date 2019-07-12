

using System.Runtime.InteropServices;

namespace Mrivai.Pelitabangsa.Modul
{
    /// <summary>
    /// Sahara 64 bit read data packet
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct sahara_64b_readdata_packet
    {
        /// <summary>
        /// packet command
        /// </summary>
        public uint Command;
        /// <summary>
        /// packet length
        /// </summary>
        public uint Length;
        /// <summary>
        /// packet image id
        /// </summary>
        public ulong Image_id;
        /// <summary>
        /// packet offset
        /// </summary>
        public ulong Offset;
        /// <summary>
        /// packet slength
        /// </summary>
        public ulong SLength;
    }
}
