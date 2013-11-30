using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Hotsoup
{
    class XML
    {
        public static void ReadXML(string fileName)
        {
            if (!System.IO.File.Exists(fileName)) throw new System.IO.FileNotFoundException("File not found.", fileName);

            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            XmlNodeList settingsList = doc.SelectNodes("/Hotsoup/Settings/*");
            foreach (XmlNode settingsNode in settingsList)
            {
                MainWindow.AutoStartDelay = Convert.ToInt32(settingsNode.SelectSingleNode("//AutoStart/@Delay").Value) * 10;
                MainWindow.timer.Enabled = Convert.ToBoolean(settingsNode.SelectSingleNode("//AutoStart/@Enabled").Value);
                MainWindow.AutoStartKey = settingsNode.SelectSingleNode("//AutoStart/@AppKey").Value;

                MainWindow.shutdownEnabled = Convert.ToBoolean(settingsNode.SelectSingleNode("//Shutdown/@Enabled").Value);
                MainWindow.shutdownDelay = Convert.ToInt32(settingsNode.SelectSingleNode("//Shutdown/@Delay").Value);
            }

            XmlNodeList appsList = doc.SelectNodes("/Hotsoup/Applications/*");
            foreach (XmlNode appsNode in appsList)
            {
                string name = appsNode.Name;
                string label = (appsNode.SelectSingleNode("@Label") == null) ? appsNode.Name : appsNode.SelectSingleNode("@Label").Value;
                bool enabled = Convert.ToBoolean(appsNode.SelectSingleNode("@Enabled").Value);
                string exeFile = appsNode.SelectSingleNode("@Executable").Value;
                string icon = (appsNode.SelectSingleNode("@Icon") == null) ? "app_icon.png" : appsNode.SelectSingleNode("@Icon").Value;

                ApplicationInfo.Add(name, enabled, label, exeFile, icon);
            }

            XmlNodeList traktList = doc.SelectNodes("/Hotsoup/Trakt.tv/*");
            foreach (XmlNode traktNode in traktList)
            {
                MainWindow.traktAutoUpdate = Convert.ToInt32(traktNode.SelectSingleNode("//AutoUpdate/@MinuteInterval").Value);
                MainWindow.timer2.Enabled = Convert.ToBoolean(traktNode.SelectSingleNode("//AutoUpdate/@Enabled").Value);

                Trakt.traktEnabled = Convert.ToBoolean(traktNode.SelectSingleNode("//Connection/@Enabled").Value);
                Trakt.timeout = Convert.ToInt32(traktNode.SelectSingleNode("//Connection/@Timeout").Value) * 1000;

                Trakt.trendEnabled = Convert.ToBoolean(traktNode.SelectSingleNode("//Trending/@Enabled").Value);
                Trakt.trendingCount = Convert.ToInt32(traktNode.SelectSingleNode("//Trending/@Count").Value);

                string username = traktNode.SelectSingleNode("//User/@Name").Value;
                string password = traktNode.SelectSingleNode("//User/@Password").Value;
                Trakt.userCred = new UserCredentials(username, Functions.GetSHA1(password));

                //Trakt.API = traktNode.SelectSingleNode("//User/@API").Value;
            }
        }
    }

    class ApplicationInfo
    {
        public string Name { set; get; }
        public bool Enabled { set; get; }
        public string Label { set; get; }
        public string Executable { set; get; }
        public string Icon { set; get; }

        public static List<ApplicationInfo> List = new List<ApplicationInfo>();

        public static void Add(string name, bool enable, string label, string exe, string icon)
        {
            ApplicationInfo ai = new ApplicationInfo();
            ai.Name = name;
            ai.Enabled = enable;
            ai.Label = label;
            ai.Executable = exe;
            ai.Icon = icon;

            List.Add(ai);
        }

        public static ApplicationInfo ReturnObj(string key)
        {
            return List.First(f => f.Name == key);
        }
    }
}
