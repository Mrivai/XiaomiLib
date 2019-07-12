using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Mrivai.Pelitabangsa
{
    public interface AddImageStream
    {
        void AddStream(byte[] buff);
    }

    public delegate void MinicapEventHandler();
    
    /// <summary>
    /// manage MinicapStream class
    /// </summary>
    public class MinicapStream : AddImageStream
    {
        public event MinicapEventHandler Update;
        private ConcurrentQueue<byte[]> bytequeue = new ConcurrentQueue<byte[]>();
        private Thread t;
        private bool active = false;
        private string IP = "127.0.0.1";
        private int PORT = 1313;

        private Banner banner = new Banner();
        private Socket socket;
        byte[] chunk = new byte[4096];

        private static MinicapStream instance;
        /// <summary>
        /// Initializes a new instance of MinicapStream class
        /// </summary>
        public static MinicapStream Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MinicapStream();
                }
                return instance;
            }
        }
        /// <summary>
        /// Initializes a new socket instance of MinicapStream class
        /// </summary>
        public MinicapStream()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //socket.Connect(new IPEndPoint(IPAddress.Parse(IP), PORT));
        }
        /// <summary>
        /// Star minicap connect to server(phone)
        /// </summary>
        public void StarMinicap()
        {
            if (!active)
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(new IPEndPoint(IPAddress.Parse(IP), PORT));
                active = true;
            }
        }
        /// <summary>
        /// Star minicap disconnect from server(phone)
        /// </summary>
        public void StopMinicap()
        {
            if (active)
            {
                socket.Dispose();
            }
            instance = null;
        }
        private byte[] subByteArray(byte[] arr, int start, int end)
        {
            int len = end - start;
            byte[] newbyte = new byte[len];
            Buffer.BlockCopy(arr, start, newbyte, 0, len);
            return newbyte;
        }
        /// <summary>
        /// image QueQue
        /// </summary>
        public ConcurrentQueue<byte[]> ImageByteQueue
        {
            get
            {
                return bytequeue;
            }
        }
        /// <summary>
        /// image banner
        /// </summary>
        public Banner Banner
        {
            get
            {
                return banner;
            }
        }
        /// <summary>
        /// Stream image
        /// </summary>
        public void ReadImageStream()
        {
            int reallen;
            int readBannerBytes = 0;
            int bannerLength = 2;
            int readFrameBytes = 0;
            int frameBodyLength = 0;
            byte[] frameBody = new byte[0];
            while ((reallen = socket.Receive(chunk)) != 0)
            {
                for (int cursor = 0, len = reallen; cursor < len;)
                {
                    if (readBannerBytes < bannerLength)
                    {
                        switch (readBannerBytes)
                        {
                            case 0:
                                banner.Version = chunk[cursor];
                                break;
                            case 1:
                                banner.Length = bannerLength = chunk[cursor];
                                break;
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                                banner.Pid += (chunk[cursor] << ((cursor - 2) * 8));
                                break;
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                                banner.RealWidth += (chunk[cursor] << ((cursor - 6) * 8));
                                break;
                            case 10:
                            case 11:
                            case 12:
                            case 13:
                                banner.RealHeight += (chunk[cursor] << ((cursor - 10) * 8));
                                break;
                            case 14:
                            case 15:
                            case 16:
                            case 17:
                                banner.VirtualWidth += (chunk[cursor] << ((cursor - 14) * 8));
                                break;
                            case 18:
                            case 19:
                            case 20:
                            case 21:
                                banner.VirtualHeight += (chunk[cursor] << ((cursor - 2) * 8));
                                break;
                            case 22:
                                banner.Orientation += chunk[cursor] * 90;
                                break;
                            case 23:
                                banner.Quirks = chunk[cursor];
                                break;
                        }
                        cursor += 1;
                        readBannerBytes += 1;
                    }

                    else if (readFrameBytes < 4)
                    {
                        frameBodyLength += (chunk[cursor] << (readFrameBytes * 8));
                        cursor += 1;
                        readFrameBytes += 1;
                    }
                    else
                    {

                        if (len - cursor >= frameBodyLength)
                        {
                            frameBody = frameBody.Concat(subByteArray(chunk, cursor, cursor + frameBodyLength)).ToArray();
                            AddStream(frameBody);
                            cursor += frameBodyLength;
                            frameBodyLength = readFrameBytes = 0;
                            frameBody = new byte[0];
                        }
                        else
                        {
                            frameBody = frameBody.Concat(subByteArray(chunk, cursor, len)).ToArray();
                            frameBodyLength -= len - cursor;
                            readFrameBytes += len - cursor;
                            cursor = len;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// add tream
        /// </summary>
        public void AddStream(byte[] frameBody)
        {
            bytequeue.Enqueue(frameBody);
            if (Update != null)
            {
                Update();
            }
        }
    }
}
