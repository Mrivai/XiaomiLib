

using System.Runtime.InteropServices;

namespace Mrivai.Pelitabangsa.Modul
{
    /// <summary>
    /// Manage sahara response
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct sahara_hello_response
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
        /// mn version
        /// </summary>
        public uint Version_min;
        /// <summary>
        /// status
        /// </summary>
        public uint Status;
        /// <summary>
        /// mode
        /// </summary>
        public uint Mode;
        /// <summary>
        /// reserved
        /// </summary>
        public uint[] Reserved;
    }
}
