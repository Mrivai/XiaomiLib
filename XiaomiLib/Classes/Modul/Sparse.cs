
namespace Mrivai.Pelitabangsa.Modul
{
    /// <summary>
    /// Manage Sparse
    /// </summary>
    public class Sparse
    {
        /// <summary>
        /// sparse magic header
        /// </summary>
        public const uint SPARSE_HEADER_MAGIC = 3978755898;
        /// <summary>
        /// sparse header major version
        /// </summary>
        public const uint SPARSE_HEADER_MAJOR_VER = 1;
        /// <summary>
        /// sparse chunk raw
        /// </summary>
        public const ushort SPARSE_CHUNK_TYPE_RAW = 51905;
        /// <summary>
        /// sparse chunk fill
        /// </summary>
        public const ushort SPARSE_CHUNK_TYPE_FILL = 51906;
        /// <summary>
        /// sparse chunk dontcare
        /// </summary>
        public const ushort SPARSE_CHUNK_TYPE_DONTCARE = 51907;
    }
}
