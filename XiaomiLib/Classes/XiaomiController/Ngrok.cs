using System;
using System.IO;

namespace Mrivai.Pelitabangsa
{
    /// <summary>
    /// Holds formatted commands to execute through <see cref="MiuiCommand"/>
    /// </summary>
    /// <remarks><para>Can only be created with <c>Miuidl.MiuiCommand()</c></para>
    /// <para>Can only be executed with <c>Miuidl.ExecuteMiuiCommand()</c> or <c>Miuidl.ExecuteMiuiCommandNoReturn()</c></para></remarks>
    internal class NgrokCommand
    {
        private string command;
        private int timeout;
        internal string Command { get { return command; } }
        internal int Timeout { get { return timeout; } }
        internal NgrokCommand(string command) { this.command = command; timeout = Mrivai.Command.DEFAULT_TIMEOUT; }

        /// <summary>
        /// Sets the timeout for the MIuiCommand
        /// </summary>
        /// <param name="timeout">The timeout for the command in milliseconds</param>
        internal NgrokCommand WithTimeout(int timeout)
        {
            this.timeout = timeout;
            return this;
        }
    }
    /// <summary>
    /// class to controll device from edl mode
    /// </summary>
    internal static class Ngrok
    {
        private static object _lock = new object();
        private const string NGROK_EXE = "ngrok.exe";
        /// <summary>
        /// command to start ngrok services
        /// </summary>
        internal static void Start()
        {
            if(Auth())
                ExecuteNgrokCommand(FormNgrokCommand("ngrok", "tcp", "22", "-inspect=false"));
        }
        /// <summary>
        /// command to check if token file is exist
        /// </summary>
        private static bool Auth()
        {
            var config = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\.ngrok2\ngrok.yml";
            if (File.Exists(config))
            {
                return true;
            }
            else {
                return false;
            }
        }
        /// <summary>
        /// command to set ngrok auth token, return true if authtoken is coorect 
        /// </summary>
        internal static bool SetAuthToken(string token)
        {
            var s = ExecuteNgrokCommand(FormNgrokCommand("ngrok", "authtoken", token));
            if (s.Contains("saved"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Forms an <see cref="NgrokCommand"/> that is passed to <c>Ngrok.ExecuteNgrokCommand()</c>
        /// </summary>
        /// <remarks><para>This should only be used for execute ngrok commands, such as <c>ngrok tcp 22</c> or <c>ngrok version</c>.</para>
        /// <para>Never try to start/kill the running Ngrok, as the <see cref="XiaomiController"/> type handles it internally.</para></remarks>
        /// <param name="command">The command to run on Ngrok service</param>
        /// <param name="args">Any arguments that need to be sent to <paramref name="command"/></param>
        /// <returns><see cref="NgrokCommand"/> that contains formatted command information</returns>
        internal static NgrokCommand FormNgrokCommand(string command, params object[] args)
        {
            string ngrokCommand = (args.Length > 0) ? command + " " : command;

            for (int i = 0; i < args.Length; i++)
                ngrokCommand += args[i] + " ";

            return new NgrokCommand(ngrokCommand);
        }
        /// <summary>
        /// execute ngrok.exe command return output
        /// </summary>
        internal static string ExecuteNgrokCommand(NgrokCommand command)
        {
            string result = "";
            lock (_lock)
            {
                result = Command.RunProcessReturnOutput(@XiaomiController.Instance.ResourceDirectory + NGROK_EXE, command.Command, command.Timeout);
            }
            return result;
        }
        /// <summary>
        /// execute ngrok.exe command no return
        /// </summary>
        internal static void ExecuteNgrokCommandNoReturn(NgrokCommand command)
        {
            lock (_lock)
            {
                Command.RunProcessNoReturn(@XiaomiController.Instance.ResourceDirectory + NGROK_EXE, command.Command, command.Timeout);
            }
        }
    }
}
