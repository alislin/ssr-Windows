﻿/* Ceated by Ya Lin. 2019/9/10 18:03:51 */

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
        private string GetDataPath()
        {
            string curpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"ssr_Data");// Path.GetFullPath(".");//Path.GetTempPath();
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
