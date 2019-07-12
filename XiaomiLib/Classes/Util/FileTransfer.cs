

using System;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using Mrivai.Pelitabangsa.Modul;
using Mrivai.Pelitabangsa.Util;

namespace Mrivai.Pelitabangsa
{
    /// <summary>
    /// Manages flashing file transfer
    /// </summary>
    public class FileTransfer
    {
        private FileStream fileStream;
        private string filePath;
        private string portName;
        private long fileLength;

        /// <summary>
        /// Manages connected Android device's info and commands
        /// </summary>
        /// <remarks><paramref name="port"/> should be a device port</remarks>
        /// <remarks><paramref name="filePath"/> should be a path of file path</remarks>
        /// <param name="port">COM20</param>
        /// <param name="filePath">X:\Rom\images\boot.img</param>
        public FileTransfer(string port, string filePath)
        {
            portName = port;
            this.filePath = filePath;
            Flash.UpdateDeviceStatus(new float?(0.0f), "flashing " + filePath, "flashing", false);
            openFile(filePath);
        }

        ~FileTransfer()
        {
            if (fileStream == null)
            return;
            fileStream.Close();
            fileStream.Dispose();
            Logger.w("destruct close file " + filePath, portName, null);
        }

        private bool openFile(string filePath)
        {
            this.filePath = filePath;
            try
            {
            fileLength = new FileInfo(filePath).Length;
            fileStream = File.OpenRead(filePath);
            return true;
            }
            catch (Exception )
            {
            return false;
            }
        }
        /// <summary>
        /// transfer 
        /// </summary>
        public int transfer(SerialPort port, int offset, int size)
        {
            if (!port.IsOpen)
            {
            string.Format("{0} is not open", port.PortName);
            return 0;
            }
            int n = 0;
            byte[] bytesFromFile = GetBytesFromFile(offset, size, out n);
            port.Write(bytesFromFile, 0, size);
            return n;
        }
        /// <summary>
        /// write file
        /// </summary>
        public void WriteFile(Flash portCnn, string strPartitionStartSector, string strPartitionSectorNumber, string pszImageFile, string strFileStartSector, string strFileSectorOffset, string sector_size, string physical_partition_number)
        {
            long num1 = Convert.ToInt64(strPartitionSectorNumber);
            if (num1 == 0L)
            num1 = int.MaxValue;
            long int64_1 = Convert.ToInt64(strFileStartSector);
            long int64_2 = Convert.ToInt64(strFileSectorOffset);
            long int64_3 = Convert.ToInt64(sector_size);
            long int64_4 = Convert.ToInt64(physical_partition_number);
            Logger.w(string.Format("write file {0} length {1} to partition {2}", filePath, getFileSize(), strPartitionStartSector), portCnn.comm.serialPort.PortName, null);
            long num2 = (getFileSize() + int64_3 - 1L) / int64_3;
            if (num2 - int64_1 > num1)
            num2 = int64_1 + num1;
            else
            num1 = num2 - int64_1;
            string str = string.Format(Firehose.FIREHOSE_PROGRAM, int64_3, num1, strPartitionStartSector, int64_4);
            portCnn.comm.SendCommand(str);
            Logger.w(str, portCnn.comm.serialPort.PortName, null);
            long num3 = int64_1;
            while (num3 < num2)
            {
            long num4 = num2 - num3;
            long num5 = num4 < portCnn.comm.m_dwBufferSectors ? num4 : portCnn.comm.m_dwBufferSectors;
            Logger.w(string.Format("WriteFile position {0}, size {1}", (int64_3 * num3).ToString("X"), int64_3 * num5), portCnn.comm.serialPort.PortName, null);
            long offset = int64_2 + int64_3 * num3;
            int size = (int) (int64_3 * num5);
            int n = 0;
            byte[] bytesFromFile = GetBytesFromFile(offset, size, out n);
            portCnn.comm.WritePort(bytesFromFile, 0, bytesFromFile.Length);
            num3 += portCnn.comm.m_dwBufferSectors;
            }
        }
        /// <summary>
        /// write Sparse to device
        /// </summary>
        public void WriteSparseFileToDevice(Flash portCnn, string pszPartitionStartSector, string pszPartitionSectorNumber, string pszImageFile, string pszFileStartSector, string pszSectorSizeInBytes, string pszPhysicalPartitionNumber)
        {
            int int32_1 = Convert.ToInt32(pszPartitionStartSector);
            int int32_2 = Convert.ToInt32(pszPartitionSectorNumber);
            int int32_3 = Convert.ToInt32(pszFileStartSector);
            long offset1 = 0;
            int int32_4 = Convert.ToInt32(pszSectorSizeInBytes);
            Convert.ToInt32(pszPhysicalPartitionNumber);
            SparseImageHeader sparseImageHeader = new SparseImageHeader();
            string str = "";
            if (int32_3 != 0)
            {
            str = "ERROR_BAD_FORMAT";
            Logger.w(str, portCnn.comm.serialPort.PortName, null);
            }
            if (int32_4 == 0)
            {
            str = "ERROR_BAD_FORMAT";
            Logger.w("ERROR_BAD_FORMAT", portCnn.comm.serialPort.PortName, null);
            }
            int size1 = Marshal.SizeOf(sparseImageHeader);
            int n = 0;
            SparseImageHeader stuct1 = (SparseImageHeader) CommandFormat.BytesToStuct(GetBytesFromFile(offset1, size1, out n), typeof (SparseImageHeader));
            long offset2 = offset1 + stuct1.uFileHeaderSize;
            if ((int) stuct1.uMagic != -316211398)
            {
            str = "ERROR_BAD_FORMAT";
            Logger.w("ERROR_BAD_FORMAT", portCnn.comm.serialPort.PortName, null);
            }
            if (stuct1.uMajorVersion != 1)
            {
            str = "ERROR_UNSUPPORTED_TYPE";
            Logger.w("ERROR_UNSUPPORTED_TYPE", portCnn.comm.serialPort.PortName, null);
            }
            if (stuct1.uBlockSize % int32_4 != 0L)
            {
            str = "ERROR_BAD_FORMAT";
            Logger.w("ERROR_BAD_FORMAT", portCnn.comm.serialPort.PortName, null);
            }
            if (int32_2 != 0 && stuct1.uBlockSize * stuct1.uTotalBlocks / int32_4 > int32_2)
            {
            str = "ERROR_FILE_TOO_LARGE";
            Logger.w("ERROR_FILE_TOO_LARGE", portCnn.comm.serialPort.PortName, null);
            }
            if (!string.IsNullOrEmpty(str))
            {
            Flash.UpdateDeviceStatus(new float?(), str, "error", false);
            }
            else
            {
            for (int index = 0; index < stuct1.uTotalChunks; ++index)
            {
                Logger.w(string.Format("total chunks {0}, current chunk {1}", stuct1.uTotalChunks, index), portCnn.comm.serialPort.PortName, null);
                int size2 = Marshal.SizeOf(new SparseChunkHeader());
                SparseChunkHeader stuct2 = (SparseChunkHeader) CommandFormat.BytesToStuct(GetBytesFromFile(offset2, size2, out n), typeof (SparseChunkHeader));
                offset2 += stuct1.uChunkHeaderSize;
                int num1 = (int) stuct1.uBlockSize * (int) stuct2.uChunkSize;
                int num2 = num1 / int32_4;
                switch (stuct2.uChunkType)
                {
                case 51905:
                    if (stuct2.uTotalSize != stuct1.uChunkHeaderSize + num1)
                    {
                    Logger.w("ERROR_BAD_FORMAT", portCnn.comm.serialPort.PortName, null);
                    Flash.UpdateDeviceStatus( new float?(), "ERROR_BAD_FORMAT", "error", false);
                    return;
                    }
                    string strPartitionStartSector = int32_1.ToString();
                    string strPartitionSectorNumber = num2.ToString();
                    string strFileStartSector = (offset2 / int32_4).ToString();
                    string strFileSectorOffset = (offset2 % int32_4).ToString();
                    WriteFile(portCnn, strPartitionStartSector, strPartitionSectorNumber, pszImageFile, strFileStartSector, strFileSectorOffset, pszSectorSizeInBytes, pszPhysicalPartitionNumber);
                    offset2 += int32_4 * num2;
                    int32_1 += num2;
                    break;
                case 51907:
                    if ((int) stuct2.uTotalSize != stuct1.uChunkHeaderSize)
                    Logger.w("ERROR_BAD_FORMAT", portCnn.comm.serialPort.PortName, null);
                    int32_1 += num2;
                    break;
                default:
                    Logger.w("ERROR_UNSUPPORTED_TYPE", portCnn.comm.serialPort.PortName, null);
                    break;
                }
            }
            }
        }

        private byte[] GetBytesFromFile(long offset, int size, out int n)
        {
            long length = fileStream.Length;
            byte[] buffer = new byte[size];
            fileStream.Seek(offset, SeekOrigin.Begin);
            n = fileStream.Read(buffer, 0, size);
            if (offset > length || offset + size > length)
            {
            fileStream.Close();
            fileStream.Dispose();
            Logger.w("close file " + filePath, portName, null);
            }
            Flash.UpdateDeviceStatus(new float?(offset / (float)length), null, "flashing", false);
            return buffer;
        }

        private long getFileSize()
        {
            if (fileLength != 0L)
            return fileLength;
            return new FileInfo(filePath).Length;
        }
    }
}
