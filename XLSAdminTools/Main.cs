using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XLMultiplayerServer;
using Newtonsoft.Json;
using System.IO;

namespace XLSAdminTools
{
    public class Main
    {

        private static Plugin pluginInfo;

        public static Users allUsers;

        public static List<PluginPlayer> mutedPlayers;

        public static void Load(Plugin p)
        {
            pluginInfo = p;
            pluginInfo.OnToggle = OnToggle;
            pluginInfo.ReceiveUsername = OnConnect;
            pluginInfo.ProcessMessage = OnMessageReceive;
            pluginInfo.PlayerCommand = OnPlayerCommand;
            pluginInfo.OnChatMessage = OnChatMessage;
            pluginInfo.ServerCommand = OnServerCommand;
            mutedPlayers = new List<PluginPlayer>();
        }

        private static void OnToggle(bool enabled)
        {
            if (enabled)
            {
                if (!File.Exists(Path.Combine(pluginInfo.path, "users.json")))
                {
                    System.IO.File.WriteAllText(Path.Combine(pluginInfo.path, "users.json"), "{\"users\": [], \"bannedUsers\": []}");
                } else
                {
                    allUsers = JsonConvert.DeserializeObject<Users>(File.ReadAllText(Path.Combine(pluginInfo.path, "users.json")));
                }
            }
        }

        private static void OnConnect(PluginPlayer player, string name)
        {
            string uuid = Guid.NewGuid().ToString();
            byte[] message = Encoding.ASCII.GetBytes(">uuid " + uuid);
            pluginInfo.LogMessage("Sending Player UUID:" + uuid, ConsoleColor.Blue);
            pluginInfo.SendMessage(pluginInfo, player.GetPlayer(), message, true);
            pluginInfo.LogMessage(player.loadedPlugins.ToString(), ConsoleColor.Yellow);
        }

        private static bool OnChatMessage(PluginPlayer player, string msg)
        {
            if (mutedPlayers.Contains(player))
            {
                pluginInfo.SendImportantMessageToPlayer("You have been muted!", 5, "f00", player.GetPlayer());
                return false;
            }
            return true;
        }

        private static void OnServerCommand(string command)
        {
            string[] msg = command.Split(' ');
            switch (msg[0])
            {
                case "makeadmin":
                    if (msg.Length < 2)
                    {
                        pluginInfo.LogMessage("Missing Person to Promote.", ConsoleColor.Red);
                    }
                    int promoteID = int.Parse(msg[1]);
                    PluginPlayer toPromote = pluginInfo.playerList.Find((p) => p.playerID == promoteID);
                    if (toPromote != null)
                    {
                        foreach (User u in allUsers.users)
                        {
                            if (u.player == toPromote)
                            {
                                u.IsAdmin = true;
                                File.WriteAllText(Path.Combine(pluginInfo.path, "users.json"), JsonConvert.SerializeObject(allUsers));
                                pluginInfo.LogMessage("Made " + toPromote.username + " an Admin!", ConsoleColor.Green);
                                break;
                            }
                        }
                    }
                    pluginInfo.LogMessage("User not Found!", ConsoleColor.Red);
                    break;
                case "mute":
                    if (msg.Length < 2)
                    {
                        pluginInfo.LogMessage("Missing Person to Mute. Please use ID. Example: /kick 3", ConsoleColor.Red);
                    }
                    int pIDTK = int.Parse(msg[1]);
                    PluginPlayer pTK = pluginInfo.playerList.Find((p) => p.playerID == pIDTK);
                    if (pTK != null)
                    {
                        mutedPlayers.Add(pTK);
                        pluginInfo.LogMessage("Muted " + pTK.username, ConsoleColor.Green);
                        pluginInfo.SendImportantMessageToPlayer("You have been muted!", 5, "ff0000", pTK.GetPlayer());
                    }
                    break;
                case "reloadmaps":
                    pluginInfo.LogMessage("Reloading Maps...", ConsoleColor.Green);
                    pluginInfo.ReloadMapList();
                    break;
                case "maps":
                    string maps = "";
                    foreach (string m in pluginInfo.mapList.Values)
                    {
                        maps = maps + "\n" + m;
                    }
                    pluginInfo.LogMessage(maps, ConsoleColor.Green);
                    break;
                case "ban":
                    if (msg.Length < 2)
                    {
                        pluginInfo.LogMessage("Missing Person to Ban. Please use ID. Example: /kick 3", ConsoleColor.Red);
                    }
                    else
                    {
                        try
                        {
                            int integer = int.Parse(msg[1]);
                        }
                        catch (System.FormatException)
                        {
                            pluginInfo.LogMessage("Bad formatting. Please Use ID. Example: /kick 3", ConsoleColor.Red);
                            break;
                        }
                        int playerIDToKick = int.Parse(msg[1]);
                        PluginPlayer playerToKick = pluginInfo.playerList.Find((p) => p.playerID == playerIDToKick);
                        if (playerToKick != null)
                        {
                            foreach (User u in allUsers.users)
                            {
                                if (playerToKick == u.player)
                                {
                                    if (allUsers.addBannedUser(u))
                                    {
                                        pluginInfo.LogMessage("Banned Player: " + pluginInfo.playerList[playerIDToKick].username, ConsoleColor.Red);
                                        File.WriteAllText(Path.Combine(pluginInfo.path, "users.json"), JsonConvert.SerializeObject(allUsers));
                                    }
                                    else
                                    {
                                        pluginInfo.LogMessage("Player: " + msg[1] + " is already banned!", ConsoleColor.Red);
                                    }
                                }
                            }
                        }
                        else
                        {
                            pluginInfo.LogMessage("Player with ID: " + msg[1] + " Could not be found!", ConsoleColor.Red);

                        }
                    }
                    break;
                case "changemap":
                    if (msg.Length < 2)
                    {
                        pluginInfo.LogMessage("Missing a Map Name!", ConsoleColor.Red);
                        break;
                    }
                    string[] mapNameArgs = msg.Skip(1).ToArray();
                    string mapName = String.Join(" ", mapNameArgs).ToLower().Trim();
                    bool changedMap = false;
                    foreach (string m in pluginInfo.mapList.Keys)
                    {
                        pluginInfo.LogMessage(m, ConsoleColor.DarkCyan);
                        if (pluginInfo.mapList[m].ToLower().Trim() == mapName)
                        {
                            pluginInfo.LogMessage("Changing To Map: " + mapName, ConsoleColor.Green);
                            pluginInfo.ChangeMap(m);
                            changedMap = true;
                            break;
                        }
                    }
                    if (!changedMap)
                        pluginInfo.LogMessage("No Map Found with That Name!", ConsoleColor.Red);
                    break;
                case "kick":
                    if (msg.Length < 2)
                    {
                        pluginInfo.LogMessage("Missing Person to Kick. Please use ID. Example: /kick 3", ConsoleColor.Red);
                    }
                    else
                    {
                        try
                        {
                            int integer = int.Parse(msg[1]);
                        }
                        catch (System.FormatException)
                        {
                            pluginInfo.LogMessage("Bad formatting. Please Use ID. Example: /kick 3", ConsoleColor.Red);
                            break;
                        }
                        int playerIDToKick = int.Parse(msg[1]);
                        PluginPlayer playerToKick = pluginInfo.playerList.Find((p) => p.playerID == playerIDToKick);
                        if (playerToKick != null)
                        {
                            pluginInfo.LogMessage("Kicking Player: " + pluginInfo.playerList[playerIDToKick].username, ConsoleColor.Red);
                            pluginInfo.DisconnectPlayer(playerToKick.GetPlayer());
                        }
                        else
                        {
                            pluginInfo.LogMessage("Player with ID: " + msg[1] + " Could not be found!", ConsoleColor.Red);

                        }
                    }
                    break;
                default:
                    pluginInfo.LogMessage("Unknown Command. Type /help for Commands", ConsoleColor.Red);
                    break;
            }
        }

        private static bool OnPlayerCommand(string command, PluginPlayer player)
        {
            bool isAdmin = false;
            foreach (User u in allUsers.users)
            {
                if (u.player == player)
                {
                    isAdmin = u.IsAdmin;
                }
            }
            if (isAdmin)
            {
                string[] msg = command.Split(' ');
                switch (msg[0])
                {
                    case "/help":
                        pluginInfo.SendImportantMessageToPlayer("/help - Display This Menu\n/ban {id} - Bans player by ID\n/kick {id} - Kicks player by ID\n/mute {id} - Mutes player by ID until server is restarted\n/unmute {id} - Unmutes Player by Id\n/maps - Get Map List\n/reloadmaps - Reloads map list\n/changemap {name} - Changes map to map with name provided.\n\nCreating Admins - To add an admin, type 'makeadmin {id}' into the server console or go to Plugins/Macs.XLSAdminTools/users.json and find the player you'd like to promote. You can then do so by change IsAdmin from False to True!", 10, "4287f5", player.GetPlayer());
                        break;
                    case "/mute":
                        if (msg.Length < 2)
                        {
                            pluginInfo.SendImportantMessageToPlayer("Missing Person to Mute. Please use ID. Example: /kick 3", 5, "f00", player.GetPlayer());
                        }
                        int pIDTK = int.Parse(msg[1]);
                        PluginPlayer pTK = pluginInfo.playerList.Find((p) => p.playerID == pIDTK);
                        if (pTK != null)
                        {
                            mutedPlayers.Add(pTK);
                            pluginInfo.SendImportantMessageToPlayer("Muted " + pTK.username, 5, "09db36", player.GetPlayer());
                            pluginInfo.SendImportantMessageToPlayer("You have been muted!", 5, "ff0000", player.GetPlayer());
                        }
                        break;
                    case "/reloadmaps":
                        pluginInfo.SendImportantMessageToPlayer("Reloading Maps...", 5, "09db36", player.GetPlayer());
                        pluginInfo.ReloadMapList();
                        break;
                    case "/maps":
                        string maps = "";
                        foreach (string m in pluginInfo.mapList.Values)
                        {
                            maps = maps + "\n" + m;
                        }
                        pluginInfo.SendImportantMessageToPlayer(maps, 5, "09db36", player.GetPlayer());
                        break;
                    case "/ban":
                        if (msg.Length < 2)
                        {
                            pluginInfo.SendImportantMessageToPlayer("Missing Person to Ban. Please use ID. Example: /kick 3", 5, "f00", player.GetPlayer());
                        }
                        else
                        {
                            try
                            {
                                int integer = int.Parse(msg[1]);
                            }
                            catch (System.FormatException)
                            {
                                pluginInfo.SendImportantMessageToPlayer("Bad formatting. Please Use ID. Example: /kick 3", 5, "f00", player.GetPlayer());
                                break;
                            }
                            int playerIDToKick = int.Parse(msg[1]);
                            PluginPlayer playerToKick = pluginInfo.playerList.Find((p) => p.playerID == playerIDToKick);
                            if (playerToKick != null)
                            {
                                foreach (User u in allUsers.users)
                                {
                                    if (playerToKick == u.player)
                                    {
                                        if (allUsers.addBannedUser(u))
                                        {
                                            pluginInfo.SendImportantMessageToPlayer("Banned Player: " + pluginInfo.playerList[playerIDToKick].username, 5, "09db36", player.GetPlayer());
                                            File.WriteAllText(Path.Combine(pluginInfo.path, "users.json"), JsonConvert.SerializeObject(allUsers));
                                        } else
                                        {
                                            pluginInfo.SendImportantMessageToPlayer("Player: " + msg[1] + " is already banned!", 5, "f00", player.GetPlayer());
                                        }
                                    }
                                }
                            }
                            else
                            {
                                pluginInfo.SendImportantMessageToPlayer("Player with ID: " + msg[1] + " Could not be found!", 5, "f00", player.GetPlayer());

                            }
                        }
                        break;
                    case "/changemap":
                        if (msg.Length < 2)
                        {
                            pluginInfo.SendImportantMessageToPlayer("Missing a Map Name!", 5, "f00", player.GetPlayer());
                            break;
                        }
                        string[] mapNameArgs = msg.Skip(1).ToArray();
                        string mapName = String.Join(" ", mapNameArgs).ToLower().Trim();
                        bool changedMap = false;
                        foreach (string m in pluginInfo.mapList.Keys)
                        {
                            pluginInfo.LogMessage(m, ConsoleColor.DarkCyan);
                            if (pluginInfo.mapList[m].ToLower().Trim() == mapName)
                            {
                                pluginInfo.SendImportantMessageToPlayer("Changing To Map: " + mapName, 5, "09db36", player.GetPlayer());
                                pluginInfo.ChangeMap(m);
                                changedMap = true;
                                break;
                            }
                        }
                        if (!changedMap)
                            pluginInfo.SendImportantMessageToPlayer("No Map Found with That Name!", 5, "f00", player.GetPlayer());
                        break;
                    case "/kick":
                        if (msg.Length < 2)
                        {
                            pluginInfo.SendImportantMessageToPlayer("Missing Person to Kick. Please use ID. Example: /kick 3", 5, "f00", player.GetPlayer());
                        } else
                        {
                            try
                            {
                                int integer = int.Parse(msg[1]);
                            } catch (System.FormatException) {
                                pluginInfo.SendImportantMessageToPlayer("Bad formatting. Please Use ID. Example: /kick 3", 5, "f00", player.GetPlayer());
                                break;
                            }
                            int playerIDToKick = int.Parse(msg[1]);
                            PluginPlayer playerToKick = pluginInfo.playerList.Find((p) => p.playerID == playerIDToKick);
                            if (playerToKick != null)
                            {
                                pluginInfo.SendImportantMessageToPlayer("Kicking Player: " + pluginInfo.playerList[playerIDToKick].username, 5, "09db36", player.GetPlayer());
                                pluginInfo.DisconnectPlayer(playerToKick.GetPlayer());
                            } else
                            {
                                pluginInfo.SendImportantMessageToPlayer("Player with ID: " + msg[1] + " Could not be found!", 5, "f00", player.GetPlayer());

                            }
                        }
                        break;
                    default:
                        pluginInfo.SendImportantMessageToPlayer("Unknown Command. Type /help for Commands", 5, "f00", player.GetPlayer());
                        break;
                }
            } else
            {
                pluginInfo.SendImportantMessageToPlayer("You have insufficient permissions.", 5, "f00", player.GetPlayer());
            }
            return true;   
        }

        private static void OnMessageReceive(PluginPlayer player, byte[] message)
        {
            string msg = Encoding.ASCII.GetString(message);
            string[] msgSplit = msg.Split(':');
            switch (msgSplit[0])
            {
                case ">uuid":
                    User u = new User();
                    User existingUser = null;
                    if (allUsers.users != null)
                    {
                        foreach (User user in allUsers.users)
                        {
                            if (msgSplit[1].Equals(user.ID))
                            {
                                existingUser = user;
                            }
                        }
                    }
                    if (existingUser != null)
                    {
                        if (allUsers.bannedUsers != null)
                        {

                            if (allUsers.bannedUsers.Contains(existingUser.ID))
                            {
                                pluginInfo.SendImportantMessageToPlayer("You have been banned from this server!", 5, "f00", player.GetPlayer());
                                pluginInfo.DisconnectPlayer(player.GetPlayer());
                            }
                        }
                        if (existingUser.IsAdmin || existingUser.IsOwner)
                        {
                            if (existingUser.IsOwner)
                            {
                                player.AddUsernamePrefix("<b>OWNER </b>");
                            } else
                            {
                                pluginInfo.LogMessage("Admin Joined: " + player.username, ConsoleColor.Red);
                                player.AddUsernamePrefix("<b>ADMIN </b>");
                            }
                        }
                        existingUser.player = player;
                        File.WriteAllText(Path.Combine(pluginInfo.path, "users.json"), JsonConvert.SerializeObject(allUsers));
                    }
                    else
                    {
                        u.ID = msgSplit[1];
                        u.IsAdmin = false;
                        u.Username = player.username;
                        u.player = player;
                        u.IsOwner = false;
                        allUsers.addUser(u);
                        File.WriteAllText(Path.Combine(pluginInfo.path, "users.json"), JsonConvert.SerializeObject(allUsers));
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
