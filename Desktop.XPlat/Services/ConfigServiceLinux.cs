using iControl.Desktop.Core.Interfaces;
using iControl.Shared.Models;
using iControl.Shared.Utilities;
using System;
using System.IO;
using System.Text.Json;

namespace iControl.Desktop.XPlat.Services
{
    public class ConfigServiceLinux : IConfigService
    {
        private static string ConfigFile => Path.Combine(ConfigFolder, "Config.json");
        private static string ConfigFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "iControl.json");

        public DesktopAppConfig GetConfig()
        {
            var config = new DesktopAppConfig();

            if (string.IsNullOrWhiteSpace(config.Host) &&
                File.Exists(ConfigFile))
            {
                try
                {
                    config = JsonSerializer.Deserialize<DesktopAppConfig>(File.ReadAllText(ConfigFile));
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
                Directory.CreateDirectory(ConfigFolder);
                File.WriteAllText(ConfigFile, JsonSerializer.Serialize(config));
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }
    }
}
