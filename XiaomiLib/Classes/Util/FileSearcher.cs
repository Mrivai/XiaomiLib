

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Mrivai.Pelitabangsa.Util
{
    /// <summary>
    /// Manage file search
    /// </summary>
    public class FileSearcher
    {
        /// <summary>
        /// Search file
        /// </summary>
        public static string[] SearchFiles(string destinationDic, string pattern)
        {
            List<string> stringList = new List<string>();
            foreach (FileInfo file in new DirectoryInfo(destinationDic).GetFiles())
            {
            Match match = new Regex(pattern).Match(file.Name);
            if (match.Groups.Count > 0 && match.Groups[0].Value == file.Name)
                stringList.Add(file.FullName);
            }
            return stringList.ToArray();
        }
    }
}
