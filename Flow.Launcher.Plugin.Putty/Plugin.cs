using Droplex;
using Flow.Launcher.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Flow.Launcher.Plugin.Putty
{
    public class PuttyPlugin : IPlugin, ISettingProvider
    {
        /// <summary>
        /// A refernce to the current PluginInitContext
        /// </summary>
        private PluginInitContext context;

        private Settings settings;

        /// <summary>
        /// A reference to the Putty PuttySessionService
        /// </summary>
        public IPuttySessionService PuttySessionService { get; set; }

        public PuttyPlugin()
        {
            PuttySessionService = new PuttySessionService();
        }

        /// <summary>
        /// Initializes the Putty plugin
        /// </summary>
        /// <param name="context"></param>
        public void Init(PluginInitContext context)
        {
            this.context = context;

            var settingsFolderLocation = 
                Path.Combine(
                    Directory.GetParent(
                        Directory.GetParent(context.CurrentPluginMetadata.PluginDirectory).FullName)
                    .FullName, 
                    "Settings","Plugins","Flow.Launcher.Plugin.Putty");

            var settingsFileLocation = Path.Combine(settingsFolderLocation, "Settings.json");

            if (!Directory.Exists(settingsFolderLocation))
            {
                Directory.CreateDirectory(settingsFolderLocation);

                settings = new Settings
                {
                    SettingsFileLocation = settingsFileLocation
                };

                settings.Save();
            }
            else
            {
                settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(settingsFileLocation));
                settings.SettingsFileLocation = settingsFileLocation;
            }

            settings.OnSettingsChanged = (s) => settings.Save();

            if (string.IsNullOrEmpty(settings.PuttyPath))
            {
                if (MessageBox.Show("Flow did not find a Putty path in settings, " +
                    "would you like to download Putty? " +
                    Environment.NewLine + Environment.NewLine +
                    "Click no if you already have Putty, " +
                    "and you will be prompted to select the path to the executable",
                    string.Empty, MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    // Solves single thread apartment (STA) mode requirement error when using OpenFileDialog
                    Thread t = new Thread(() =>
                    {
                        var dlg = new OpenFileDialog
                        {
                            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
                        };

                        var result = dlg.ShowDialog();
                        if (result == DialogResult.OK && !string.IsNullOrEmpty(dlg.FileName))
                            settings.PuttyPath = dlg.FileName;

                    });

                    // Run your code from a thread that joins the STA Thread
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                    t.Join();
                }
                else
                {
                    SetupPutty();
                }

                settings.Save();
            }
        }

        private void SetupPutty()
        {
            var filePath = Path.Combine(context.CurrentPluginMetadata.PluginDirectory, "putty.exe");
            // Save to the plugin directory instead of the user data settings directory 
            // because it's a portable version and so it can removed along with the plugin.
            Downloader
                .Get("https://the.earth.li/~sgtatham/putty/latest/w64/putty.exe", filePath)
                .Wait();

            settings.PuttyPath = filePath;
        }

        /// <summary>
        /// Returns a filtered Putty sessions list based on the given Query.
        /// If no Query.ActionParameter is provided only the default Putty item is returned.
        /// </summary>
        /// <param name="query">A Query that contains an ActionParameter to filter the Putty session list</param>
        /// <returns>The filtered Putty session list</returns>
        public List<Result> Query(Query query)
        {
            var results = new List<Result> { };
            if (settings.AddPuttyExeToResults)
            {
                results.Add(CreateResult());
            }
            var querySearch = query.Search;

            if (string.IsNullOrEmpty(querySearch))
            {
                if (settings.AddPuttyExeToResults)
                {
                    return results;
                } 
                else 
                {
                    querySearch = string.Empty;
                }
            }

            var puttySessions = PuttySessionService.GetAll().Where(session => session.Identifier.ToLowerInvariant().Contains(querySearch.ToLowerInvariant()));
            foreach (var puttySession in puttySessions)
            {
                results.Add(CreateResult(puttySession.Identifier, puttySession.ToString()));
            }

            return results;
        }

        /// <summary>
        /// Creates a new Result item
        /// </summary>
        /// <param name="title"></param>
        /// <param name="subTitle"></param>
        /// <returns>A Result object containing the PuttySession identifier and its connection string</returns>
        private Result CreateResult(string title = "putty.exe", string subTitle = "Launch Clean Putty")
        {
            return new Result
            {
                Title = title,
                SubTitle = subTitle,
                IcoPath = "icon.png",
                Action = context => LaunchPuttySession(title),
            };
        }

        /// <summary>
        /// Launches a new Putty session
        /// </summary>
        /// <param name="sessionIdentifier">The session identifier</param>
        /// <returns>If launching Putty succeeded</returns>
        private bool LaunchPuttySession(string sessionIdentifier)
        {
            try
            {
				string PuttyPath = "putty.exe";
				if(!string.IsNullOrEmpty(settings.PuttyPath))
					PuttyPath = settings.PuttyPath;
                var p = new Process { StartInfo = { FileName = PuttyPath } };

                // Optionally pass the session identifier
                if (!string.IsNullOrEmpty(sessionIdentifier))
                {
                    p.StartInfo.Arguments = "-load \"" + sessionIdentifier + "\"";
                }

                if (settings.AlwaysStartsSessionMaximized)
                {
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                }

                p.Start();

                return true;
            }
            catch (Exception ex)
            {
                // Report the exception to the user. No further actions required
                context.API.ShowMsg("Putty Error: " + sessionIdentifier + " (" + settings.PuttyPath + ") ", ex.Message, "");

                return false;
            }
        }

        public System.Windows.Controls.Control CreateSettingPanel()
        {
            return new PuttySettings(settings);
        }
    }
}