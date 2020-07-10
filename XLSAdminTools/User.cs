using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XLMultiplayerServer;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace XLSAdminTools
{

    public class User
    {
        [JsonProperty("UID")]
        public string ID { get; set; }
        [JsonProperty("IsAdmin")]
        public bool IsAdmin { get; set; }

        [JsonProperty("IsOwner")]
        public bool IsOwner { get; set; }

        [JsonProperty("Username")]
        public string Username { get; set; }

        public PluginPlayer player;
    }

    public class Users
    {
        [JsonProperty("users")]
        public List<User> users { get; set; }

        [JsonProperty("bannedUsers")]
        public List<string> bannedUsers { get; set; }

        public bool addBannedUser(User user)
        {
            if (!bannedUsers.Contains(user.ID))
            {
                bannedUsers.Add(user.ID);
                return true;
            } else
            {
                return false;
            }
        }

        public void addUser(User user)
        {
            bool match = false;
            foreach (User u in users)
            {
                if (u.ID == user.ID)
                    match = true;
            }
            if (!match)
            {
                users.Add(user);
            }
        }
    }
}
