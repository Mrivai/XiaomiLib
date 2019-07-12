

using System.Runtime.InteropServices;

namespace Mrivai.Pelitabangsa.Modul
{
    /// <summary>
    /// Manage sparse image header
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SparseImageHeader
    {
        /// <summary>
        /// sparse umagic
        /// </summary>
        public uint uMagic;
        /// <summary>
        /// umagic major version
        /// </summary>
        public ushort uMajorVersion;
        /// <summary>
        /// sparse minor version
        /// </summary>
        public ushort uMinorVersion;
        /// <summary>
        /// sparse ufile header size
        /// </summary>
        public ushort uFileHeaderSize;
        /// <summary>
        /// sparse uchunk header size
        /// </summary>
        public ushort uChunkHeaderSize;
        /// <summary>
        /// sparse ublock  size
        /// </summary>
        public uint uBlockSize;
        /// <summary>
        /// sparse utotal block
        /// </summary>
        public uint uTotalBlocks;
        /// <summary>
        /// sparse total chunks
        /// </summary>
        public uint uTotalChunks;
        /// <summary>
        /// sparse uimage checksum
        /// </summary>
        public uint uImageChecksum;
    }
}
