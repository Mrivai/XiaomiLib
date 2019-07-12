
namespace Mrivai.Pelitabangsa.Modul
{
    /// <summary>
    /// Manage Command packet
    /// </summary>
    public class CommandPacket
      {
        private int _command;
        private int _Lengthgth;
        private int _Versionnumber;
        private int _Versioncompatible;
        private int _commandpacketLengthgth;
        private int _Mode;
        /// <summary>
        /// command
        /// </summary>
        public int Command
        {
          get
          {
            return _command;
          }
          set
          {
            _command = value;
          }
        }
        /// <summary>
        /// command length
        /// </summary>
        public int Length
        {
          get
          {
            return _Lengthgth;
          }
          set
          {
             _Lengthgth = value;
          }
        }
        /// <summary>
        /// command version number
        /// </summary>
        public int VersionNumber
        {
          get
          {
            return _Versionnumber;
          }
          set
          {
            _Versionnumber = value;
          }
        }
        /// <summary>
        /// command version compatible
        /// </summary>
        public int VersionCompatible
        {
          get
          {
            return this._Versioncompatible;
          }
          set
          {
            this._Versioncompatible = value;
          }
        }
        /// <summary>
        /// command package length
        /// </summary>
        public int CommandPacketLengthgth
        {
          get
          {
            return _commandpacketLengthgth;
          }
          set
          {
            _commandpacketLengthgth = value;
          }
        }
        /// <summary>
        /// command mode
        /// </summary>
        public int Mode
        {
          get
          {
            return _Mode;
          }
          set
          {
            _Mode = value;
          }
        }
        /// <summary>
        /// command package
        /// </summary>
        public CommandPacket()
        {
        }
        /// <summary>
        /// command package
        /// </summary>
        public CommandPacket(byte[] arr)
        {
          if (arr.Length < 48)
            return;
          for (int index = 0; index < arr.Length; ++index)
          {
            if (index % 4 == 0)
            {
              switch (index)
              {
                case 12:
                  _Versioncompatible = arr[index];
                  continue;
                case 16:
                  _commandpacketLengthgth = arr[index];
                  continue;
                case 20:
                  _Mode = arr[index];
                  continue;
                case 0:
                  _command = arr[index];
                  continue;
                case 4:
                  _Lengthgth = arr[index];
                  continue;
                case 8:
                  _Versionnumber = arr[index];
                  continue;
                default:
                  continue;
              }
            }
          }
        }
      }
}
