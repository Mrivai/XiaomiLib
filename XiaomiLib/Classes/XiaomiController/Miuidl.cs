
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Mrivai.Pelitabangsa
{
    /// <summary>
    /// Holds formatted commands to execute through <see cref="MiuiCommand"/>
    /// </summary>
    /// <remarks><para>Can only be created with <c>Miuidl.MiuiCommand()</c></para>
    /// <para>Can only be executed with <c>Miuidl.ExecuteMiuiCommand()</c> or <c>Miuidl.ExecuteMiuiCommandNoReturn()</c></para></remarks>
    public class MiuiCommand
    {
        private string command;
        private int timeout;
        internal string Command { get { return command; } }
        internal int Timeout { get { return timeout; } }
        internal MiuiCommand(string command) { this.command = command; timeout = Mrivai.Command.DEFAULT_TIMEOUT; }

        /// <summary>
        /// Sets the timeout for the MIuiCommand
        /// </summary>
        /// <param name="timeout">The timeout for the command in milliseconds</param>
        public MiuiCommand WithTimeout(int timeout)
        {
            this.timeout = timeout;
            return this;
        }
    }

    /// <summary>
    /// class to controll device from edl mode
    /// </summary>
    public static class Miuidl
    {
        private static object _lock = new object();
        private const string EDL_EXE = "emmcdl.exe";
        private const string COM_EXE = "lsusb.exe";
        private const string Proggramer = "prog_emmc_firehose_8976_ddr.mbn";
        public static List<Gpt> partition = new List<Gpt>();
        private static string[] gptraw;
        public static string rawProgram;

        internal static string Devices()
        {
            return ExecuteCOMCommand(FormLsCommand("qcLsUsb"));
        }

        internal static string Port()
        {
            string port = "";
            //string[] strArray = Regex.Split(new Cmd("").Execute(null, qcLsUsb), "\r\n");
            string[] strArray = Regex.Split(Devices(), "\r\n");
            for (int index = 0; index < strArray.Length; ++index)
            {
                if (!string.IsNullOrEmpty(strArray[index]) && strArray[index].IndexOf("900") > 0)
                {
                    string str = strArray[index].Split('(')[1].Replace(')', ' ');
                    port += str.Trim();
                }
            }
            return port;
        }

        internal static string Program()
        {
            string pro;
            pro = "-f ";
            pro += XiaomiController.Instance.ResourceDirectory + "prog_emmc_firehose_8976_ddr.mbn ";
            return pro;
        }
        /// <summary>
        /// command lsusb.exe
        /// </summary>
        public static MiuiCommand FormLsCommand(string command, params string[] args)
        {
            string fbCmd = (args.Length > 0) ? command + " " : command;

            for (int i = 0; i < args.Length; i++)
                fbCmd += args[i] + " ";
            return new MiuiCommand(fbCmd);
        }
        /// <summary>
        /// command emmcdl.exe
        /// </summary>
        public static MiuiCommand MiuidlFlashCommand(string command, params string[] args)
        {
            string port = "-p ";
            string fbCmd = (args.Length > 0) ? command + " " : command;
            port += Port();
            port += " ";
            string pro = Program();
            for (int i = 0; i < args.Length; i++)
                fbCmd += args[i] + " ";
            string cmd =port + pro + fbCmd;
            
            return new MiuiCommand(cmd);
        }

        /// <summary>
        /// execute lsusb.exe command
        /// </summary>
        public static string ExecuteCOMCommand(MiuiCommand command)
        {
            return Command.RunProcessReturnOutput(@XiaomiController.Instance.ResourceDirectory + COM_EXE, command.Command, command.Timeout);
        }

        /// <summary>
        /// execute emmcdl.exe command return output
        /// </summary>
        public static string ExecuteMiuidlCommand(MiuiCommand command)
        {
            string result = "";
            lock (_lock)
            {
                result = Command.RunProcessReturnOutput(@XiaomiController.Instance.ResourceDirectory + EDL_EXE, command.Command, command.Timeout);
            }
            return result;
        }
        /// <summary>
        /// execute emmcdl.exe command no return
        /// </summary>
        public static void ExecuteMiuidlCommandNoReturn(MiuiCommand command)
        {
            lock (_lock)
            {
                Command.RunProcessNoReturn(@XiaomiController.Instance.ResourceDirectory + EDL_EXE, command.Command, command.Timeout);
            }
        }

        internal static void GetGPT()
        {
            var contents = ExecuteMiuidlCommand(MiuidlFlashCommand("-gpt > "));
            //var contents = File.ReadAllText(raw);
            int lst = contents.IndexOf("1. Partition Name:");
            contents = contents.Remove(0, lst);
            contents = contents.Replace(" Partition Name: ", "name");
            contents = contents.Replace("Start LBA: ", "cok");
            contents = contents.Replace("Size in LBA: ", "end");
            contents = contents.Remove(contents.IndexOf("Status"));
            string[] m = contents.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            gptraw = m;
            var name = "";
            var start = "";
            var end = "";
            foreach (string z in m)
            {
                int f4 = z.IndexOf("name");
                int f5 = z.IndexOf("cok");
                int f6 = z.IndexOf("end");
                end = z.Substring(f6 + 3);
                start = z.Substring(f5 + 3);
                start = start.Substring(0, 7);
                name = z.Substring(f4 + 4, f5 - 7).Replace(" ","");
                partition.Add(new Gpt(name, start, end));
            }
        }
        internal static string MakeRawProgram()
        {
            rawProgram = "<?xml version=\"1.0\" ?>\n";
            rawProgram += "<data>\n";
            foreach (Gpt part in partition)
            {
                int kb = Convert.ToInt32(part.end) / 2;
                var dec = Convert.ToInt32(part.start) * 2;
                var hex = string.Format("{0:x}", dec);
                rawProgram += string.Format("  <program SECTOR_SIZE_IN_BYTES=\"512\" file_sector_offset=\"0\" filename=\"{0}.img\" label=\"{0}\" num_partition_sectors=\"{2}\" physical_partition_number=\"0\" size_in_KB=\"{3}.0\" sparse=\"false\" start_byte_hex=\"0x{4}00L\" start_sector=\"{1}\"/>\n", part.name, part.start, part.end, kb, hex);
            }
            rawProgram += "</data>";
            return rawProgram;
        }
    }

    public class Gpt
    {
        public string name;
        public string start;
        public string end;

        internal Gpt(string nm, string st, string en)
        {
            name = nm;
            start = st;
            end = en;
        }
    }
}
