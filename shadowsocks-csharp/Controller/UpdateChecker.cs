﻿using Shadowsocks.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Windows.Forms;

namespace Shadowsocks.Controller
{
    public class UpdateChecker
    {
        private const string UpdateURL = "https://raw.githubusercontent.com/breakwa11/breakwa11.github.io/master/update/ssr-win-4.0.xml";

        public string LatestVersionNumber;
        public string LatestVersionURL;
        public event EventHandler NewVersionFound;

        public const string Name = "ShadowsocksR";
        public const string Copyright = "Copyright © BreakWa11 2017. Fork from Shadowsocks by clowwindy";
        public const string Version = "4.6.2";
#if !_DOTNET_4_0
        public const string NetVer = "2.0";
#elif !_CONSOLE
        public const string NetVer = "4.0";
#else
        public const string NetVer = "";
#endif
        public const string FullVersion = Version +
#if DEBUG
        " Debug";
#else
/*
        " Alpha";
/*/
        "";
//*/
#endif

        private static bool UseProxy = true;

        public void CheckUpdate(Configuration config)
        {
            try
            {
                WebClient http = new WebClient();
                http.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.3319.102 Safari/537.36");
                if (UseProxy)
                {
                    WebProxy proxy = new WebProxy(IPAddress.Loopback.ToString(), config.localPort);
                    if (!string.IsNullOrEmpty(config.authPass))
                    {
                        proxy.Credentials = new NetworkCredential(config.authUser, config.authPass);
                    }
                    http.Proxy = proxy;
                }
                else
                {
                    http.Proxy = null;
                }
                //UseProxy = !UseProxy;
                http.DownloadStringCompleted += http_DownloadStringCompleted;
                http.DownloadStringAsync(new Uri(UpdateURL + "?rnd=" + Util.Utils.RandUInt32().ToString()));
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
            }
        }

        public static int CompareVersion(string l, string r)
        {
            var ls = l.Split('.');
            var rs = r.Split('.');
            for (int i = 0; i < Math.Max(ls.Length, rs.Length); i++)
            {
                int lp = (i < ls.Length) ? int.Parse(ls[i]) : 0;
                int rp = (i < rs.Length) ? int.Parse(rs[i]) : 0;
                if (lp != rp)
                {
                    return lp - rp;
                }
            }
            return 0;
        }

        public class VersionComparer : IComparer<string>
        {
            // Calls CaseInsensitiveComparer.Compare with the parameters reversed. 
            public int Compare(string x, string y)
            {
                return CompareVersion(ParseVersionFromURL(x), ParseVersionFromURL(y));
            }

        }

        private static string ParseVersionFromURL(string url)
        {
            Match match = Regex.Match(url, @".*" + Name + @"-win.*?-([\d\.]+)\.\w+", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                if (match.Groups.Count == 2)
                {
                    return match.Groups[1].Value;
                }
            }
            return null;
        }

        private void SortVersions(List<string> versions)
        {
            versions.Sort(new VersionComparer());
        }

        private bool IsNewVersion(string url)
        {
            if (url.IndexOf("prerelease") >= 0)
            {
                return false;
            }
            // check dotnet 4.0
            AssemblyName[] references = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
            Version dotNetVersion = Environment.Version;
            foreach (AssemblyName reference in references)
            {
                if (reference.Name == "mscorlib")
                {
                    dotNetVersion = reference.Version;
                }
            }
            if (dotNetVersion.Major >= 4)
            {
                if (url.IndexOf("dotnet4.0") < 0)
                {
                    return false;
                }
            }
            else
            {
                if (url.IndexOf("dotnet4.0") >= 0)
                {
                    return false;
                }
            }
            string version = ParseVersionFromURL(url);
            if (version == null)
            {
                return false;
            }
            string currentVersion = Version;

            if (url.IndexOf("banned") > 0 && CompareVersion(version, currentVersion) == 0
                || url.IndexOf("deprecated") > 0 && CompareVersion(version, currentVersion) > 0)
            {
                Application.Exit();
                return false;
            }
            return CompareVersion(version, currentVersion) > 0;
        }

        private void http_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                string response = e.Result;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(response);
                XmlNodeList elements = xmlDoc.GetElementsByTagName("media:content");
                List<string> versions = new List<string>();
                foreach (XmlNode el in elements)
                {
                    foreach (XmlAttribute attr in el.Attributes)
                    {
                        if (attr.Name == "url")
                        {
                            if (IsNewVersion(attr.Value))
                            {
                                versions.Add(attr.Value);
                            }
                        }
                    }
                }
                if (versions.Count == 0)
                {
                    return;
                }
                // sort versions
                SortVersions(versions);
                LatestVersionURL = versions[versions.Count - 1];
                LatestVersionNumber = ParseVersionFromURL(LatestVersionURL);
                if (NewVersionFound != null)
                {
                    NewVersionFound(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                if (e.Error != null)
                {
                    Logging.Debug(e.Error.ToString());
                }
                Logging.Debug(ex.ToString());
                if (NewVersionFound != null)
                {
                    NewVersionFound(this, new EventArgs());
                }
                return;
            }
        }
    }
}
