

using System.Runtime.InteropServices;

namespace Mrivai.Pelitabangsa.Modul
{
    /// <summary>
    /// Manage Sahara end transfer
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct sahara_end_transfer_packet
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
        /// image id
        /// </summary>
        public uint Image_id;
        /// <summary>
        /// status
        /// </summary>
        public uint Status;
    }
}
