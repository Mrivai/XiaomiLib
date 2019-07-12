

using Mrivai.Pelitabangsa.Modul;
using Mrivai.Pelitabangsa.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace Mrivai.Pelitabangsa
{
    /// <summary>
    /// Flashing connected devices
    /// </summary>
    public class Flash
    {
        private static float progress;
        private Device device;
        private static string result;
        private static string status;
        private static bool done = false;
        /// <summary>
        /// initialize comm
        /// </summary>
        public Comm comm = new Comm();
        private int BUFFER_SECTORS = 256;
        private int programmerType = 1;
        private string storageType = "ufs";
        private string flashtype;
        private string swPath;
        private DateTime _startTime;
        private bool update;
        private string name;
        private bool eror = false;
        /// <summary>
        /// init devices to flash
        /// </summary>
        public Flash(Device dev)
        {
            device = dev;
            name = device.SerialNumber;
        }
        /// <summary>
        /// set start time first
        /// </summary>
        public DateTime StartTime
        {
            get { return _startTime; }
            set { _startTime = value; }
        }
        /// <summary>
        /// set flash firmware folder
        /// </summary>
        public string SwPath { get { return swPath; } set { swPath = value; } }
        /// <summary>
        /// set flash type example: flash all, flash all except storage.
        /// </summary>
        public string Flashtype { get { return flashtype; } set { flashtype = value; } }
        /// <summary>
        /// to check is flashing is done or not
        /// </summary>
        public bool IsDone { get { return done; } }
        /// <summary>
        /// to check is flashing isupdate
        /// </summary>
        public bool IsUpdate { get { return update; } set { update = value; } }
        /// <summary>
        /// get set flashing progress
        /// </summary>
        public float Progress { get { return progress; } set { progress = value; } }
        /// <summary>
        /// get flashing result
        /// </summary>
        public string Result { get { return result; } }
        /// <summary>
        /// get set flashing status
        /// </summary>
        public string Status { get { return status; } set { status = value; } }
        /// <summary>
        /// get devices name
        /// </summary>
        public string Name { get { return name; } }
        /// <summary>
        /// get devices flashing error status
        /// </summary>
        public bool Error { get { return eror; } }

        /// <summary>
        /// update device flashing status
        /// </summary>
        internal static void UpdateDeviceStatus(float? _progress, string _status, string _result, bool isDone)
        {
            if (_progress.HasValue)
                progress = _progress.Value;
            if (!string.IsNullOrEmpty(_status))
                status = _status;
            if (!string.IsNullOrEmpty(_result))
                result = _result;
            done = isDone;
        }
        /// <summary>
        /// Star flashing
        /// </summary>
        public void StartFlash()
        {
            try
            {
                if (device.State == DeviceState.FASTBOOT)
                {
                    Logger.w("start", "fastboot flasher:", null);
                    FastbootFlasher();
                }
                else if (device.State == DeviceState.EDL)
                {
                    Logger.w("start", "Edl flasher:", null);
                    EDLFlasher();
                }
            }
            catch (Exception ex)
            {
                UpdateDeviceStatus(new float?(), ex.Message, "Error", true);
                eror = true;
            }
            
        }

        private void FastbootFlasher()
        {
            if (!Directory.Exists(swPath))
            {
                Logger.w("sw path is not valid", "Fastboot Flasher:", null);
                throw new Exception("sw path is not valid");
            }
            try
            {
                string[] strArray = FileSearcher.SearchFiles(swPath, flashtype);
                if (strArray.Length == 0)
                {
                    Logger.w("can not found file " + Flashtype, "Fastboot Flasher:", null);
                    throw new Exception("can not found file " + Flashtype);
                }
                string str = strArray[0];
                string command = string.Format("pushd \"{0}\"&&prompt $$&&set PATH=\"{1}\";%PATH%&&\"{2}\" -s {3}&&popd", swPath, XiaomiController.Instance.ResourceDirectory, str, name);
                Logger.w("Firmware path: " + swPath, "Fastboot Flasher:", null);
                Logger.w("env android path: " + XiaomiController.Instance.ResourceDirectory, "Fastboot Flasher:", null);
                Logger.w("Flash Type :" + str, "Fastboot Flasher:", null);
                Logger.w("Send Command: " + command, "Fastboot Flasher:", null);
                Command.ProcessReturnOutput(command);
            }
            catch (Exception ex)
            {
                UpdateDeviceStatus(new float?(), ex.Message, "Error", true);
                eror = true;
            }
        }

        private void EDLFlasher()
        {
            if (string.IsNullOrEmpty(name)) { return; }
            if (!Directory.Exists(swPath))
            {
                Logger.w("sw path is not valid", "Edl flasher:", null);
                throw new Exception("sw path is not valid");
            }

            foreach (DirectoryInfo directory in new DirectoryInfo(swPath).GetDirectories())
            {
                if (directory.Name.ToLower() == "images")
                {
                    swPath = directory.FullName;
                    break;
                }
            }
            try
            {
                UpdateDeviceStatus( new float?(0.0f), "start flash", "flashing", false);
                //register port example (COM10)
                registerPort(name);
                SaharaDownloadProgrammer();

                Thread.Sleep(1000);
                PropareFirehose();
                ConfigureDDR(comm.intSectorSize, BUFFER_SECTORS, storageType, 0);
                if (Provision(swPath))
                {
                    Thread.Sleep(2000);
                    SaharaDownloadProgrammer();
                    Thread.Sleep(1000);
                    PropareFirehose();
                    ConfigureDDR(comm.intSectorSize, BUFFER_SECTORS, storageType, 0);
                }
                if (storageType == Storage.ufs)
                    SetBootPartition();
                if (comm.ignoreResponse)
                    comm.StartReading();
                FirehoseDownloadImg(swPath);
                comm.StopReading();
                UpdateDeviceStatus( new float?(1f), "flash done", "success", true);
                Logger.w("Flashing Succes: ", name, null);
            }
            catch (Exception ex)
            {
                eror = true;
                UpdateDeviceStatus(new float?(), ex.Message, "error", true);
                Logger.w("Edl Flasher Error Status: "+ ex.Message, name, ex.StackTrace);
            }
            finally
            {
                comm.serialPort.Close();
                comm.serialPort.Dispose();
            }
        }

        private void SaharaDownloadProgrammer()
        {
            if (comm.IsOpen)
            {
                string msg1 = string.Format("[{0}]:{1}", comm.serialPort.PortName, "start flash.");
                UpdateDeviceStatus(new float?(0.0f), "read hello packet", "flashing", false);
                Logger.w(msg1, comm.serialPort.PortName, null);
                comm.getRecData();
                if (comm.recData.Length == 0)
                    comm.recData = new byte[48];
                sahara_packet saharaPacket = new sahara_packet();
                sahara_hello_packet stuct1 = (sahara_hello_packet)CommandFormat.BytesToStuct(comm.recData, typeof(sahara_hello_packet));
                stuct1.Reserved = new uint[6];
                sahara_hello_response saharaHelloResponse = new sahara_hello_response();
                saharaHelloResponse.Reserved = new uint[6];
                sahara_switch_Mode_packet switchModePacket = new sahara_switch_Mode_packet();
                int num = 20;
                while (num-- > 0 && (int)stuct1.Command != 1)
                {
                    string str1 = "cannot receive hello packet,try agian";
                    Logger.w(str1, comm.serialPort.PortName, null);
                    UpdateDeviceStatus(new float?(0.0f), str1, "flashing", false);
                    if (comm.recData.Length == 0)
                        comm.recData = new byte[48];
                    stuct1 = (sahara_hello_packet)CommandFormat.BytesToStuct(comm.recData, typeof(sahara_hello_packet));
                    Thread.Sleep(500);
                    if (num == 10)
                    {
                        string str2 = "cannot receive hello packet,try to reset";
                        Logger.w(str2, comm.serialPort.PortName, null);
                        UpdateDeviceStatus( new float?(0.0f), str2, "flashing", false);
                        saharaHelloResponse.Command = 2U;
                        saharaHelloResponse.Length = (uint)Marshal.SizeOf(saharaHelloResponse);
                        saharaHelloResponse.Version = 2U;
                        saharaHelloResponse.Version_min = 1U;
                        saharaHelloResponse.Mode = 3U;
                        comm.WritePort(CommandFormat.StructToBytes(saharaHelloResponse), 0, Marshal.SizeOf(saharaHelloResponse));
                        string str3 = "Switch mode back";
                        Logger.w(str3, comm.serialPort.PortName, null);
                        UpdateDeviceStatus(new float?(0.0f), str3, "flashing", false);
                        switchModePacket.Command = 12U;
                        switchModePacket.Length = (uint)Marshal.SizeOf(switchModePacket);
                        switchModePacket.Mode = 0U;
                        comm.WritePort(CommandFormat.StructToBytes(switchModePacket), 0, Marshal.SizeOf(switchModePacket));
                        if (comm.recData.Length == 0)
                            comm.recData = new byte[48];
                        stuct1 = (sahara_hello_packet)CommandFormat.BytesToStuct(comm.recData, typeof(sahara_hello_packet));
                    }
                }
                if ((int)stuct1.Command == 1)
                {
                    string str = "received hello packet";
                    UpdateDeviceStatus(new float?(0.0f), str, "flashing", false);
                    Logger.w(str, comm.serialPort.PortName, null);
                }
                else
                {
                    string str = "cannot receive hello packet";
                    UpdateDeviceStatus(new float?(0.0f), str, "flashing", false);
                    Logger.w(str, comm.serialPort.PortName, null);
                }
                saharaHelloResponse.Command = 2U;
                saharaHelloResponse.Length = 48U;
                saharaHelloResponse.Version = 2U;
                saharaHelloResponse.Version_min = 1U;
                byte[] bytes1 = CommandFormat.StructToBytes(saharaHelloResponse);
                comm.WritePort(bytes1, 0, bytes1.Length);
                string[] strArray = FileSearcher.SearchFiles(swPath, SoftwareImage.ProgrammerPattern);
                if (strArray.Length != 1)
                    throw new Exception("can not found programmer file.");
                string str4 = strArray[0];
                FileInfo fileInfo = new FileInfo(str4);
                if (fileInfo.Name.ToLower().IndexOf("firehose") >= 0)
                    programmerType = Programmer.firehose;
                if (fileInfo.Name.ToLower().IndexOf("ufs") >= 0)
                    storageType = Storage.ufs;
                else if (fileInfo.Name.ToLower().IndexOf("emmc") >= 0)
                    storageType = Storage.emmc;
                comm.intSectorSize = storageType == Storage.ufs ? comm.SECTOR_SIZE_UFS : comm.SECTOR_SIZE_EMMC;
                Logger.w("donwload programmer " + str4, comm.serialPort.PortName, null);
                UpdateDeviceStatus(new float?(0.0f), "download programmer " + str4, "flashing", false);
                FileTransfer fileTransfer = new FileTransfer(comm.serialPort.PortName, str4);
                bool flag;
                do
                {
                    flag = false;
                    comm.getRecData();
                    byte[] recData = comm.recData;
                    saharaPacket = (sahara_packet)CommandFormat.BytesToStuct(comm.recData, typeof(sahara_packet));
                    switch (saharaPacket.Command)
                    {
                        case 3:
                            sahara_readdata_packet stuct3 = (sahara_readdata_packet)CommandFormat.BytesToStuct(comm.recData, typeof(sahara_readdata_packet));
                            string msg2 = string.Format("sahara read data:imgID {0}, offset {1},length {2}", stuct3.Image_id, stuct3.Offset, stuct3.SLength);
                            fileTransfer.transfer(comm.serialPort, (int)stuct3.Offset, (int)stuct3.SLength);
                            Logger.w(msg2, comm.serialPort.PortName, null);
                            break;
                        case 4:
                            sahara_end_transfer_packet stuct4 = (sahara_end_transfer_packet)CommandFormat.BytesToStuct(comm.recData, typeof(sahara_end_transfer_packet));
                            string msg3 = string.Format("sahara read end  imgID:{0} status:{1}", stuct4.Image_id, stuct4.Status);
                            if ((int)stuct4.Status != 0)
                                Logger.w(string.Format("sahara read end error with status:{0}", stuct4.Status), comm.serialPort.PortName, null);
                            flag = true;
                            Logger.w(msg3, comm.serialPort.PortName, null);
                            break;
                        case 18:
                            sahara_64b_readdata_packet stuct5 = (sahara_64b_readdata_packet)CommandFormat.BytesToStuct(comm.recData, typeof(sahara_64b_readdata_packet));
                            string msg4 = string.Format("sahara read 64b data:imgID {0},offset {1},length {2}", stuct5.Image_id, stuct5.Offset, stuct5.SLength);
                            fileTransfer.transfer(comm.serialPort, (int)stuct5.Offset, (int)stuct5.SLength);
                            Logger.w(msg4, comm.serialPort.PortName, null);
                            break;
                        default:
                            Logger.w(string.Format("invalid command:{0}", saharaPacket.Command), comm.serialPort.PortName, null);
                            break;
                    }
                }
                while (!flag);
                saharaPacket.Command = 5U;
                saharaPacket.Length = 8U;
                byte[] bytes2 = CommandFormat.StructToBytes(saharaPacket, 8);
                for (int index = 8; index < bytes2.Length; ++index)
                    bytes2[index] = 0;
                comm.WritePort(bytes2, 0, bytes2.Length);
                comm.getRecData();
                if (comm.recData.Length == 0)
                    comm.recData = new byte[48];
                if ((int)((sahara_done_response)CommandFormat.BytesToStuct(comm.recData, typeof(sahara_done_response))).Command == 6)
                {
                    string str1 = string.Format("file {0} transferred successfully", str4);
                    Thread.Sleep(2000);
                    Logger.w(str1, comm.serialPort.PortName, null);
                    UpdateDeviceStatus(new float?(1f), str1, "flashing", false);
                }
                else
                {
                    string str1 = "programmer transfer error";
                    Logger.w(str1, comm.serialPort.PortName, null);
                    UpdateDeviceStatus(new float?(), str1, "flashing", false);
                }
            }
            else
                Logger.w(string.Format("port {0} is not open.", comm.serialPort.PortName), comm.serialPort.PortName, null);
        }

        private void registerPort(string port)
        {
            if (comm.serialPort != null)
                comm.serialPort.Close();
            comm.serialPort.PortName = port;
            comm.serialPort.BaudRate = 9600;
            comm.serialPort.Parity = Parity.None;
            comm.serialPort.ReadTimeout = 100;
            comm.serialPort.WriteTimeout = 100;
            comm.Open();
        }
        private void PropareFirehose()
        {
            ping();
        }

        private void ping()
        {
            Logger.w("send nop command", comm.serialPort.PortName, null);
            UpdateDeviceStatus(new float?(0.0f), "ping target via firehose", "flashing", false);
            if (!comm.SendCommand(Firehose.Nop, true))
            {
                Logger.w("cant send nop command ping target failed", comm.serialPort.PortName, null);
                throw new Exception("ping target failed");
            }
            UpdateDeviceStatus(new float?(1f), "ping target via firehose", "flashing", false);
        }
        private void ConfigureDDR(int intSectorSize, int buffer_sectors, string ddrType, int m_iSkipStorageInit)
        {
            Logger.w("send configure command", comm.serialPort.PortName, null);
            UpdateDeviceStatus(new float?(0.0f), "send configure command", "flashing", false);
            if (!comm.SendCommand(string.Format(Firehose.Configure, (intSectorSize * buffer_sectors), ddrType, m_iSkipStorageInit), true))
            {
                Logger.w("send configure command failed", comm.serialPort.PortName, null);
                throw new Exception("send configure command failed");
            }
            UpdateDeviceStatus(new float?(1f), "send command command", "flashing", false);
        }
        private bool Provision(string swpath)
        {
            string[] strArray = FileSearcher.SearchFiles(swpath, SoftwareImage.ProvisionPattern);
            if (strArray.Length == 0)
                return false;
            string str = strArray[0];
            Logger.w(string.Format("start provision:{0}", str), comm.serialPort.PortName, null);
            XmlDocument xmlDocument = new XmlDocument();
            XmlReader reader = XmlReader.Create(str, new XmlReaderSettings()
            {
                IgnoreComments = true
            });
            xmlDocument.Load(reader);
            XmlNodeList childNodes = xmlDocument.SelectSingleNode("data").ChildNodes;
            int num = 0;
            foreach (XmlElement xmlElement in childNodes)
            {
                if (!(xmlElement.Name.ToLower() != "ufs"))
                {
                    StringBuilder stringBuilder = new StringBuilder("<ufs ");
                    foreach (XmlAttribute attribute in xmlElement.Attributes)
                    {
                        if (!(attribute.Name.ToLower() == "desc"))
                            stringBuilder.Append(string.Format("{0}=\"{1}\" ", attribute.Name, attribute.Value));
                    }
                    stringBuilder.Append("/>");
                    string command = string.Format("<?xml version=\"1.0\" ?>\n<data>\n{0}\n</data>", stringBuilder.ToString());
                    if (!comm.SendCommand(command, true))
                        Logger.w("Provision failed :" + command, comm.serialPort.PortName, null);
                    UpdateDeviceStatus(new float?(num / (float)childNodes.Count), str, "provisioning", false);
                    ++num;
                }
            }
            Logger.w("Provision done.", comm.serialPort.PortName, null);
            UpdateDeviceStatus( new float?(1f), "provisiong done", "provisioning", false);
            return Reboot(comm.serialPort.PortName);
        }
        private void SetBootPartition()
        {
            UpdateDeviceStatus(new float?(0.0f), "send nop command", "flashing", false);
            if (!comm.SendCommand(Firehose.SetBootPartition, true))
                throw new Exception("set boot partition failed");
            UpdateDeviceStatus(new float?(1f), "set boot partition", "flashing", false);
        }
        private void FirehoseDownloadImg(string swPath)
        {
            // find rawprogram0.xml
            string[] strArray1 = FileSearcher.SearchFiles(swPath, SoftwareImage.RawProgramPattern);
            string[] strArray2 = FileSearcher.SearchFiles(swPath, SoftwareImage.PatchPattern);
            for (int index = 0; index < strArray1.Length; ++index)
            {
                if (WriteFilesToDevice(comm.serialPort.PortName, swPath, strArray1[index]))
                {
                    ApplyPatchesToDevice(comm.serialPort.PortName, strArray2[index]);
                }
            }
        }
        private bool WriteFilesToDevice(string portName, string swPath, string rawFilePath)
        {
            bool flag1 = true;
            Logger.w(string.Format("open program file {0}", rawFilePath), comm.serialPort.PortName, null);
            UpdateDeviceStatus(new float(), rawFilePath, "Flashing", false);
            XmlDocument xmlDocument = new XmlDocument();
            XmlReader reader = XmlReader.Create(rawFilePath, new XmlReaderSettings()
            {
                IgnoreComments = true
            });
            xmlDocument.Load(reader);
            XmlNodeList childNodes = xmlDocument.SelectSingleNode("data").ChildNodes;
            bool flag2 = false;
            string str1 = "";
            string str2 = "0";
            string str3 = "0";
            string str4 = "0";
            string str5 = "0";
            string str6 = "512";
            string str7 = "";
            foreach (XmlElement xmlElement in childNodes)
            {
                if (!(xmlElement.Name.ToLower() != "program"))
                {
                    foreach (XmlAttribute attribute in xmlElement.Attributes)
                    {
                        switch (attribute.Name.ToLower())
                        {
                            case "file_sector_offset":
                                str2 = attribute.Value;
                                continue;
                            case "filename":
                                str1 = attribute.Value;
                                continue;
                            case "num_partition_sectors":
                                str4 = attribute.Value;
                                continue;
                            case "start_sector":
                                str3 = attribute.Value;
                                continue;
                            case "sparse":
                                flag2 = attribute.Value == "true";
                                continue;
                            case "sector_size_in_bytes":
                                str6 = attribute.Value;
                                continue;
                            case "physical_partition_number":
                                str5 = attribute.Value;
                                continue;
                            case "label":
                                str7 = attribute.Value;
                                continue;
                            default:
                                continue;
                        }
                    }
                    //bypass if filename is emty
                    if (!string.IsNullOrEmpty(str1))
                    {
                        //str1 = swpath + \ + filename
                        str1 = swPath + "\\" + str1;
                        if (str1.IndexOf("gpt_main1") >= 0 || str1.IndexOf("gpt_main2") >= 0)
                            Thread.Sleep(3000);
                        //check if sparse = true
                        if (flag2)
                        {
                            Logger.w(string.Format("Write sparse file {0} to partition[{1}] sector {2}", str1, str7, str3), comm.serialPort.PortName, null);
                            //write image to device via file transfer using comm and write image with parameter {start_sector, num_partition_sectors, filename, file_sector_offset, sector_size_in_bytes, physical_partition_number}
                            new FileTransfer(comm.serialPort.PortName, str1).WriteSparseFileToDevice(this, str3, str4, str1, str2, str6, str5);
                        }
                        else
                        {
                            Logger.w(string.Format("Write file {0} to partition[{1}] sector {2}", str1, str7, str3), comm.serialPort.PortName, null);
                            //write image to device via file transfer using comm and write image with parameter {start_sector, num_partition_sectors, filename, file_sector_offset, 0, sector_size_in_bytes, physical_partition_number }
                            //just add 0 value to the parameter
                            new FileTransfer(comm.serialPort.PortName, str1).WriteFile(this, str3, str4, str1, str2, "0", str6, str5);
                        }
                        Logger.w(string.Format("Image {0} transferred successfully", str1), comm.serialPort.PortName, null);
                    }
                }
            }
            reader.Close();
            return flag1;
        }
        private bool Reboot(string portName)
        {
            Logger.w("reboot target", comm.serialPort.PortName, null);

            UpdateDeviceStatus(new float?(0.0f), "reboot target", "flashing", false);
            if (!comm.SendCommand(Firehose.Reset_To_Edl, true))
                throw new Exception("reboot target failed");
            comm.serialPort.Close();
            comm.serialPort.Dispose();
            List<string> list = ((IEnumerable<string>)getDevice()).ToList();
            int num = 10;
            while (num-- > 0 && list.IndexOf(portName) < 0)
            {
                Thread.Sleep(500);
                list = ((IEnumerable<string>)getDevice()).ToList<string>();
                string str = string.Format("waiting for {0} reboot", portName);
                Logger.w(str, comm.serialPort.PortName, null);
                UpdateDeviceStatus(new float?(), str, "reboot", false);
            }
            bool flag1;
            if (list.IndexOf(portName) >= 0)
            {
                bool flag2 = true;
                string str = string.Format("{0} reboot successfully", portName);
                Logger.w(str, comm.serialPort.PortName,null);
                Thread.Sleep(2000);
                comm.serialPort.Open();
                flag1 = flag2 && comm.IsOpen;
                UpdateDeviceStatus(new float?(1f), str, "reboot", false);
            }
            else
            {
                string str = string.Format("{0} reboot failed", portName);
                Logger.w(str, comm.serialPort.PortName, null);
                UpdateDeviceStatus(new float?(0.0f), str, "reboot", false);
                flag1 = false;
            }
            return flag1;
        }
        private bool ApplyPatchesToDevice(string portName, string patchFilePath)
        {
            bool flag = true;
            UpdateDeviceStatus(new float?(0.0f), patchFilePath, "flashing", false);
            XmlDocument xmlDocument = new XmlDocument();
            XmlReader reader = XmlReader.Create(patchFilePath, new XmlReaderSettings()
            {
                IgnoreComments = true
            });
            xmlDocument.Load(reader);
            XmlNodeList childNodes = xmlDocument.SelectSingleNode("patches").ChildNodes;
            string str = "";
            string pszPatchSize = "0";
            string pszPatchValue = "0";
            string pszDiskOffsetSector = "0";
            string pszSectorOffsetByte = "0";
            string pszPhysicalPartitionNumber = "0";
            string pszSectorSizeInBytes = "512";
            foreach (XmlElement xmlElement in childNodes)
            {
                if (!(xmlElement.Name.ToLower() != "patch"))
                {
                    foreach (XmlAttribute attribute in xmlElement.Attributes)
                    {
                        switch (attribute.Name.ToLower())
                        {
                            case "byte_offset":
                                pszSectorOffsetByte = attribute.Value;
                                continue;
                            case "filename":
                                str = attribute.Value;
                                continue;
                            case "size_in_bytes":
                                pszPatchSize = attribute.Value;
                                continue;
                            case "start_sector":
                                pszDiskOffsetSector = attribute.Value;
                                continue;
                            case "value":
                                pszPatchValue = attribute.Value;
                                continue;
                            case "sector_size_in_bytes":
                                pszSectorSizeInBytes = attribute.Value;
                                continue;
                            case "physical_partition_number":
                                pszPhysicalPartitionNumber = attribute.Value;
                                continue;
                            default:
                                continue;
                        }
                    }
                    if (str.ToLower() == "disk")
                        ApplyPatch(pszDiskOffsetSector, pszSectorOffsetByte, pszPatchValue, pszPatchSize, pszSectorSizeInBytes, pszPhysicalPartitionNumber);
                }
            }
            UpdateDeviceStatus( new float?(1f), patchFilePath, "flashing", false);
            return flag;
        }

        private void ApplyPatch(string pszDiskOffsetSector, string pszSectorOffsetByte, string pszPatchValue, string pszPatchSize, string pszSectorSizeInBytes, string pszPhysicalPartitionNumber)
        {
            Logger.w(string.Format("ApplyPatch sector {0}, offset {1}, value {2}, size {3}", pszDiskOffsetSector, pszSectorOffsetByte, pszPatchValue, pszPatchSize), comm.serialPort.PortName, null);
            comm.SendCommand(string.Format(Firehose.FIREHOSE_PATCH, pszSectorSizeInBytes, pszSectorOffsetByte, pszPhysicalPartitionNumber, pszPatchSize, pszDiskOffsetSector, pszPatchValue));
        }

        private string[] getDevice()
        {
            return getDevicesQc();
        }
        private string[] getDevicesQc()
        {
            List<string> stringList = new List<string>();
            string qcLsUsb = XiaomiController.Instance.ResourceDirectory + "lsusb.exe";
            if (!File.Exists(qcLsUsb))
                throw new Exception("no lsusb.");
            string[] strArray = Regex.Split(Command.Execute(qcLsUsb), "\r\n");
            for (int index = 0; index < strArray.Length; ++index)
            {
                if (!string.IsNullOrEmpty(strArray[index]) && strArray[index].IndexOf("9008") > 0)
                {
                    string str = strArray[index].Split('(')[1].Replace(')', ' ');
                    stringList.Add(str.Trim());
                }
            }
            return stringList.ToArray();
        }
    }
}
