using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Mrivai
{
    public class Message
    {
        internal static SocketData DeserializeData(byte[] buffer)
        {
            MemoryStream stream = new MemoryStream(buffer);
            BinaryFormatter bf = new BinaryFormatter();
            stream.Position = 0;
            SocketData data = new SocketData();
            data = (SocketData)bf.Deserialize(stream);
            return data;
        }

        internal static byte[] SerializeData(SocketData ob)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            ms.Position = 0;
            bf.Serialize(ms, ob);
            return ms.ToArray();
        }
    }
}
