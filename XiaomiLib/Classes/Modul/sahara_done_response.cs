

using System.Runtime.InteropServices;

namespace Mrivai.Pelitabangsa.Modul
{
    /// <summary>
    /// manage sahara done response
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct sahara_done_response
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
        /// status
        /// </summary>
        public uint Status;
    }
}
