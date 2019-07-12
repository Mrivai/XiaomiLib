

using System.Runtime.InteropServices;

namespace Mrivai.Pelitabangsa.Modul
{
    /// <summary>
    /// Manage sparse chunk header
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SparseChunkHeader
    {
        /// <summary>
        /// uchunk type
        /// </summary>
        public ushort uChunkType;
        /// <summary>
        /// ureserved
        /// </summary>
        public ushort uReserved1;
        /// <summary>
        /// chunk size
        /// </summary>
        public uint uChunkSize;
        /// <summary>
        /// totalsize
        /// </summary>
        public uint uTotalSize;
    }
}
