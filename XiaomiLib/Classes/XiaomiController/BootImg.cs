

using System.IO;

namespace Mrivai.Pelitabangsa
{
    /// <summary>
    /// Holds formatted commands to execute through <see cref="BootCommand"/>
    /// </summary>
    /// <remarks><para>Can only be created with <c>Bootimg.BootCommand()</c></para>
    /// <para>Can only be executed with <c>Bootimg.ExecuteBootCommand()</c> or <c>Bootimg.ExecuteBootCommandNoReturn()</c></para></remarks>
    public class BootCommand
    {
        private string command;
        private int timeout;
        internal string Command { get { return command; } }
        internal int Timeout { get { return timeout; } }
        internal BootCommand(string command) { this.command = command; timeout = Mrivai.Command.DEFAULT_TIMEOUT; }

        /// <summary>
        /// Sets the timeout for the BootCommand
        /// </summary>
        /// <param name="timeout">The timeout for the command in milliseconds</param>
        public BootCommand WithTimeout(int timeout) { this.timeout = timeout; return this; }
    }
    /// <summary>
    /// class for manage boot.img unpack repack
    /// </summary>
    public static class BootImg
    {
        private static object _lock = new object();
        private const string BOOT_EXE = "bootimg.exe";

        internal static BootCommand FormBootCommand(string command, params string[] args)
        {
            string fbCmd = (args.Length > 0) ? command + " " : command;

            for (int i = 0; i < args.Length; i++)
                fbCmd += args[i] + " ";
            return new BootCommand(fbCmd);
        }
        /// <summary>
        /// Executes an <see cref="BootCommand"/> running BootImg Command
        /// </summary>
        /// <param name="command">Instance of <see cref="BootCommand"/></param>
        /// <returns>Exit code of the process</returns>
        
        internal static string ExecuteBootimgCommand(BootCommand command)
        {
            string result = "";
            lock (_lock)
            {
                result = Command.RunProcessReturnOutput(XiaomiController.Instance.ResourceDirectory + BOOT_EXE, command.Command, command.Timeout);
            }
            return result;
        }

        /// <summary>
        /// unpacking boot image.
        /// </summary>
        /// <remarks><paramref name="dest"/> should be a raw path of a boot.img (C:\unpack\boot.img)</remarks>
        /// <param name="dest">path of boot.img</param>
        internal static string UnpackBootImg(string dest)
        {
            string command = "--unpack-bootimg " + @dest;
            return ExecuteBootimgCommand(FormBootCommand(command));
        }

        /// <summary>
        /// unpacking boot image.
        /// </summary>
        /// <remarks><paramref name="file"/> should be a raw path of a boot.img (C:\unpack\boot.img)</remarks>
        /// <param name="file">path of boot.img</param>
        internal static string PackBootImg(string file)
        {
            string command = "--repack-bootimg " + @file;
            return ExecuteBootimgCommand(FormBootCommand(command));
        }
        /// <summary>
        /// disableVerity
        /// </summary>
        /// <remarks><paramref name="file"/> should be a raw path of a fstab.qcom (C:\unpack\fstab.qcom)</remarks>
        /// <param name="file">path of boot.img</param>
        internal static void disableVerity(string file)
        {
            string contents = File.ReadAllText(file);
            contents = contents.Replace(",verity", "");
            contents = contents.Replace("encryptable", "");
            File.WriteAllText(file, contents);
        }
    }
}