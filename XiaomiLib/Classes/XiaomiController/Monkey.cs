using System.Collections.Generic;

namespace Mrivai.Pelitabangsa
{
    /// <summary>
    /// monkey runner
    /// </summary>
    public class Monkey
    {
        InputEvent _inputEvent;

        /// <summary>
        /// monkey input event
        /// </summary>
        public Monkey( InputEvent inputEvent)
        {
            _inputEvent = inputEvent;
        }

        /// <summary>
        /// monkey mouse down (x, y) position
        /// </summary>
        public void Down(uint x, uint y)
        {
            List<uint[]> aas = new List<uint[]>();

            aas.Add(new uint[] { KeyCodes.EV_KEY, KeyCodes.BTN_TOUCH, KeyCodes.DOWN });
            aas.Add(new uint[] { KeyCodes.EV_ABS, KeyCodes.ABS_MT_PRESSURE, 1 });
            aas.Add(new uint[] { KeyCodes.EV_ABS, KeyCodes.ABS_MT_POSITION_X, x });
            aas.Add(new uint[] { KeyCodes.EV_ABS, KeyCodes.ABS_MT_POSITION_Y, y });
            aas.Add(new uint[] { KeyCodes.EV_SYN, KeyCodes.SYN_MT_REPORT, 0 });
            aas.Add(new uint[] { KeyCodes.EV_SYN, KeyCodes.SYN_REPORT, 0 });
            executeCommand(aas);
        }
        /// <summary>
        /// monkey set position (x, y)
        /// </summary>
        public void SetXY(uint x, uint y)
        {
            List<uint[]> aas = new List<uint[]>();
            aas.Add(new uint[] { KeyCodes.EV_ABS, KeyCodes.ABS_MT_POSITION_X, x });
            aas.Add(new uint[] { KeyCodes.EV_ABS, KeyCodes.ABS_MT_POSITION_Y, y });
            aas.Add(new uint[] { KeyCodes.EV_SYN, KeyCodes.SYN_MT_REPORT, 0 });
            aas.Add(new uint[] { KeyCodes.EV_SYN, KeyCodes.SYN_REPORT, 0 });
            executeCommand(aas);
        }
        /// <summary>
        /// monkey mouse up
        /// </summary>
        public void Up()
        {
            List<uint[]> aas = new List<uint[]>();
            aas.Add(new uint[] { KeyCodes.EV_KEY, KeyCodes.BTN_TOUCH, KeyCodes.UP });
            aas.Add(new uint[] { KeyCodes.EV_SYN, KeyCodes.SYN_MT_REPORT, 0 });
            aas.Add(new uint[] { KeyCodes.EV_SYN, KeyCodes.SYN_REPORT, 0 });
            executeCommand(aas);
        }
        private void executeCommand(List<uint[]> cmds)
        {
            AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbCommand(getCommand(cmds)));
            //AdbClient.Instance.ExecuteRemoteCommand(getCommand(cmds), device, receiver);
        }
        private string getCommand(List<uint[]> aas)
        {
            string command = "";
            foreach (uint[] uints in aas)
            {
                command += "sendevent " + _inputEvent.getShellPath() + " " + uints[0] + " " + uints[1] + " " + uints[2] + ";";
            }
            return command;
        }
    }
}
