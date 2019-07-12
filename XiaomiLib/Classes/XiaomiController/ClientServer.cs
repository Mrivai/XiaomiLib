using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System;

namespace Mrivai.Pelitabangsa
{
    public class ClientServer
    {
        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
        const int KEYEVENTF_EXTENDEDKEY = 0x1;
        const uint KEYEVENTF_KEYUP = 0x0002;
        const uint KEYEVENTF_KEYDOWN = 0;

        const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;
        const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        const uint MOUSEEVENTF_MOVE = 0x0001;
        const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        const uint MOUSEEVENTF_XDOWN = 0x0080;
        const uint MOUSEEVENTF_XUP = 0x0100;
        const uint MOUSEEVENTF_WHEEL = 0x0800;
        const uint MOUSEEVENTF_HWHEEL = 0x01000;

        private TcpListener listener;
        /// <summary>
        /// list of connected technician
        /// </summary>
        public List<TcpClient> lsClient = new List<TcpClient>();
        /// <summary>
        /// message buffersize
        /// </summary>
        public int bufferSize = 1024 * 5000;
        private Bitmap printscreen;
        private IPAddress ServerIp;
        private int ServerPort;
        private NetworkStream stream;
        public string Me;
        public TcpClient klien;
        private IPEndPoint EndPoint;
        
        public bool IsConnected { get { return checkConnection(); } }

        public int PORT { get { return ServerPort; } set { ServerPort = value; } }

        /// <summary>
        /// command to connect to server, execute by technician (as client)
        /// </summary>
        public void StartConnect()
        {
            Me = "Teknisi";
            klien = new TcpClient();
            EndPoint = new IPEndPoint(ServerIp, ServerPort);
            klien.Connect(EndPoint);
        }
        /// <summary>
        /// command to start server, execute by client (as server)
        /// </summary>
        public void StartServer()
        {
            Me = "Klien";
            listener = new TcpListener(ServerIp, ServerPort);
            listener.Start();
        }
        /// <summary>
        /// command to stop server, execute by client (as server)
        /// </summary>
        public void DisposeServer()
        {
            if (listener != null) listener.Stop();
            Disconnect();
        }
        /// <summary>
        /// command to accept technician connection, execute by client (as server)
        /// </summary>
        public void AcceptTechnicians()
        {
            klien = listener.AcceptTcpClient();
        }
        /// <summary>
        /// command to disconnect coonection, execute by client (as server)
        /// </summary>
        public void Disconnect()
        {
            if (stream != null) { stream.Close(); }
            else if (klien != null) { klien.Close(); }
            else if (lsClient != null) { lsClient.Clear(); }
        }
        /// <summary>
        /// command to get stream data, execute by client (as server), technician (as client)
        /// </summary>
        public NetworkStream GetStream()
        {
            stream = klien.GetStream();
            return stream;
        }
        /// <summary>
        /// command to get message data, execute by client (as server), technician (as client)
        /// </summary>
        public SocketData GetData(byte[] buffer)
        {
            SocketData data = Message.DeserializeData(buffer);
            return data;
        }
        /// <summary>
        /// command to process message data, self execute by library (as mediator)
        /// </summary>
        public void ProcessData(SocketData data)
        {
            switch (data.Command)
            {
                case (int)SocketCommand.SCREENSHOT:
                    GetScreen();
                    break;
                case (int)SocketCommand.RESOLUTION:
                    resolutionScreen();
                    break;
                case (int)SocketCommand.POINT:
                    MoveCursor(data);
                    break;
                case (int)SocketCommand.CLICK:
                    MC();
                    break;
                case (int)SocketCommand.DOUBLE:
                    MouseDBClick();
                    break;
                case (int)SocketCommand.MOUSELEFTUP:
                    MouseUpDown(MOUSEEVENTF_LEFTUP);
                    break;
                case (int)SocketCommand.MOUSELEFTDOWN:
                    MouseUpDown(MOUSEEVENTF_LEFTDOWN);
                    break;
                case (int)SocketCommand.MOUSEMIDDLEUP:
                    MouseUpDown(MOUSEEVENTF_MIDDLEUP);
                    break;
                case (int)SocketCommand.MOUSEMIDDLEDOWN:
                    MouseUpDown(MOUSEEVENTF_MIDDLEDOWN);
                    break;
                case (int)SocketCommand.MOUSERIGTHDOWN:
                    MouseUpDown(MOUSEEVENTF_RIGHTDOWN);
                    break;
                case (int)SocketCommand.MOUSERIGTHUP:
                    MouseUpDown(MOUSEEVENTF_RIGHTUP);
                    break;
                case (int)SocketCommand.KEYUP:
                    EventKeyUp(data);
                    break;
                case (int)SocketCommand.KEYDOWN:
                    EventKeyDown(data);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// command to send message, execute by client (as server), technician (as client)
        /// </summary>
        public void SendMessage(SocketData message)
        {
            SendData(GetStream(), message);
        }
        private void SendData(NetworkStream stream, SocketData ob)
        {
            try
            {
                byte[] sendBuffer = Message.SerializeData(ob);
                stream.Write(sendBuffer, 0, sendBuffer.Length);
            }
            catch
            {
            }
        }

        private void resolutionScreen()
        {
            var resolutionWidth = Screen.PrimaryScreen.Bounds.Width.ToString();
            var resolutionHeight = Screen.PrimaryScreen.Bounds.Height.ToString();
            var res = resolutionWidth + ":" + resolutionHeight;
            SendData(stream, new SocketData(Me, (int)SocketCommand.RESOLUTION, res));
        }

        private void GetScreen()
        {
            printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(printscreen as Image);
            g.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
            SendData(stream, new SocketData(Me, (int)SocketCommand.IMAGE, printscreen));
            g.Dispose();
            printscreen.Dispose();
        }

        private void MC()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            Thread.Sleep(10);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        private void MouseDBClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            Thread.Sleep(10);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            Thread.Sleep(10);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            Thread.Sleep(10);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        private void MouseUpDown(uint action)
        {
            mouse_event(action, 0, 0, 0, 0);
        }

        private void MoveCursor(SocketData data)
        {
            //var point = (string)data.Data;
            //string[] arr;
            //int curPosX, curPosY;
            //arr = point.Split(':');
            //curPosX = int.Parse(arr[0]);
            //curPosY = int.Parse(arr[1]);
            //Cursor.Position = new Point(curPosX, curPosY);
            Cursor.Position = (Point)data.Data;
        }

        private void EventKeyUp(SocketData data)
        {
            var key = (string)data.Data;
            keybd_event(byte.Parse(key), 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }

        private void EventKeyDown(SocketData data)
        {
            var key = (string)data.Data;
            keybd_event(byte.Parse(key), 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYDOWN, 0);
        }

        private bool checkConnection()
        {
            if (!klien.Connected)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}