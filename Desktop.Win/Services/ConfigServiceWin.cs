using iControl.Desktop.Core.Interfaces;
using iControl.Shared.Models;
using iControl.Shared.Utilities;
using System;
using System.IO;
using System.Text.Json;

namespace iControl.Desktop.Win.Services
{

    public class ConfigServiceWin : IConfigService
    {
        private static readonly string _configFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "iControl");
        private static readonly string _configFile = Path.Combine(_configFolder, "Config.json");

        public DesktopAppConfig GetConfig()
        {
            var config = new DesktopAppConfig();

            if (string.IsNullOrWhiteSpace(config.Host) &&
                File.Exists(_configFile))
            {
                try
                {
                    config = JsonSerializer.Deserialize<DesktopAppConfig>(File.ReadAllText(_configFile));
                }
                catch (Exception ex)
                {
                    Logger.Write(ex);
                }
            }

            return config;
        }

        public void Save(DesktopAppConfig config)
        {
            try
            {
                Directory.CreateDirectory(_configFolder);
                File.WriteAllText(_configFile, JsonSerializer.Serialize(config));
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }
    }
}
