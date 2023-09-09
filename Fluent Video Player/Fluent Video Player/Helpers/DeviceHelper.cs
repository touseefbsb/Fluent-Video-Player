using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Globalization;
using System.Reflection;
using Windows.ApplicationModel;

namespace Fluent_Video_Player.Helpers
{
    public static class DeviceHelper
    {
        public enum Device
        {
            Desktop,
            Xbox,
            Mobile,
            WindowsTeam,
            VR,
            IoT
        }
        
        public static Device GetDevice()
        {
            Device device = Device.Desktop;
            switch (DeviceFamily)
            {
                case "Windows.Desktop":
                    device = Device.Desktop; break;
                case "Windows.Mobile":
                    device = Device.Mobile; break;
                case "Windows.Xbox":
                    device = Device.Xbox; break;
                case "Windows.Holographic":
                    device = Device.VR; break;
                case "Windows.IoT":
                    device = Device.IoT; break;
                case "Windows.Team":
                    device = Device.WindowsTeam; break;
                default:
                    break;
            }
            return device;
        }
        // To get application's name:
        public static string ApplicationName => SystemInformation.ApplicationName;

        // To  staticget application's version:
        public static string ApplicationVersion => $"{SystemInformation.ApplicationVersion.Major}.{SystemInformation.ApplicationVersion.Minor}.{SystemInformation.ApplicationVersion.Build}.{SystemInformation.ApplicationVersion.Revision}";

        // To  staticget the most preferred language by the user:
        public static CultureInfo Culture => SystemInformation.Culture;

        // To  staticget operating syste,
        public static string OperatingSystem => SystemInformation.OperatingSystem;

        // To  staticget used processor architecture
        public static ProcessorArchitecture OperatingSystemArchitecture => (ProcessorArchitecture)SystemInformation.OperatingSystemArchitecture;

        // To  staticget operating system version
        public static OSVersion OperatingSystemVersion => SystemInformation.OperatingSystemVersion;

        // To  staticget device family
        public static string DeviceFamily => SystemInformation.DeviceFamily;

        // To  staticget device model
        public static string DeviceModel => SystemInformation.DeviceModel;

        // To  staticget device manufacturer
        public static string DeviceManufacturer => SystemInformation.DeviceManufacturer;

        // To  staticget available memory in MB
        public static float AvailableMemory => SystemInformation.AvailableMemory;

        // To  staticget if the app is being used for the first time since it was installed.
        public static bool IsFirstUse => SystemInformation.IsFirstRun;

        // To  staticget if the app is being used for the first time since being upgraded from an older version.
        public static bool IsAppUpdated => SystemInformation.IsAppUpdated;

        // To  staticget the first version installed
        public static PackageVersion FirstVersionInstalled => SystemInformation.FirstVersionInstalled;

        // To  staticget the first time the app was launched
        public static DateTime FirstUseTime => SystemInformation.FirstUseTime;

        // To  staticget the time the app was launched.
        public static DateTime LaunchTime => SystemInformation.LaunchTime;

        // To  staticget the time the app was previously launched, not including this instance.
        public static DateTime LastLaunchTime => SystemInformation.LastLaunchTime;

        // To  staticget the time the launch count was reset, not including this instance
        public static string LastResetTime => SystemInformation.LastResetTime.ToString(Culture.DateTimeFormat);

        // To  staticget the number of times the app has been launched sicne the last reset.
        public static long LaunchCount => SystemInformation.LaunchCount;

        // To  staticget the number of times the app has been launched.
        public static long TotalLaunchCount => SystemInformation.TotalLaunchCount;

        // To  staticget how long the app has been running
        public static TimeSpan AppUptime => SystemInformation.AppUptime;

    }
}
