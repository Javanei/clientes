using System;
using System.Collections.Generic;
using System.Text;

namespace vaultsrv
{
    [Serializable]
    public class LoginInfo
    {
        private static string HTTPS_PREFIX = "https://";

        public string Username;
        public string Password;
        public string Server;
        public bool SSL = false;

        /// <summary>
        /// A value of 0 means the default port should be used
        /// </summary>
        public int Port = 0;

        public string Vault;

        /// <summary>
        /// The server location formatted the same way as Vault Explorer.
        /// Format:  [https://]servername[:port]
        /// </summary>
        public string ServerString
        {
            get
            {
                return (this.SSL ? "https://" : "") + this.Server + (this.Port != 0 ? ":" + this.Port.ToString() : "");
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serverStr">Formatted information about the server.
        /// Format:  [https://]servername[:port]</param>
        public LoginInfo(string username, string password, string serverStr, string vault)
        {
            this.Username = username;
            this.Password = password;
            this.Vault = vault;

            // parse the server string

            // check to see if an SSL connection is needed
            if (serverStr.StartsWith(HTTPS_PREFIX, StringComparison.CurrentCultureIgnoreCase))
            {
                this.SSL = true;
                serverStr = serverStr.Remove(0, HTTPS_PREFIX.Length);
            }
            else
                this.SSL = false;


            // check to see if a non-default port is needed
            int index = serverStr.LastIndexOf(':');
            if (index >= 0 && serverStr.Length > index + 2)
            {
                string portStr = serverStr.Substring(index + 1);
                serverStr = serverStr.Remove(index);
                Int32.TryParse(portStr, out this.Port);
            }
            else
                this.Port = 0;

            this.Server = serverStr;
        }
    }
}
