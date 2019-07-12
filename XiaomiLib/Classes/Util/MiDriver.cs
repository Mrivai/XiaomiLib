using Microsoft.Win32;
using System;
using System.IO;

namespace Mrivai
{
    public class MiDriver
    {
        public string[] infFiles = new string[] { "Google\\Driver\\android_winusb.inf", "Nvidia\\Driver\\NvidiaUsb.inf", "Microsoft\\Driver\\tetherxp.inf", "Microsoft\\Driver\\wpdmtphw.inf", "Qualcomm\\Driver\\qcser.inf" };

        public MiDriver()
        {
        }

        public void CopyFiles(string installationPath)
        {
            installationPath = installationPath.Substring(0, installationPath.LastIndexOf('\\') + 1);
            try
            {
                string systemDirectory = Environment.SystemDirectory;
                string[] strArrays = new string[] { "Qualcomm\\Driver\\serial\\i386\\qcCoInstaller.dll" };
                string.Concat(systemDirectory, "\\qcCoInstaller.dll");
                string.Concat(installationPath, "File\\ThirdParty\\");
                int num = 0;
                while (num < (int)strArrays.Length)
                {
                    num++;
                }
            }
            catch (Exception exception1)
            {
                Logger.w(exception1.Message, "MiDriver Installer: copy file failed", exception1.StackTrace);
            }
        }

        public void InstallAllDriver(string installationPath, bool uninstallOld)
        {
            installationPath = installationPath.Substring(0, installationPath.LastIndexOf('\\') + 1);
            string str = string.Concat(installationPath, "File\\ThirdParty\\");
            if (!(new DirectoryInfo(str)).Exists)
            {
                Logger.w(installationPath, "MiDriver Installer: Directory Not Exists:", null);
                return;
            }
            for (int i = 0; i < (int)this.infFiles.Length; i++)
            {
                this.InstallDriver(string.Concat(str, this.infFiles[i]), installationPath, uninstallOld);
            }
        }

        public void InstallDriver(string infPath, string installationPath, bool uninstallOld)
        {
            try
            {
                string str = "Software\\XiaoMi\\MiFlash\\";
                FileInfo fileInfo = new FileInfo(infPath);
                RegistryKey localMachine = Registry.LocalMachine;
                RegistryKey registryKey = localMachine.OpenSubKey(str, true);
                Logger.w(string.Format("open RegistryKey {0}", str), "MiDriver Installer:", null);
                if (registryKey == null)
                {
                    registryKey = localMachine.CreateSubKey(str, RegistryKeyPermissionCheck.ReadWriteSubTree);
                    Logger.w(string.Format("create RegistryKey {0}", str), "MiDriver Installer:", null);
                }
                registryKey.GetValueNames();
                object value = registryKey.GetValue(fileInfo.Name);
                bool flag = true;
                string str1 = "";
                if (value != null && uninstallOld)
                {
                    str1 = Driver.UninstallInf(value.ToString(), out flag);
                    Logger.w(string.Format("driver {0} exists,uninstall,reuslt {1},GetLastWin32Error{2}", value.ToString(), flag.ToString(), str1), "MiDriver Installer:", null);
                }
                string str2 = "";
                string str3 = "";
                str1 = Driver.SetupOEMInf(fileInfo.FullName, out str3, out str2, out flag);
                object[] fullName = new object[] { fileInfo.FullName, str3, flag.ToString(), str1 };
                Logger.w(string.Format("install driver {0} to {1},result {2},GetLastWin32Error {3}", fullName), "MiDriver Installer:", null);
                if (flag)
                {
                    registryKey.SetValue(fileInfo.Name, str2);
                    Logger.w(string.Format("set RegistryKey value:{0}--{1}", fileInfo.Name, str2), "MiDriver Installer:", null);
                }
                registryKey.Close();
                if (infPath.IndexOf("android_winusb.inf") >= 0)
                {
                    string environmentVariable = Environment.GetEnvironmentVariable("USERPROFILE");
                    string str4 = string.Format("mkdir \"{0}\\.android\"", environmentVariable);
                    string str5 = Command.Execute(str4);
                    Logger.w(str4, "MiDriver Installer:", null);
                    Logger.w(str5, "MiDriver Installer:", null);
                    str4 = string.Format(" echo 0x2717 >>\"{0}\\.android\\adb_usb.ini\"", environmentVariable);
                    str5 = Command.Execute(str4);
                    Logger.w(str4, "MiDriver Installer:", null);
                    Logger.w(str5, "MiDriver Installer:", null);
                }
            }
            catch (Exception exception1)
            {

                Logger.w(exception1.Message, "MiDriver Installer:", exception1.StackTrace);
                Exception exception = exception1;
            }
        }
    }
}
