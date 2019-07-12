/*
 * FileSystem.cs - Developed by Mrivai for XiaomiLib.dll
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Mrivai.Pelitabangsa
{
    /// <summary>
    /// Contains mount directory information
    /// </summary>
    public class MountInfo
    {
        private string directory;
        private string block;
        private MountType type;

        internal MountInfo(string directory, string block, MountType type)
        {
            this.directory = directory;
            this.block = block;
            this.type = type;
        }

        /// <summary>
        /// Gets a value indicating the mount directory
        /// </summary>
        public string Directory { get { return directory; } }

        /// <summary>
        /// Gets a value indicating the mount block
        /// </summary>
        public string Block { get { return block; } }

        /// <summary>
        /// Gets a value indicating how the mount directory is mounted
        /// </summary>
        /// <remarks>See <see cref="MountType"/> for more details</remarks>
        public MountType MountType { get { return type; } }
    }

    /// <summary>
    /// Contains information about the Android device's file system
    /// </summary>
    public class FileSystem
    {
        private readonly Device device;

        private MountInfo systemMount;

        internal FileSystem(Device device)
        {
            this.device = device;
            UpdateMountPoints();
        }

        private void UpdateMountPoints()
        {
            if (device.State != DeviceState.ONLINE)
            {
                systemMount = new MountInfo(null, null, MountType.NONE);
                return;
            }

            AdbCommand adbCmd = AdbCmd.FormAdbShellCommand(device, false, "mount");
            using (StringReader r = new StringReader(AdbCmd.ExecuteAdbCommand(adbCmd)))
            {
                string line;
                string[] splitLine;
                string dir, mount;
                MountType type;

                while (r.Peek() != -1)
                {
                    line = r.ReadLine();
                    splitLine = line.Split(' ');

                    try
                    {
                        if (line.Contains(" on /system "))
                        {
                            dir = splitLine[2];
                            mount = splitLine[0];
                            type = (MountType)Enum.Parse(typeof(MountType), splitLine[5].Substring(1, 2).ToUpper());
                            systemMount = new MountInfo(dir, mount, type);
                            return;
                        }

                        if (line.Contains(" /system "))
                        {
                            dir = splitLine[1];
                            mount = splitLine[0];
                            type = (MountType)Enum.Parse(typeof(MountType), splitLine[3].Substring(0, 2).ToUpper());
                            systemMount = new MountInfo(dir, mount, type);
                            return;
                        }
                    }
                    catch
                    {
                        dir = "/system";
                        mount = "ERROR";
                        type = MountType.NONE;
                        systemMount = new MountInfo(dir, mount, type);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="MountInfo"/> containing information about the /system mount directory
        /// </summary>
        /// <remarks>See <see cref="MountInfo"/> for more details</remarks>
        public MountInfo SystemMountInfo { get { UpdateMountPoints(); return systemMount; } }

        //void PushFile();
        //void PullFile();

        /// <summary>
        /// Mounts connected Android device's file system as specified
        /// </summary>
        /// <param name="type">The desired <see cref="MountType"/> (RW or RO)</param>
        /// <returns>True if remount is successful, False if remount is unsuccessful</returns>
        /// <example>The following example shows how you can mount the file system as Read-Writable or Read-Only
        /// <code>
        /// // This example demonstrates mounting the Android device's file system as Read-Writable
        /// using System;
        /// using Mrivai.Pelitabangsa;
        /// 
        /// class Program
        /// {
        ///     static void Main(string[] args)
        ///     {
        ///         AndroidController android = AndroidController.Instance;
        ///         Device device;
        ///         
        ///         Console.WriteLine("Waiting For Device...");
        ///         android.WaitForDevice(); //This will wait until a device is connected to the computer
        ///         device = android.ConnectedDevices[0]; //Sets device to the first Device in the collection
        ///
        ///         Console.WriteLine("Connected Device - {0}", device.SerialNumber);
        ///
        ///         Console.WriteLine("Mounting System as RW...");
        ///     	Console.WriteLine("Mount success? - {0}", device.RemountSystem(MountType.RW));
        ///     }
        /// }
        /// 
        ///	// The example displays the following output (if mounting is successful):
        ///	//		Waiting For Device...
        ///	//		Connected Device - {serial # here}
        ///	//		Mounting System as RW...
        ///	//		Mount success? - true
        /// </code>
        /// </example>
        public bool RemountSystem(MountType type)
        {
            if (!device.HasRoot)
                return false;

            AdbCommand adbCmd = AdbCmd.FormAdbShellCommand(device, true, "mount", string.Format("-o remount,{0} -t yaffs2 {1} /system", type.ToString().ToLower(), systemMount.Block));
            AdbCmd.ExecuteAdbCommandNoReturn(adbCmd);

            UpdateMountPoints();

            if (systemMount.MountType == type)
                return true;

            return false;
        }

        private const string IS_FILE = "if [ -f {0} ]; then echo \"1\"; else echo \"0\"; fi";
        private const string IS_DIRECTORY = "if [ -d {0} ]; then echo \"1\"; else echo \"0\"; fi";

        /// <summary>
        /// Gets a <see cref="ListingType"/> indicating is the requested location is a File or Directory
        /// </summary>
        /// <param name="location">Path of requested location on device</param>
        /// <returns>See <see cref="ListingType"/></returns>
        /// <remarks><para>Requires a device containing BusyBox for now, returns ListingType.ERROR if not installed.</para>
        /// <para>Returns ListingType.NONE if file/Directory does not exist</para></remarks>
        public ListingType FileOrDirectory(string location)
        {
            if (!device.BusyBox.IsInstalled)
                return ListingType.ERROR;

            AdbCommand isFile = AdbCmd.FormAdbShellCommand(device, false, string.Format(IS_FILE, location));
            AdbCommand isDir = AdbCmd.FormAdbShellCommand(device, false, string.Format(IS_DIRECTORY, location));

            if (AdbCmd.ExecuteAdbCommand(isFile).Contains("1"))
                return ListingType.FILE;
            else if (AdbCmd.ExecuteAdbCommand(isDir).Contains("1"))
                return ListingType.DIRECTORY;

            return ListingType.NONE;
        }

        /// <summary>
        /// </summary>
        public Dictionary<string, ListingType> GetFilesAndDirectories(string location)
        {
            if (location == null || string.IsNullOrEmpty(location) || Regex.IsMatch(location, @"\s"))
                throw new ArgumentException("rootDir must not be null or empty!");

            Dictionary<string, ListingType> filesAndDirs = new Dictionary<string, ListingType>();
            AdbCommand cmd = null;

            if (device.BusyBox.IsInstalled)
                cmd = AdbCmd.FormAdbShellCommand(device, true, "busybox", "ls", "-a", "-p", "-l", location);
            else
                cmd = AdbCmd.FormAdbShellCommand(device, true, "ls", "-a", "-p", "-l", location);

            using (StringReader reader = new StringReader(AdbCmd.ExecuteAdbCommand(cmd)))
            {
                string line = null;
                while (reader.Peek() != -1)
                {
                    line = reader.ReadLine();
                    if (!string.IsNullOrEmpty(line) && !Regex.IsMatch(line, @"\s"))
                    {
                        filesAndDirs.Add(line, line.EndsWith("/") ? ListingType.DIRECTORY : ListingType.FILE);
                    }
                }
            }


            return filesAndDirs;
        }
    }
}