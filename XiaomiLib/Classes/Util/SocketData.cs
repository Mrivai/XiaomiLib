using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mrivai
{
    [Serializable]
    public class SocketData
    {
        public string ShowName { get; set; }

        public int Command { get; set; }

        public object Data { get; set; }

        public SocketData(string name, int command, object data)
        {
            ShowName = name;
            Command = command;
            Data = data;
        }

        public SocketData() { }
    }

    public enum SocketCommand
    {
        CONNECT,
        MESSAGE,
        SCREENSHOT,
        IMAGE,
        SOUND,
        FILE,
        POINT,
        CLICK,
        DOUBLE,
        MOUSELEFTDOWN,
        MOUSELEFTUP,
        MOUSERIGTHDOWN,
        MOUSERIGTHUP,
        MOUSEMIDDLEDOWN,
        MOUSEMIDDLEUP,
        KEYUP,
        KEYDOWN,
        RESOLUTION
    }
}
