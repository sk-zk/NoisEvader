using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace NoisEvader
{
    public static class Config
    {
        public static GameSettings GameSettings { get; set; }

        private const string SettingsFile = "Settings.json";

        private static JsonSerializerOptions serializerOptions;

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static Config()
        {
            serializerOptions = new JsonSerializerOptions()
            {
                WriteIndented = true,
                AllowTrailingCommas = true,
            };
        }

        public static void LoadSettings()
        {
            if (!File.Exists(SettingsFile))
            {
                logger.Warn("No settings file found, loading defaults");
                GameSettings = new GameSettings();
                return;
            }

            logger.Info("Loading settings");
            try
            {
                GameSettings = LoadFromJson<GameSettings>(SettingsFile);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to load settings, using defaults");
                GameSettings = new GameSettings();
            }

            GameSettings.Volume = VolSanityCheck(GameSettings.Volume);
        }

        private static float VolSanityCheck(float vol) =>
            MathHelper.Clamp(vol, 0, 1);

        public static void SaveSettings()
        {
            logger.Info("Saving settings");
            try
            {
                SaveJson(GameSettings, SettingsFile);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to save settings");
            }
        }

        private static T LoadFromJson<T>(string path) =>
           JsonSerializer.Deserialize<T>(File.ReadAllText(path), serializerOptions);

        private static void SaveJson(object value, string path)
        {
            lock (value)
            {
                var json = JsonSerializer.Serialize(value, value.GetType(), serializerOptions);
                if (File.Exists(path))
                {
                    var old = path + ".old";
                    if (File.Exists(old))
                        File.Delete(old);
                    File.Move(path, old);
                    File.Delete(path);
                }
                File.WriteAllText(path, json);
            }
        }
    }

}