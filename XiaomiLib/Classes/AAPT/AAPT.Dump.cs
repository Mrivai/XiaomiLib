using System;
using System.Collections.Generic;
using System.IO;

namespace Mrivai.Pelitabangsa
{
    public partial class AAPT
    {
        /// <summary>
        /// Contains Apk Badging Dump Information 
        /// </summary>
        public class Badging
        {
            #region Constants
            private const string APOSTROPHE = "'";

            private const string PACKAGE = "package:";
            private const string PACKAGE_NAME = "name='";
            private const string PACKAGE_VERSION_CODE = "versionCode='";
            private const string PACKAGE_VERSION_NAME = "versionName='";

            private const string APPLICATION = "application:";
            private const string APPLICATION_LABEL = "label='";
            private const string APPLICATION_ICON = "icon='";

            private const string ACTIVITY = "launchable-activity:";
            private const string ACTIVITY_NAME = "name='";
            private const string ACTIVITY_LABEL = "label='";
            private const string ACTIVITY_ICON = "icon='";

            private const string SDK_VERSION = "sdkVersion:'";
            private const string SDK_TARGET = "targetSdkVersion:'";

            private const string USES_PERMISSION = "uses-permission:'";

            private const string DENSITIES = "densities:";
            #endregion

            #region Fields
            private FileInfo source;

            private PackageInfo package;
            private ApplicationInfo application;
            private LaunchableActivity activity;

            private string sdkVersion;
            private string targetSdkVersion;

            private List<string> usesPermission;
            private List<int> densities;
            #endregion

            #region Properties
            /// <summary>
            /// Gets a <c>FileInfo</c> containing information about the source Apk
            /// </summary>
            public FileInfo Source { get { return source; } }

            /// <summary>
            /// Gets a <see cref="PackageInfo"/> containing infomation about the Apk
            /// </summary>
            public PackageInfo Package { get { return package; } }

            /// <summary>
            /// Gets a <see cref="ApplicationInfo"/> containing infomation about the Apk
            /// </summary>
            public ApplicationInfo Application { get { return application; } }

            /// <summary>
            /// Gets a <see cref="LaunchableActivity"/> containing infomation about the Apk
            /// </summary>
            public LaunchableActivity Activity { get { return activity; } }

            /// <summary>
            /// Gets a value indicating the Android Sdk Version of the Apk
            /// </summary>
            public string SdkVersion { get { return sdkVersion; } }

            /// <summary>
            /// Gets a value indicating the Target Android Sdk Version of the Apk
            /// </summary>
            public string TargetSdkVersion { get { return targetSdkVersion; } }

            /// <summary>
            /// Gets a <c>List&lt;string&gt;</c> containing the Android Permissions used by the Apk
            /// </summary>
            public List<string> Permissions { get { return usesPermission; } }

            /// <summary>
            /// Gets a <c>List&lt;int&gt;</c> containing the supported screen densities of the Apk
            /// </summary>
            public List<int> ScreenDensities { get { return densities; } }
            #endregion

            internal Badging(FileInfo Source, string dump)
            {
                source = Source;

                package = new PackageInfo();
                application = new ApplicationInfo();
                activity = new LaunchableActivity();

                sdkVersion = "";
                targetSdkVersion = "";

                usesPermission = new List<string>();
                densities = new List<int>();

                ProcessDump(dump);
            }

            private void ProcessDump(string dump)
            {
                using (StringReader r = new StringReader(dump))
                {
                    string line;

                    while (r.Peek() != -1)
                    {
                        line = r.ReadLine();

                        if (line.StartsWith(PACKAGE))
                        {
                            //find name
                            int nameStart = line.IndexOf(PACKAGE_NAME) + PACKAGE_NAME.Length;
                            int nameLength = line.IndexOf(APOSTROPHE, nameStart) - nameStart;
                            string name = line.Substring(nameStart, nameLength);

                            //find versionCode
                            int versionCodeStart = line.IndexOf(PACKAGE_VERSION_CODE) + PACKAGE_VERSION_CODE.Length;
                            int versionCodeLength = line.IndexOf(APOSTROPHE, versionCodeStart) - versionCodeStart;
                            string versionCode = line.Substring(versionCodeStart, versionCodeLength);

                            //find versionName
                            int versionNameStart = line.IndexOf(PACKAGE_VERSION_NAME) + PACKAGE_VERSION_NAME.Length;
                            int versionNameLength = line.IndexOf(APOSTROPHE, versionNameStart) - versionNameStart;
                            string versionName = line.Substring(versionNameStart, versionNameLength);

                            package = new PackageInfo(name, versionCode, versionName);
                        }
                        else if (line.StartsWith(APPLICATION))
                        {
                            //find label
                            int labelStart = line.IndexOf(APPLICATION_LABEL) + APPLICATION_LABEL.Length;
                            int labelLength = line.IndexOf(APOSTROPHE, labelStart) - labelStart;
                            string label = line.Substring(labelStart, labelLength);

                            //find icon
                            int iconStart = line.IndexOf(APPLICATION_ICON) + APPLICATION_ICON.Length;
                            int iconLength = line.IndexOf(APOSTROPHE, iconStart) - iconStart;
                            string icon = line.Substring(iconStart, iconLength);

                            application = new ApplicationInfo(label, icon);
                        }
                        else if (line.StartsWith(ACTIVITY))
                        {
                            //find name
                            int nameStart = line.IndexOf(ACTIVITY_NAME) + ACTIVITY_NAME.Length;
                            int nameLength = line.IndexOf(APOSTROPHE, nameStart) - nameStart;
                            string name = line.Substring(nameStart, nameLength);

                            //find label
                            int labelStart = line.IndexOf(ACTIVITY_LABEL) + ACTIVITY_LABEL.Length;
                            int labelLength = line.IndexOf(APOSTROPHE, labelStart) - labelStart;
                            string label = line.Substring(labelStart, labelLength);

                            //find icon
                            int iconStart = line.IndexOf(ACTIVITY_ICON) + ACTIVITY_ICON.Length;
                            int iconLength = line.IndexOf(APOSTROPHE, iconStart) - iconStart;
                            string icon = line.Substring(iconStart, iconLength);

                            activity = new LaunchableActivity(name, label, icon);
                        }
                        else if (line.StartsWith(SDK_VERSION))
                        {
                            sdkVersion = line.Substring(SDK_VERSION.Length).Replace(APOSTROPHE, "");
                        }
                        else if (line.StartsWith(SDK_TARGET))
                        {
                            targetSdkVersion = line.Substring(SDK_TARGET.Length).Replace(APOSTROPHE, "");
                        }
                        else if (line.StartsWith(USES_PERMISSION))
                        {
                            usesPermission.Add(line.Substring(USES_PERMISSION.Length).Replace(APOSTROPHE, ""));
                        }
                        else if (line.StartsWith(DENSITIES))
                        {
                            string[] densities = line.Substring(DENSITIES.Length + 2).Split(new char[] { '\'', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            for (int i = 0; i < densities.Length; i++)
                                this.densities.Add(int.Parse(densities[i]));
                        }
                    }
                }
            }

            /// <summary>
            /// Contains information about an Apk's package
            /// </summary>
            public class PackageInfo
            {
                private string name;
                private string versionCode;
                private string versionName;

                internal PackageInfo() : this(null, null, null) { }
                internal PackageInfo(string Name, string VersionCode, string VersionName)
                {
                    name = Name;
                    versionCode = VersionCode;
                    versionName = VersionName;
                }

                /// <summary>
                /// Gets a value indicating the Apk's package name
                /// </summary>
                public string Name
                {
                    get { return name; }
                }

                /// <summary>
                /// Gets a value indicating the Version Code of the Apk's package
                /// </summary>
                public string VersionCode
                {
                    get { return versionCode; }
                }

                /// <summary>
                /// Gets a value indicating the Version Name of the Apk's package
                /// </summary>
                public string VersionName
                {
                    get { return versionName; }
                }
            }

            /// <summary>
            /// Contains general information about an Apk
            /// </summary>
            public class ApplicationInfo
            {
                private string label;
                private string icon;

                internal ApplicationInfo() : this(null, null) { }
                internal ApplicationInfo(string Label, string Icon)
                {
                    label = Label;
                    icon = Icon;
                }

                /// <summary>
                /// Gets a value indicating the Application's Label
                /// </summary>
                public string Label
                {
                    get { return label; }
                }

                /// <summary>
                /// Gets a value indicating the path inside the apk to the Application's default icon
                /// </summary>
                public string Icon
                {
                    get { return icon; }
                }
            }

            /// <summary>
            /// Contains information about an Apk's main Activity
            /// </summary>
            public class LaunchableActivity
            {
                private string name;
                private string label;
                private string icon;

                internal LaunchableActivity() : this(null, null, null) { }
                internal LaunchableActivity(string Name, string Label, string Icon)
                {
                    name = Name;
                    label = Label;
                    icon = Icon;
                }

                /// <summary>
                /// Gets a value indicating the name of the Apk's main Activity
                /// </summary>
                public string Name
                {
                    get { return name; }
                }

                /// <summary>
                /// Gets a value indicating the label of the Apk's main Activity
                /// </summary>
                public string Label
                {
                    get { return label; }
                }

                /// <summary>
                /// Gets a value indicating the path to the default icon of the Apk's main Activity
                /// </summary>
                public string Icon
                {
                    get { return icon; }
                }
            }
        }
    }
}