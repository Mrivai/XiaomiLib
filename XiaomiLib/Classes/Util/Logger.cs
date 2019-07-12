using System;
using System.IO;

namespace Mrivai
{
    internal static class Logger
    {

        private static string Path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "log\\";
        private static string ErrorLogPath = Path + string.Format("{0}@{1}.txt", "Xiaomilib_log", DateTime.Now.ToString("yyyyMd"));
        internal static bool w(string Message, string Title, string StackTrace)
        {
            try
            {
                if (!(new DirectoryInfo(Path)).Exists)
                {
                    Directory.CreateDirectory(Path);
                }
                if (!File.Exists(ErrorLogPath))
                    File.Create(ErrorLogPath).Close();
                else
                using (FileStream fs = new FileStream(ErrorLogPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    using (StreamWriter sw = new StreamWriter(fs))
                        sw.WriteLine(string.Format("[{0}]:{1}   {2} {3}", DateTime.Now.ToLongTimeString(), Title, Message, StackTrace));
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
