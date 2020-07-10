using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using XLMultiplayer;
namespace XLSAdminToolsClient
{
    public class Main
    {
        private static Plugin pluginInfo;

        private static void Load(Plugin plugin)
        {
            pluginInfo = plugin;
            pluginInfo.ProcessMessage = ReceiveMessage;
            pluginInfo.OnToggle = OnToggle;
        }

        private static void OnToggle(bool enabled)
        {
            if (enabled)
            {
                pluginInfo.SendMessage(pluginInfo, new byte[] { 1, 2, 3, 4 }, true);
            }
            else
            {

            }
        }

        private static void ReceiveMessage(byte[] message)
        {
            string msg = Encoding.ASCII.GetString(message);
            string[] msgSplit = msg.Split(' ');
            pluginInfo.SendMessage(pluginInfo, Encoding.ASCII.GetBytes(">uuid:"+ SystemInfo.deviceUniqueIdentifier), true);
        }
    }
}
