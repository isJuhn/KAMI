using System;
using System.IO;
using System.Text.Json;

namespace KAMI.Core.Utilities
{
    public class ConfigManager<TConfig> where TConfig : IConfig
    {
        public TConfig Config { get; private set; } = default;
        public string FileName { get; init; }

        public ConfigManager(string fileName)
        {
            FileName = fileName;
            ReloadConfig();
        }

        public void ReloadConfig()
        {
            if (!File.Exists(FileName))
            {
                Config = (TConfig)TConfig.GetDefaultConfig();
                WriteConfig();
                return;
            }
            string json = File.ReadAllText(FileName);
            var config = JsonSerializer.Deserialize<TConfig>(json);
            if (config == null)
            {
                throw new Exception("Failed to load config");
            }
            Config = config;
        }

        public void WriteConfig()
        {
            string json = JsonSerializer.Serialize(Config);
            File.WriteAllText(FileName, json);
            ReloadConfig();
        }
    }
}
