using System;
using System.IO;

namespace NoisEvader
{
    public class CachedLevelData
    {
        public string XmlPath { get; set; }

        public string Mp3Path => Path.Combine(Path.GetDirectoryName(XmlPath), Info.AudioFile);

        public string XmlHash { get; set; }

        public LevelInfo Info { get; set; } = new LevelInfo();

        public TimeSpan? AudioDuration { get; set; }

        public int Playcount { get; set; }

        public bool HeartGotten { get; set; }

        public LevelSettings Settings { get; set; } = default;

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public void LoadAudioDuration()
        {
            if (AudioDuration != null)
                return;

            try
            {
                if (File.Exists(Mp3Path))
                    AudioDuration = AudioPlayer.GetAudioDuration(Mp3Path);

                Database.UpdateAudioDuration(XmlHash, AudioDuration);
            }
            catch (InvalidOperationException iex)
            {
                logger.Error(iex, "Failed to load audio duration");
            }
            catch (NullReferenceException nrex)
            {
                logger.Error(nrex, "Failed to load audio duration");
            }
        }
    }
}