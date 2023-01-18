using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KAMI.Core.Utilities
{
    public class ConfigManager<TConfig> where TConfig : IConfig
    {
        internal TConfig Config { get; private set; } = default;
        private string FileName { get; init; }

        internal ConfigManager(string fileName)
        {
            FileName = fileName;
        }

        internal void ReloadConfig()
        {
            if (!File.Exists(FileName))
            {
                Config = (TConfig)TConfig.GetDefaultConfig();
                WriteConfig();
                return;
            }
            string json = File.ReadAllText(FileName);
            var config = JsonSerializer.Deserialize<TConfig>(json, SerializerOptions);
            if (config == null)
            {
                throw new Exception("Failed to load config");
            }
            Config = config;
        }

        internal void WriteConfig()
        {
            string json = JsonSerializer.Serialize(Config, SerializerOptions);
            File.WriteAllText(FileName, json);
            ReloadConfig();
        }

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            Converters = {
                new JsonStringEnumConverter()
            }
        };
    }
}
