/*
 * XiaomiController.cs - Handles communication between computer and Android devices
 * Developed by Mrivai for XiaomiLib.dll - 04/12/12
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Mrivai.Pelitabangsa
{
    /// <summary>
    /// Controls communication to and from connected Android devices.  Use only one instance for the entire project.
    /// </summary>
    /// <remarks>
    /// <para><see cref="XiaomiController"/> is the core class in XiaomiLib. You must always call the <c>Dispose()</c> method when finished before program exits.</para>
    /// <para><see cref="XiaomiController"/> specifically controls the Android Debug Bridge Server, and a developer should NEVER try to start/kill the server using an <see cref="AdbCommand"/></para>
    /// </remarks>
   
    public sealed class XiaomiController
    {
        private const string ANDROID_CONTROLLER_TMP_FOLDER = "XiaomiLib\\";
        private static readonly Dictionary<string, string> RESOURCES = new Dictionary<string, string>
        {
            {"adb.exe","041d1c825660387732c1069c2f0faa6b"},
            {"AdbWinApi.dll", "47a6ee3f186b2c2f5057028906bac0c6"},
            {"AdbWinUsbApi.dll", "5f23f2f936bdfac90bb0a4970ad365cf"},
            {"bootimg.exe", "3620583fb334bffc6a65f86001cb210a"},
            {"fastboot.exe", "284f62373f3b9cc94bd10f7f374dfdcb"},
            {"emmcdl.exe", "da735ba5a4689b3cfa8eaa51d7decb9b"},
            {"edl.exe", "83846850fe14720c22ad3cfbf09b7e5c"},
            {"lsusb.exe", "74a89b028606628d3ec6416ff5fe4c0c"},
            {"Key.zip", "58a8d0a0b3cb3b1a24a95b52a0420dda"},
            {"ngrok.exe", "84ed6012ec62b0bddcd18058a8ff7ddd"},
            {"prog_emmc_firehose_8976_ddr.mbn", "c21079e422ca08e29ef9822827666769"}

        };

        private static XiaomiController instance;

        private string resourceDirectory;
        private List<string> connectedDevices;
        private bool IsConnected = false;
        private bool Extract_Resources = false;

        /// <summary>
        /// Gets the current XiaomiController Instance.
        /// </summary>
        public static XiaomiController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new XiaomiController();
                    instance.CreateResourceDirectories();
                    instance.ExtractResources();
                    AdbCmd.StartServer();
                }
                return instance;
            }
        }

        /// <summary>
        /// Gets a <c>List&lt;string&gt;</c> object containing the serial numbers of all connected Android devices
        /// </summary>
        public List<string> ConnectedDevices
        {
            get
            {
                UpdateDeviceList();
                return connectedDevices;
            }
        }

        internal string ResourceDirectory
        {
            get { return resourceDirectory; }
        }

        private XiaomiController()
        {
            connectedDevices = new List<string>();
            ResourceFolderManager.Register(ANDROID_CONTROLLER_TMP_FOLDER);
            resourceDirectory = ResourceFolderManager.GetRegisteredFolderPath(ANDROID_CONTROLLER_TMP_FOLDER);
        }

        private void CreateResourceDirectories()
        {
            try
            {
                if (!AdbCmd.ExecuteAdbCommand(new AdbCommand("version")).Contains(AdbCmd.ADB_VERSION))
                {
                    AdbCmd.KillServer();
                    Thread.Sleep(1000);
                    Extract_Resources = true;
                }
            }
            catch (Exception)
            {
                Extract_Resources = true;
            }
        }

        private void ExtractResources()
        {
            if (Extract_Resources)
            {
                string[] res = new string[RESOURCES.Count];
                RESOURCES.Keys.CopyTo(res, 0);
                Extract.Resources(this, resourceDirectory, "Resources.XiaomiController", res);
            }
        }

        /// <summary>
        /// Releases all resources used by <see cref="XiaomiController"/>        
        /// </summary>
        /// <remarks>Needs to be called when application has finished using <see cref="XiaomiController"/></remarks>

        public void Dispose()
        {
            if (AdbCmd.ServerRunning)
            {
                AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbCommand("forward--remove - all"));
                AdbCmd.KillServer();
                Thread.Sleep(1000);
            }
            AdbCmd.KillServer();
            ResourceFolderManager.Unregister(ANDROID_CONTROLLER_TMP_FOLDER);
            instance = null;
        }

        /// <summary>
        /// Gets the first <see cref="Device"/> in the internal collection of devices controlled by <see cref="XiaomiController"/>
        /// </summary>
        /// <returns><see cref="Device"/> containing info about the device with the first serial number in the internal collection</returns>
        public Device GetConnectedDevice()
        {
            if (HasConnectedDevices)
                return new Device(connectedDevices[0]);
            return null;
        }

        /// <summary>
        /// Gets a <see cref="Device"/> containing data about a specified Android device.
        /// </summary>
        /// <remarks><paramref name="deviceSerial"/> must be a serial number of a connected device, or the method returns null</remarks>
        /// <param name="deviceSerial">Serial number of connected device</param>
        /// <returns><see cref="Device"/> containing info about the device with the serial number <paramref name="deviceSerial"/></returns>
        public Device GetConnectedDevice(string deviceSerial)
        {
            UpdateDeviceList();
            if (connectedDevices.Contains(deviceSerial))
                return new Device(deviceSerial);
            return null;
        }

        /// <summary>
        /// Gets a value indicating if there are any Android devices currently connected
        /// </summary>
        public bool HasConnectedDevices
        {
            get { UpdateDeviceList(); return IsConnected; }
        }

        /// <summary>
        /// Determines if the Android device with the serial number provided is currently connected
        /// </summary>
        /// <example>The following example shows how to use <c>IsDeviceConnected(string deviceSerial)</c> in one of your programs
        /// <code>
        /// //This example demonstrates how to use IsDeviceConnected(string deviceSerial) in your project
        /// //This example assumes there is an instance of XiaomiController running named android.
        /// 
        /// string serialNumber = "HTC123456789";
        /// 
        /// bool currentlyConnected = android.IsDeviceConnected(serialNumber);
        /// </code>
        /// </example>
        /// <param name="deviceSerial">Serial number of Android device</param>
        /// <returns>A value indicating if the Android device with the serial number <paramref name="deviceSerial"/> is connected</returns>
        public bool IsDeviceConnected(string deviceSerial)
        {
            UpdateDeviceList();

            foreach (string s in connectedDevices)
                if (s.ToLower() == deviceSerial.ToLower())
                    return true;

            return false;
        }

        /// <summary>
        /// Determines if the Android device tied to <paramref name="device"/> is currently connected
        /// </summary>
        /// <param name="device">Instance of <see cref="Device"/></param>
        /// <returns>A value indicating if the Android device indicated in <paramref name="device"/> is connected</returns>
        public bool IsDeviceConnected(Device device)
        {
            UpdateDeviceList();

            foreach (string d in connectedDevices)
                if (d == device.SerialNumber)
                    return true;

            return false;
        }

        /// <summary>
        /// Updates Internal Device List
        /// </summary>
        /// <remarks>Call this before checking for Devices, or setting a new Device, for most updated results</remarks>
        public void UpdateDeviceList()
        {
            connectedDevices.Clear();
            if(SN(AdbCmd.Devices().Replace("List of devices attached", "")).Length > 0 || SN(Fastboot.Devices()).Length > 0)
            {
                IsConnected = true;
            }
            else if(Miuidl.Port().Length > 0)
            {
                connectedDevices.Add(Miuidl.Port());
                IsConnected = true;
            }
            else
            {
                IsConnected = false;
            }
            
        }

        private string SN(string sr)
        {
            string line= "";
            if(sr.Length >= 0)
            {
                using (StringReader s = new StringReader(sr))
                {
                    while (s.Peek() != -1)
                    {
                        line = s.ReadLine();
                        if (line.StartsWith("List") || line.StartsWith("\r\n") || line.Trim() == "")
                            continue;
                        if (line.IndexOf('\t') != -1)
                        {
                            line = line.Substring(0, line.IndexOf('\t'));
                            connectedDevices.Add(line);
                        }
                    }
                }
            }
            
            return line;
        }

        private bool _CancelRequest;
        /// <summary>
        /// Set to true to cancel a WaitForDevice() method call
        /// </summary>
        public bool CancelWait
        {
            get { return _CancelRequest; }
            set { _CancelRequest = value; }
        }

        /// <summary>
        /// Pauses thread until 1 or more Android devices are connected
        /// </summary>
        /// <remarks>Do Not Use in Windows Forms applications, as this method pauses the current thread.  Works fine in Console Applications</remarks>
        public void WaitForDevice()
        {
            /* Entering an endless loop will exhaust CPU. 
             * Since this method must be called in a child thread in Windows Presentation Foundation (WPF) or Windows Form Apps,
             * sleeping thread for 250 miliSecond (1/4 of a second)
             * will be more friendly to the CPU. Nonetheless checking 4 times for a connected device in each second is more than enough,
             * and will not result in late response from the app if a device gets connected. 
             */
            while (!HasConnectedDevices && !CancelWait)
            {
                Thread.Sleep(250);
            }

            CancelWait = false;
        }
        /// <summary>
        /// collect app action, error code or something else
        /// </summary>
        public void log(string title, string message, string stacktrace)
        {
            Logger.w(message, title, stacktrace);
        }
    }
}
