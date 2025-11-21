using Microsoft.Win32;
using System.Runtime.Versioning;

namespace HtmlPrinter
{
    [SupportedOSPlatform("windows")]
    internal class ChromeEdgeHelper
    {
        public static string? FindChromeOrEdge()
        {
            var edge = GetEdgePath();
            if (File.Exists(edge)) return edge;
            return GetChromePath();
        }

        static string? GetEdgePath()
        {
            // 注册表路径
            var regPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\msedge.exe";

            // 先查系统级别 (HKLM)
            var path = GetPathFromRegistry(RegistryHive.LocalMachine, regPath);
            if (!string.IsNullOrEmpty(path))
                return path;

            // 再查用户级别 (HKCU)
            path = GetPathFromRegistry(RegistryHive.CurrentUser, regPath);
            if (!string.IsNullOrEmpty(path))
                return path;

            return null; // 未找到
        }

        static string? GetChromePath()
        {
            // 注册表路径
            var regPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe";

            // 先查系统级别 (HKLM)
            var path = GetPathFromRegistry(RegistryHive.LocalMachine, regPath);
            if (!string.IsNullOrEmpty(path))
                return path;

            // 再查用户级别 (HKCU)
            path = GetPathFromRegistry(RegistryHive.CurrentUser, regPath);
            if (!string.IsNullOrEmpty(path))
                return path;

            return null; // 未找到
        }

        static string? GetPathFromRegistry(RegistryHive hive, string subKey)
        {
            // 支持 32/64 位系统
            RegistryView[] views = { RegistryView.Registry64, RegistryView.Registry32 };

            foreach (var view in views)
            {
                try
                {
                    using var baseKey = RegistryKey.OpenBaseKey(hive, view);
                    using var key = baseKey.OpenSubKey(subKey);
                    if (key != null)
                    {
                        var value = key.GetValue(string.Empty) as string; // 默认值
                        if (!string.IsNullOrEmpty(value))
                            return value;
                    }
                }
                catch
                {
                    // 忽略异常，继续尝试
                }
            }

            return null;
        }
    }
}
