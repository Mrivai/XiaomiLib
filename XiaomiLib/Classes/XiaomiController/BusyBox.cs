/*
 * BusyBox.cs - Developed by Mrivai for XiaomiLib.dll
 */

using System.Collections.Generic;
using System.IO;

namespace Mrivai.Pelitabangsa
{
    /// <summary>
    /// Conatins information about device's busybox
    /// </summary>
    public class BusyBox
    {
        internal const string EXECUTABLE = "busybox";

        private Device device;

        private bool isInstalled;
        private string version;
        private List<string> commands;

        /// <summary>
        /// Gets a value indicating if busybox is installed on the current device
        /// </summary>
        public bool IsInstalled { get { return isInstalled; } }

        /// <summary>
        /// Gets a value indicating the version of busybox installed
        /// </summary>
        public string Version { get { return version; } }

        /// <summary>
        /// Gets a <c>List&lt;string&gt;</c> containing busybox's commands
        /// </summary>
        public List<string> Commands { get { return commands; } }

        internal BusyBox(Device device)
        {
            this.device = device;

            commands = new List<string>();

            Update();
        }

        /// <summary>
        /// Updates the instance of busybox
        /// </summary>
        /// <remarks>Generally called only if busybox may have changed on the device</remarks>
        public void Update()
        {
            commands.Clear();

            if (!device.HasRoot || device.State != DeviceState.ONLINE)
            {
                SetNoBusybox();
                return;
            }

            AdbCommand adbCmd = AdbCmd.FormAdbShellCommand(device, false, EXECUTABLE);
            using (StringReader s = new StringReader(AdbCmd.ExecuteAdbCommand(adbCmd)))
            {
                string check = s.ReadLine();

                if (check.Contains(string.Format("{0}: not found", EXECUTABLE)))
                {
                    SetNoBusybox();
                    return;
                }

                isInstalled = true;

                version = check.Split(' ')[1].Substring(1);

                while (s.Peek() != -1 && s.ReadLine() != "Currently defined functions:") { }

                string[] cmds = s.ReadToEnd().Replace(" ", "").Replace("\r\r\n\t", "").Trim('\t', '\r', '\n').Split(',');

                if (cmds.Length.Equals(0))
                {
                    SetNoBusybox();
                }
                else
                {
                    foreach (string cmd in cmds)
                        commands.Add(cmd);
                }
            }
        }

        private void SetNoBusybox()
        {
            isInstalled = false;
            version = null;
        }
    }
}