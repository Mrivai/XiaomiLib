
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.Threading;

namespace Mrivai.Pelitabangsa.Util
{
    /// <summary>
    /// Manage Coomm Command
    /// </summary>
    public class Comm
    {
        public bool ignoreResponse = true;
        public int MAX_SECTOR_STR_LEN = 20;
        public int SECTOR_SIZE_UFS = 4096;
        public int SECTOR_SIZE_EMMC = 512;
        public bool isDump;
        public SerialPort serialPort;
        public Thread thread;
        private volatile bool _keepReading;
        public byte[] recData;
        private long received_count;
        public int m_dwBufferSectors;
        public int intSectorSize;
        /// <summary>
        /// check is port open or not
        /// </summary>
        public bool IsOpen
        {
          get
          {
            int num = 10;
            while (num-- > 0 && !serialPort.IsOpen)
            {
              Logger.w(serialPort.PortName, "wait for port open.", null);
              Thread.Sleep(1000);
            }
            return serialPort.IsOpen;
          }
        }
        /// <summary>
        /// Init Comm port
        /// </summary>
        public Comm()
        {
          serialPort = new SerialPort();
          thread = null;
          _keepReading = false;
        }
        /// <summary>
        /// Start Reading Device comm port
        /// </summary>
        public void StartReading()
        {
          if (_keepReading)
            return;
          _keepReading = true;
          thread = new Thread(new ThreadStart(ReadPort));
          thread.Start();
        }
        /// <summary>
        /// Stop reading device port
        /// </summary>
        public void StopReading()
        {
          if (!_keepReading)
            return;
          _keepReading = false;
          thread.Join();
          thread = null;
        }

        private void ReadPort()
        {
          while (_keepReading)
          {
            if (serialPort.IsOpen)
            {
              int bytesToRead = serialPort.BytesToRead;
              if (bytesToRead > 0)
              {
                byte[] buffer = new byte[bytesToRead];
                try
                {
                  serialPort.Read(buffer, 0, bytesToRead);
                }
                catch (TimeoutException ex)
                {
                  Logger.w(ex.Message, serialPort.PortName, ex.StackTrace);
                }
              }
            }
          }
        }
        /// <summary>
        /// read comm port data
        /// </summary>
        public byte[] ReadPortData()
        {
          byte[] buffer = null;
          if (serialPort.IsOpen)
          {
            int bytesToRead = serialPort.BytesToRead;
            if (bytesToRead > 0)
            {
              buffer = new byte[bytesToRead];
              try
              {
                serialPort.Read(buffer, 0, bytesToRead);
              }
              catch (TimeoutException ex)
              {
                Logger.w(ex.Message, serialPort.PortName, ex.StackTrace);
              }
            }
          }
          return buffer;
        }
        /// <summary>
        /// read comm port data
        /// </summary>
        public byte[] ReadPortData(int offset, int count)
        {
          byte[] buffer = new byte[count];
          try
          {
            serialPort.Read(buffer, offset, count);
          }
          catch (TimeoutException ex)
          {
            Logger.w(ex.Message, serialPort.PortName, ex.StackTrace);
          }
          return buffer;
        }
        /// <summary>
        /// open comm port
        /// </summary>
        public void Open()
        {
          Close();
          serialPort.Open();
          if (serialPort.IsOpen)
            return;
          string str = "open serial port failed!";
          Logger.w(str, serialPort.PortName, null);
          Flash.UpdateDeviceStatus(new float?(), str, "error", true);
        }

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
          int bytesToRead = serialPort.BytesToRead;
          recData = new byte[bytesToRead];
          received_count += bytesToRead;
          serialPort.Read(recData, 0, bytesToRead);
        }
        /// <summary>
        /// close comm port
        /// </summary>
        public void Close()
        {
          StopReading();
          serialPort.Close();
        }
        /// <summary>
        /// write to comm port
        /// </summary>
        public void WritePort(byte[] send, int offSet, int count)
        {
          if (!IsOpen)
            return;
          int num = 100;
          Exception ex1 = new TimeoutException();
          bool flag = false;
          while (num-- > 0 && ex1 != null)
          {
            if (ex1.GetType() == typeof (TimeoutException))
            {
              try
              {
                serialPort.WriteTimeout = 1000;
                serialPort.Write(send, offSet, count);
                flag = true;
                if (isDump)
                Dump(send);
                ex1 = null;
              }
              catch (TimeoutException ex2)
              {
                ex1 = ex2;
                Logger.w(serialPort.PortName, "write time out try agian " + (100 - num), null);
                Thread.Sleep(500);
              }
              catch (Exception ex2)
              {
                 Logger.w(ex2.Message, "write failed:" + serialPort.PortName , ex2.StackTrace);
              }
            }
            else
              break;
          }
          if (flag)
            return;
          Logger.w(ex1.Message , serialPort.PortName, ex1.StackTrace);
        }
        /// <summary>
        /// send comm port command
        /// </summary>
        public bool SendCommand(string command)
        {
          return SendCommand(command, false);
        }
        /// <summary>
        /// send comm port command
        /// </summary>
        public bool SendCommand(string command, bool checkAck)
        {
          byte[] bytes = Encoding.Default.GetBytes(command);
          if (isDump || checkAck)
            Logger.w("send command:" + command, serialPort.PortName, null);
          WritePort(bytes, 0, bytes.Length);
          if (checkAck)
            return GetResponse(checkAck);
          return false;
        }
        /// <summary>
        /// read recieved comm port data
        /// </summary>
        public byte[] getRecData()
        {
          byte[] binary = ReadDataFromPort();
          if (binary == null)
            {
                Logger.w("read from port", serialPort.PortName, null);
                throw new Exception("can not read from port " + serialPort.PortName);
            }
          if (binary.Length > 0 && isDump)
          {
            Logger.w("read from port", serialPort.PortName,  null);
            Dump(binary);
          }
          return binary;
        }

        private byte[] ReadDataFromPort()
        {
          int num = 10;
          recData = null;
          for (recData = ReadPortData(); num-- > 0 && recData == null; recData = ReadPortData())
            Thread.Sleep(500);
          return recData;
        }

        private bool WaitForAck()
        {
          bool flag = false;
          int num = 10;
          while (num-- > 0 && !flag)
          {
            string[] strArray = Dump(ReadDataFromPort());
            flag = strArray.Length == 2 && strArray[1].IndexOf("<response value=\"ACK\" />") >= 0;
            Thread.Sleep(500);
          }
          return flag;
        }
        /// <summary>
        /// get comm port response
        /// </summary>
        public bool GetResponse(bool waiteACK)
        {
          bool flag = false;
          Logger.w("get response from target", serialPort.PortName,null);
          if (!waiteACK)
            return ReadDataFromPort() != null;
          int num = 2;
          if (waiteACK)
            num = 10;
          while (num-- > 0 && !flag)
          {
            List<XmlDocument> responseXml = GetResponseXml(waiteACK);
            int count = responseXml.Count;
            foreach (XmlNode xmlNode in responseXml)
            {
              foreach (XmlNode childNode in xmlNode.SelectSingleNode("data").ChildNodes)
              {
                foreach (XmlAttribute attribute in childNode.Attributes)
                {
                  if (attribute.Name.ToLower() == "maxpayloadsizetotargetinbytes")
                    m_dwBufferSectors = Convert.ToInt32(attribute.Value) / intSectorSize;
                  if (attribute.Value.ToLower() == "ack")
                    flag = true;
                }
              }
            }
            if (waiteACK)
              Thread.Sleep(500);
          }
          return flag;
        }

        private List<XmlDocument> GetResponseXml()
        {
          return GetResponseXml(false);
        }

        private List<XmlDocument> GetResponseXml(bool waiteACK)
        {
          List<XmlDocument> xmlDocumentList = new List<XmlDocument>();
          string[] strArray = Dump(ReadDataFromPort(), waiteACK);
          if (strArray.Length >= 2)
          {
            foreach (string str in ((IEnumerable<string>) Regex.Split(strArray[1], "\\<\\?xml")).ToList())
            {
              if (!string.IsNullOrEmpty(str))
              {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml("<?xml " + str);
                xmlDocumentList.Add(xmlDocument);
              }
            }
          }
          return xmlDocumentList;
        }

        private string GetResponseXmlStr()
        {
          return Dump(ReadDataFromPort())[1];
        }

        private string[] Dump(byte[] binary)
        {
          return Dump(binary, false);
        }

        private string[] Dump(byte[] binary, bool waitACK)
        {
          Logger.w("dump:", serialPort.PortName,  null);
          if (binary == null)
          {
            Logger.w("no Binary dump", serialPort.PortName, null);
            return new string[2]{ "", "" };
          }
          StringBuilder stringBuilder1 = new StringBuilder();
          StringBuilder stringBuilder2 = new StringBuilder();
          StringBuilder stringBuilder3 = new StringBuilder();
          StringBuilder stringBuilder4 = new StringBuilder();
          int num = 0;
          while (num < binary.Length)
          {
            for (int index = 0; index < 16; ++index)
            {
              if (num + index < binary.Length)
                stringBuilder4.Append(Convert.ToChar(binary[num + index]).ToString());
              else
                stringBuilder4.Append(" ");
            }
            stringBuilder2.Append(stringBuilder4);
            stringBuilder3.Length = 0;
            stringBuilder4.Length = 0;
            num += 16;
          }
          if (isDump || waitACK)
            Logger.w(stringBuilder2.ToString() + "\r\n\r\n", serialPort.PortName, null);
          return new string[2]
          {
            stringBuilder1.ToString(),
            stringBuilder2.ToString()
          };
        }
      }
}
