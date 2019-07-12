using System;
using System.Collections.Generic;

namespace Mrivai.Pelitabangsa
{
    public class Apps
    {
        public string name;
        public string packagename;

        internal Apps(string nm, string pm)
        {
            name = nm;
            packagename = pm;
        }

        public static List<Apps> getlist()
        {
            List<Apps> Applist = new List<Apps>();
            Applist.Clear();
            string sts = AdbCmd.ExecuteAdbCommand(AdbCmd.FormAdbCommand("shell pm list packages -3"));
            string[] m = sts.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string z in m)
            {
                var name = z.Replace("package:", "");
                var pm = name;
                name = name.Replace("com.", "").Replace(".", " ");
                Logger.w(name, "",null);
                Applist.Add(new Apps(name, pm));
            }
            return Applist;
        }
    }
}
