using System;
using System.IO;
using System.Text.Json;

namespace Flow.Launcher.Plugin.Putty
{
    public class Settings
    {
        internal string SettingsFileLocation;
        public bool AddPuttyExeToResults { get; set; } = true;
        public bool AlwaysStartsSessionMaximized { get; set; } = false;
		public string PuttyPath { get; set; }

        internal Action<Settings> OnSettingsChanged { get; set; }

        internal void Save()
        {
            File.WriteAllText(SettingsFileLocation, JsonSerializer.Serialize(this));
        }
    }
}