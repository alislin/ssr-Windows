/* Ceated by Ya Lin. 2019/9/10 18:03:51 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Shadowsocks.Model
{
    public class AppdataPath
    {
        private static AppdataPath current = new AppdataPath();

        public static AppdataPath Current => current;
        public string DataPath => GetDataPath();
        public bool UseAppDataPath { get; set; } = false;
        private string GetDataPath()
        {
            var path = System.Windows.Forms.Application.StartupPath;
            if (UseAppDataPath) path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string curpath = Path.Combine(path, @"ssr_Data");// Path.GetFullPath(".");//Path.GetTempPath();
            if (!Directory.Exists(curpath))
            {
                Directory.CreateDirectory(curpath);
            }

            return curpath;
        }

        public string GetFilePath(string fileName)
        {
            var name = Path.Combine(DataPath, fileName);
            return name;
        }
    }
}
