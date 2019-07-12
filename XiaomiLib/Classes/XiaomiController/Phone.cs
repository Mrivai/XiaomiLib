/*
 * Phone.cs - Developed by Mrivai for XiaomiLib.dll
 */

namespace Mrivai.Pelitabangsa
{
    /// <summary>
    /// Controls radio options on an Android device
    /// </summary>
    public class Phones
    {
        private Device device;

        internal Phones(Device device)
        {
            this.device = device;
        }

        

        /// <summary>
        /// Calls a phone number on the Android device
        /// </summary>
        /// <param name="phoneNumber">Phone number to call</param>
        public void CallPhoneNumber(string phoneNumber)
        {
            if (device.State != DeviceState.ONLINE)
                return;

            AdbCommand adbCmd = AdbCmd.FormAdbShellCommand(device, false, "service", "call", "phone", "2", "s16", phoneNumber);
            AdbCmd.ExecuteAdbCommandNoReturn(adbCmd);
            adbCmd = AdbCmd.FormAdbShellCommand(device, false, "input", "keyevent", (int)KeyEventCode.BACK);
            AdbCmd.ExecuteAdbCommandNoReturn(adbCmd);
        }

        /// <summary>
        /// Dials (does not call) a phone number on the Android device
        /// </summary>
        /// <param name="phoneNumber">Phone number to dial</param>
        public void DialPhoneNumber(string phoneNumber)
        {
            if (device.State != DeviceState.ONLINE)
                return;

            AdbCommand adbCmd = AdbCmd.FormAdbShellCommand(device, false, "service", "call", "phone", "1", "s16", phoneNumber);
            AdbCmd.ExecuteAdbCommandNoReturn(adbCmd);
        }
    }
}
