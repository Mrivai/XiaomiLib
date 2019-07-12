/*
 * Exceptions.cs - Developed by Mrivai for XiaomiLib.dll
 */

using System;

namespace Mrivai.Pelitabangsa
{
    /// <summary>
    /// Thrown when a root shell command is executed on a device without root
    /// </summary>
    /// <remarks>Only created and called internally</remarks>
    public class DeviceHasNoRootException : Exception
    {
        internal DeviceHasNoRootException() { }
    }
}