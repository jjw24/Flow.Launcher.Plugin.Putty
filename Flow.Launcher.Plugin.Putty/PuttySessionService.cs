namespace Flow.Launcher.Plugin.Putty
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;

    public class PuttySessionService : IPuttySessionService
    {
        /// <summary>
        /// Returns a List of all Putty Sessions
        /// </summary>
        /// <returns>A List of all Putty Sessions</returns>
        public IEnumerable<PuttySession> GetAll()
        {
            var results = new List<PuttySession>();

            using (var root = Registry.CurrentUser.OpenSubKey("Software\\SimonTatham\\PuTTY\\Sessions"))
            {
                if (root == null)
                {
                    return results;
                }

                foreach (var subKey in root.GetSubKeyNames())
                {
                    using (var puttySessionSubKey = root.OpenSubKey(subKey))
                    {
                        if (puttySessionSubKey == null)
                        {
                            continue;
                        }
                        try
                        {
                            var session = new PuttySession
                            {
                                Identifier = Uri.UnescapeDataString(subKey),
                                Protocol = puttySessionSubKey.GetValue("Protocol").ToString(),
                                Username = puttySessionSubKey.GetValue("UserName").ToString(),
                                Hostname = puttySessionSubKey.GetValue("HostName").ToString(),
                            };
                            if (session.Protocol == "serial")
                            {
                                session.Hostname = $"{puttySessionSubKey.GetValue("SerialLine")}?baud={puttySessionSubKey.GetValue("SerialSpeed")}";
                                session.Username = string.Empty; // ensure ToString doesn't append username
                            }
                            results.Add(session);
                        }
                        catch (Exception)
                        {
                            // If there is any exception related to the registry access, just do nothing for that key, but don't let the whole results fails.
                        }
                    }
                }
            }

            return results;
        }
    }
}