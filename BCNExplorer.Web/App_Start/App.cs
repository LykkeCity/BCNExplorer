using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using Common.Log;
using Core.Settings;

namespace BCNExplorer.Web
{
    public static class App
    {
        private static readonly Lazy<string> _version = new Lazy<string>(GetVersion);
        public static string Version => _version.Value;

        public static ILog Log { get; set; }

        public static BaseSettings BaseSettings { get; set; }

        private static string GetVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fileVersionInfo.ProductVersion;

            return version;
        }
    }
}