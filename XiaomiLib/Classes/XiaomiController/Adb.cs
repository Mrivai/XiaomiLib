/*
 * Adb.cs - Developed by Mrivai for XiaomiLib.dll
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Mrivai.Pelitabangsa
{
    /// <summary>
    /// Holds formatted commands to execute through <see cref="AdbCmd"/>
    /// </summary>
    /// <remarks><para>Can only be created with <c>Adb.FormAdbCommand()</c> or <c>Adb.FormAdbShellCommand()</c></para>
    /// <para>Can only be executed with <c>Adb.ExecuteAdbCommand()</c> or <c>Adb.ExecuteAdbCommandNoReturn()</c></para></remarks>
    public class AdbCommand
    {

        private string command;
        private int timeout;
        internal string Command { get { return command; } }
        internal int Timeout { get { return timeout; } }
        internal AdbCommand(string command)
        {
            this.command = command;
            timeout = Mrivai.Command.DEFAULT_TIMEOUT;
        }

        /// <summary>
        /// Sets the timeout for the AdbCommand
        /// </summary>
        /// <param name="timeout">The timeout for the command in milliseconds</param>
        public AdbCommand WithTimeout(int timeout)
        {
            this.timeout = timeout; return this;
        }
    }

    /// <summary>
    /// Controls all commands sent to the currently running Android Debug Bridge Server
    /// </summary>
    public static class AdbCmd
    {
        private static object _lock = new object();
        internal const string ADB = "adb";
        internal const string SS = "ss.png";
        internal const string ADB_EXE = "adb.exe";
        internal const string ADB_VERSION = "1.0.39";
        internal static string DEFAULT_ENCODING = "ISO-8859-1";
        private const int WAIT_TIME = 5;


        /// <summary>
        /// Forms an <see cref="AdbCommand"/> that is passed to <c>Adb.ExecuteAdbCommand()</c>
        /// </summary>
        /// <remarks><para>This should only be used for non device-specific Adb commands, such as <c>adb devices</c> or <c>adb version</c>.</para>
        /// <para>Never try to start/kill the running Adb Server, as the <see cref="XiaomiController"/> type handles it internally.</para></remarks>
        /// <param name="command">The command to run on the Adb Server</param>
        /// <param name="args">Any arguments that need to be sent to <paramref name="command"/></param>
        /// <returns><see cref="AdbCommand"/> that contains formatted command information</returns>
        /// <example>This example demonstrates how to create an <see cref="AdbCommand"/>
        /// <code>
        /// //This example shows how to create an AdbCommand object to execute on the running server.
        /// //The command we will create is "adb devices".  
        /// //Notice how in the formation, you don't supply the prefix "adb", because the method takes care of it for you.
        /// 
        /// AdbCommand adbCmd = Adb.FormAdbCommand("devices");
        /// 
        /// </code>
        /// </example>
        public static AdbCommand FormAdbCommand(string command, params object[] args)
        {
            string adbCommand = (args.Length > 0) ? command + " " : command;

            for (int i = 0; i < args.Length; i++)
                adbCommand += args[i] + " ";

            return new AdbCommand(adbCommand);
        }

        /// <summary>
        /// Forms an <see cref="AdbCommand"/> that is passed to <c>Adb.ExecuteAdbCommand()</c>
        /// </summary>
        /// <remarks>This should only be used for device-specific Adb commands, such as <c>adb push</c> or <c>adb pull</c>.</remarks>
        /// <param name="device">Specific <see cref="Device"/> to run the command on</param>
        /// <param name="command">The command to run on the Adb Server</param>
        /// <param name="args">Any arguments that need to be sent to <paramref name="command"/></param>
        /// <returns><see cref="AdbCommand"/> that contains formatted command information</returns>
        /// <example>This example demonstrates how to create an <see cref="AdbCommand"/>
        /// <code>//This example shows how to create an AdbCommand object to execute on the running server.
        /// //The command we will create is "adb pull /system/app C:\".  
        /// //Notice how in the formation, you don't supply the prefix "adb", because the method takes care of it for you.
        /// //This example also assumes you have a Device instance named device.
        /// 
        /// AdbCommand adbCmd = Adb.FormAdbCommand(device, "pull", "/system/app", @"C:\");
        /// 
        /// </code>
        /// </example>
        public static AdbCommand FormAdbShellCommand(Device device, string command, params object[] args)
        {
            return FormAdbCommand("-s " + device.SerialNumber + " " + command, args);
        }

        /// <summary>
        /// Forms an <see cref="AdbCommand"/> that is passed to <c>Adb.ExecuteAdbCommand()</c>
        /// </summary>
        /// <param name="device">Specific <see cref="Device"/> to run the command on</param>
        /// <param name="rootShell">Specifies if you need <paramref name="executable"/> to run in a root shell</param>
        /// <param name="executable">Executable file on <paramref name="device"/> to execute</param>
        /// <param name="args">Any arguments that need to be sent to <paramref name="executable"/></param>
        /// <returns><see cref="AdbCommand"/> that contains formatted command information</returns>
        /// <remarks>This should only be used for Adb Shell commands, such as <c>adb shell getprop</c> or <c>adb shell dumpsys</c>.</remarks>
        /// <exception cref="DeviceHasNoRootException"> if <paramref name="device"/> does not have root</exception>
        /// <example>This example demonstrates how to create an <see cref="AdbCommand"/>
        /// <code>
        /// //This example shows how to create an AdbCommand object to execute on the running server.
        /// //The command we will create is "adb shell input keyevent KeyEventCode.HOME".
        /// //Notice how in the formation, you don't supply the prefix "adb", because the method takes care of it for you.
        /// //This example also assumes you have a Device instance named device.
        /// 
        /// AdbCommand adbCmd = Adb.FormAdbCommand(device, true, "input", "keyevent", (int)KeyEventCode.HOME);
        /// 
        /// </code>
        /// </example>
        public static AdbCommand FormAdbShellCommand(Device device, bool rootShell, string executable, params object[] args)
        {
            if (rootShell && !device.HasRoot)
                throw new DeviceHasNoRootException();

            string shellCommand = string.Format("-s {0} shell \"", device.SerialNumber);

            if (rootShell)
                shellCommand += "su -c \"";

            shellCommand += executable;

            for (int i = 0; i < args.Length; i++)
                shellCommand += " " + args[i];

            if (rootShell)
                shellCommand += "\"";

            shellCommand += "\"";

            return new AdbCommand(shellCommand);
        }

        /// <summary>
        /// Opens Adb Shell and allows input to be typed directly to the shell.  Experimental!
        /// </summary>
        /// <remarks>Added specifically for RegawMOD CDMA Hero Rooter.  Always remember to pass "exit" as the last command or it will not return!</remarks>
        /// <param name="device">Specific <see cref="Device"/> to run the command on</param>
        /// <param name="inputLines">Lines of commands to send to shell</param>
        [Obsolete("Method is deprecated, please use ExecuteAdbShellCommandInputString(Device, int, string...) instead.")]
        public static void ExecuteAdbShellCommandInputString(Device device, params string[] inputLines)
        {
            lock (_lock)
            {
                Command.RunProcessWriteInput(XiaomiController.Instance.ResourceDirectory + ADB_EXE, "shell", inputLines);
            }
        }

        /// <summary>
        /// Opens Adb Shell and allows input to be typed directly to the shell.  Experimental!
        /// </summary>
        /// <remarks>Added specifically for CDMA Hero Rooter.  Always remember to pass "exit" as the last command or it will not return!</remarks>
        /// <param name="device">Specific <see cref="Device"/> to run the command on</param>
        /// <param name="timeout">The timeout in milliseonds</param>
        /// <param name="inputLines">Lines of commands to send to shell</param>
        public static void ExecuteAdbShellCommandInputString(Device device, int timeout, params string[] inputLines)
        {
            lock (_lock)
            {
                Command.RunProcessWriteInput(XiaomiController.Instance.ResourceDirectory + ADB_EXE, "shell", timeout, inputLines);
            }
        }

        /// <summary>
        /// Executes an <see cref="AdbCommand"/> on the running Adb Server
        /// </summary>
        /// <remarks>This should be used if you want the output of the command returned</remarks>
        /// <param name="command">Instance of <see cref="AdbCommand"/></param>
        /// <param name="forceRegular">Forces Output of stdout, not stderror if any</param>
        /// <returns>Output of <paramref name="command"/> run on server</returns>
        public static string ExecuteAdbCommand(AdbCommand command, bool forceRegular = false)
        {
            string result = "";

            lock (_lock)
            {
                result = Command.RunProcessReturnOutput(XiaomiController.Instance.ResourceDirectory + ADB_EXE, command.Command, forceRegular, command.Timeout);
            }

            return result;
        }

        /// <summary>
        /// Executes an <see cref="AdbCommand"/> on the running Adb Server
        /// </summary>
        /// <remarks>This should be used if you do not want the output of the command returned.  Good for quick abd shell commands</remarks>
        /// <param name="command">Instance of <see cref="AdbCommand"/></param>
        /// <returns>Output of <paramref name="command"/> run on server</returns>
        public static void ExecuteAdbCommandNoReturn(AdbCommand command)
        {
            lock (_lock)
            {
                Command.RunProcessNoReturn(XiaomiController.Instance.ResourceDirectory + ADB_EXE, command.Command, command.Timeout);
            }
        }

        /// <summary>
        /// Executes an <see cref="AdbCommand"/> on the running Adb Server
        /// </summary>
        /// <param name="command">Instance of <see cref="AdbCommand"/></param>
        /// <returns>Exit code of the process</returns>
        public static int ExecuteAdbCommandReturnExitCode(AdbCommand command)
        {
            int result = -1;

            lock (_lock)
            {
                result = Command.RunProcessReturnExitCode(XiaomiController.Instance.ResourceDirectory + ADB_EXE, command.Command, command.Timeout);
            }

            return result;
        }

        /// <summary>
        /// Gets a value indicating if an Android Debug Bridge Server is currently running.
        /// </summary>
        public static bool ServerRunning
        {
            get { return Command.IsProcessRunning(ADB); }
        }
        internal static void StartServer()
        {
            ExecuteAdbCommandNoReturn(FormAdbCommand("start-server"));
        }

        internal static void KillServer()
        {
            ExecuteAdbCommandNoReturn(FormAdbCommand("kill-server"));
        }
        //new
        
        internal static bool GetSS()
        {
            bool sc = false;
            //Process.Start(XiaomiController.Instance.ResourceDirectory + "ss.exe");
            var Arguments = @XiaomiController.Instance.ResourceDirectory + ADB_EXE + " exec-out screencap -p > ss.raw";
            //Process.Start(@"cmd.exe", "/c " + Arguments);
            var streamImageProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"cmd.exe",
                    Arguments = "/c " + Arguments,
                    Verb = "runas",
                    WorkingDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                    RedirectStandardError = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                },
            };
            streamImageProcess.Start();
            streamImageProcess.WaitForExit();
            if (streamImageProcess.HasExited)
            {
                sc = true;
            }
            return sc;
        }
        /// <summary>
        /// adb get device
        /// </summary>
        internal static string Devices()
        {
            return ExecuteAdbCommand(FormAdbCommand("devices"), true);
        }
        
        /// <summary>
        /// Start adb wireless o specifix port
        /// </summary>
        public static string StartWireless(string port)
        {
            var x = ExecuteAdbCommand(FormAdbCommand("adb tcpip " + port));
            var res = ".*?";
            res += "error";
            Regex r = new Regex(res, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match m = r.Match(x);
            if (m.Success)
            {
                return res = "error";
            } else
            {
                return res = "succes";
            }
        }
        /// <summary>
        /// connect to android device based on phone ip address
        /// </summary>
        public static string Connect(string ip)
        {
            var x = ExecuteAdbCommand(FormAdbCommand("adb connect " + ip));
            var res = ".*?";
            res += "unable";
            Regex r = new Regex(res, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match m = r.Match(x);
            if (m.Success)
            {
                return res = "error";
            }
            else
            {
                return res = "succes";
            }
        }
    }
}