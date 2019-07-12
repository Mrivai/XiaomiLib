/*
 * Command.cs - Developed by Mrivai for XiaomiLib.dll - 04/12/12
 */

using Mrivai.Pelitabangsa;
using Mrivai.Pelitabangsa.Modul;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Mrivai
{
    internal static class Command
    {
        /// <summary>
        /// The default timeout for commands. -1 implies infinite time
        /// </summary>
        public const int DEFAULT_TIMEOUT = -1;

        [Obsolete("Method is deprecated, please use RunProcessNoReturn(string, string, int) instead.")]
        internal static void RunProcessNoReturn(string executable, string arguments, bool waitForExit = true)
        {
            using (Process p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = true;

                p.Start();

                if (waitForExit)
                    p.WaitForExit();
            }
        }
        internal static void RunProcessNoReturn(string executable, string arguments, int timeout)
        {
            using (Process p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments =  arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = true;

                p.Start();

                p.WaitForExit(timeout);
            }
        }

        internal static string RunProcessReturnOutput(string executable, string arguments, int timeout)
        {
            using (Process p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                    using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                        return HandleOutput(p, outputWaitHandle, errorWaitHandle, timeout, false);
            }
        }



        internal static string RunProcessReturnOutput(string executable, string arguments, bool forceRegular, int timeout)
        {
            using (Process p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                    using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                        return HandleOutput(p, outputWaitHandle, errorWaitHandle, timeout, forceRegular);
            }
        }

        private static string HandleOutput(Process p, AutoResetEvent outputWaitHandle, AutoResetEvent errorWaitHandle, int timeout, bool forceRegular)
        {
            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();

            p.OutputDataReceived += (sender, e) =>
            {
                if (e.Data == null)
                    outputWaitHandle.Set();
                else
                    output.AppendLine(e.Data);
            };
            p.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data == null)
                    errorWaitHandle.Set();
                else
                    error.AppendLine(e.Data);
            };

            p.Start();

            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            if (p.WaitForExit(timeout) && outputWaitHandle.WaitOne(timeout) && errorWaitHandle.WaitOne(timeout))
            {
                string strReturn = "";

                if (error.ToString().Trim().Length.Equals(0) || forceRegular)
                    strReturn = output.ToString().Trim();
                else
                    strReturn = error.ToString().Trim();

                return strReturn;
            }
            else
            {
                // Timed out.
                return "PROCESS TIMEOUT";
            }
        }

        internal static int RunProcessReturnExitCode(string executable, string arguments, int timeout)
        {
            int exitCode;

            using (Process p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = true;

                p.Start();
                p.WaitForExit(timeout);
                exitCode = p.ExitCode;
            }

            return exitCode;
        }

        [Obsolete("Method is deprecated, please use RunProcessWriteInput(string, string, int, string...) instead.")]
        internal static void RunProcessWriteInput(string executable, string arguments, params string[] input)
        {
            using (Process p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;

                p.StartInfo.RedirectStandardInput = true;

                p.Start();

                using (StreamWriter w = p.StandardInput)
                    for (int i = 0; i < input.Length; i++)
                        w.WriteLine(input[i]);

                p.WaitForExit();
            }
        }

        internal static void RunProcessWriteInput(string executable, string arguments, int timeout, params string[] input)
        {
            using (Process p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;

                p.StartInfo.RedirectStandardInput = true;

                p.Start();

                using (StreamWriter w = p.StandardInput)
                    for (int i = 0; i < input.Length; i++)
                        w.WriteLine(input[i]);

                p.WaitForExit(timeout);
            }
        }

        internal static bool IsProcessRunning(string processName)
        {
            Process[] processes = Process.GetProcesses();

            foreach (Process p in processes)
                if (p.ProcessName.ToLower().Contains(processName.ToLower()))
                    return true;

            return false;
        }

        internal static void KillProcess(string processName)
        {
            Process[] processes = Process.GetProcesses();

            foreach (Process p in processes)
            {
                if (p.ProcessName.ToLower().Contains(processName.ToLower()))
                {
                    p.Kill();
                    return;
                }
            }
        }

        internal static string Execute( string dosCommand)
        {
            return ProcessReturnOutput(dosCommand);
        }

        internal static Process initCmdProcess(string command)
        {
            return new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    Arguments = "/C " + command,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
        }

        internal static string ProcessReturnOutput(string command)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (command != null && !command.Equals(""))
            {
                Process process = initCmdProcess(command);
                process.EnableRaisingEvents = true;
                try
                {
                    process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
                    process.ErrorDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
                    process.EnableRaisingEvents = true;
                    if (process.Start())
                    {
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                    Logger.w(ex.Message, "Fastboot Flasher Process Error" , ex.StackTrace);
                }
                finally
                {
                    if (process != null)
                        process.Close();
                }
            }
            return stringBuilder.ToString();
        }
        internal static void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string data = e.Data;
            if (string.IsNullOrEmpty(data))
                return;
            if (data.IndexOf("error") >= 0 || data.IndexOf("fail") >= 0)
            {
                Logger.w(e.Data, "Fastboot Flash error:", null);
                throw new Exception(e.Data);
            }
            if (data.IndexOf("rebooting") >= 0)
            {
                Flash.UpdateDeviceStatus(new float?(1f), "flash done", "success", true);
            }
            if (data.IndexOf("writing") >= 0)
            {
                string file = data.Substring(9);
                file = file.Substring(0, file.Length - 4);
                Flash.UpdateDeviceStatus(GetPercent(data), "Fastboot Flash " + file, "Flashing", false);
                Logger.w("Flash " + file, "Fastboot", null);
            }
        }

        private static float GetPercent(string line)
        {
            Hashtable dummyProgress = SoftwareImage.DummyProgress;
            float nullable = new float();
            foreach (string key in dummyProgress.Keys)
            {
                if (line.IndexOf(key) >= 0)
                {
                    nullable = Convert.ToInt32(dummyProgress[key]) / 50f;
                    break;
                }
            }
            return nullable;
        }
    }
}
