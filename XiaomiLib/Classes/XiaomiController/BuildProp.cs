/*
 * BuildProp.cs - Developed by Mrivai for XiaomiLib.dll
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace Mrivai.Pelitabangsa
{
    /// <summary>
    /// Manages all information from connected Android device's build properties
    /// </summary>
    public class BuildProp
    {
        private Device device;

        private Dictionary<string, string> prop;

        internal BuildProp(Device device)
        {
            prop = new Dictionary<string, string>();
            this.device = device;
            Update();
        }

        /// <summary>
        /// Gets a <c>List&lt;string&gt;</c> containing all of the device's build proprty keys
        /// </summary>
        public List<string> Keys
        {
            get
            {
                Update();

                List<string> keys = new List<string>();

                foreach (string key in prop.Keys)
                    keys.Add(key);

                return keys;
            }
        }

        /// <summary>
        /// Gets a <c>List&lt;string&gt;</c> object containing all of the device's build proprty values
        /// </summary>
        public List<string> Values
        {
            get
            {
                Update();

                List<string> values = new List<string>();

                foreach (string val in prop.Values)
                    values.Add(val);

                return values;
            }
        }

        /// <summary>
        /// Gets the value of the specified build property key.
        /// </summary>
        /// <param name="key">Key of build property</param>
        /// <returns>Value if key exists, null if key doesn't exist</returns>
        public string GetProp(string key)
        {
            //Update();

            string tmp;

            prop.TryGetValue(key, out tmp);

            return tmp;
        }

        /// <summary>
        /// Sets a build property value
        /// </summary>
        /// <remarks>If <paramref name="key"/> does not exist or device does not have root, returns false, and does not set any values</remarks>
        /// <param name="key">Build property key to set</param>
        /// <param name="newValue">Value you wish to set <paramref name="key"/> to</param>
        /// <returns>True if new value set, false if not</returns>
        public bool SetProp(string key, string newValue)
        {
            string before;
            if (!prop.TryGetValue(key, out before))
                return false;

            if (!device.HasRoot)
                return false;

            AdbCommand adbCmd = AdbCmd.FormAdbShellCommand(device, true, "setprop", key, newValue);
            AdbCmd.ExecuteAdbCommandNoReturn(adbCmd);

            Update();

            string after;
            if (!prop.TryGetValue(key, out after))
                return false;

            return newValue == after;
        }

        /// <summary>
        /// Returns a formatted string containing all of the build properties
        /// </summary>
        /// <returns>Formatted string containing build.prop</returns>
        public override string ToString()
        {
            Update();

            string outPut = "";

            foreach (KeyValuePair<string, string> s in prop)
                outPut += string.Format("[{0}]: [{1}]" + Environment.NewLine, s.Key, s.Value);
            return outPut;
        }

        private void Update()
        {
            try
            {
                this.prop.Clear();

                if (device.State != DeviceState.ONLINE)
                    return;

                AdbCommand adbCmd = AdbCmd.FormAdbShellCommand(device, false, "getprop");
                string prop = AdbCmd.ExecuteAdbCommand(adbCmd);

                string[] lines = prop.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < lines.Length; i++)
                {
                    string[] entry = lines[i].Split(new string[] { "[", "]: [", "]" }, StringSplitOptions.RemoveEmptyEntries);

                    if (entry.Length == 2)
                        this.prop.Add(entry[0], entry[1]);
                }
            }
            catch (Exception ex)
            {
                Logger.w(ex.Message, "Using: getprop in BuildProp.cs", ex.StackTrace);
            }
        }

        internal void changeProp(string file, string oldProp, string newProp)
        {
            string contents = File.ReadAllText(file);
            contents = contents.Replace(oldProp, newProp);
            File.WriteAllText(file, contents);
        }
    }
}