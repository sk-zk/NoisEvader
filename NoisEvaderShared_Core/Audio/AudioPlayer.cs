using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using CSCore.Codecs;
using CSCore.SoundOut;
using CSCore;
using CSCore.Streams;
using NoisEvader.Audio.SoundTouch;
using NoisEvader.Audio;

namespace NoisEvader
{
    public sealed class AudioPlayer : IDisposable
    {
        private ISoundOut soundOut;
        private ISampleSource sampleSource;
        private SoundTouchSource speedSource;
        private FadeInOut fadeSource;
        private VolumeSource masterVol;
        private IWaveSource waveSource;

        private static readonly AudioPlayer instance = new AudioPlayer();

        public static AudioPlayer Instance
        {
            get => instance;
        }

        /// <summary>
        /// The current position in ms.
        /// </summary>
        public double Position { get; private set; }

        /// <summary>
        /// The wave stream's reported position in ms.
        /// </summary>
        public double Playhead => sampleSource.GetPosition().TotalMilliseconds;

        /// <summary>
        /// The interpolated playhead position in percent of total length.
        /// </summary>
        public double PositionPercent => Position / TotalTime.TotalMilliseconds;

        /// <summary>
        /// Whether or not the waveOutDevice is currently playing.
        /// </summary>
        public bool IsPlaying => soundOut.PlaybackState == PlaybackState.Playing;

        /// <summary>
        /// The audio file to be played.
        /// </summary>
        public string CurrentFile { get; private set; }

        /// <summary>
        /// The total length of the track.
        /// </summary>
        public TimeSpan TotalTime { get; private set; } 

        private float playbackSpeed = 1f;
        public float PlaybackSpeed
        {
            get => playbackSpeed;
            set
            {
                playbackSpeed = value;
                SetSoundTouchSpeed();
            }
        }

        private float volume = 1;
        public float Volume
        {
            get => volume;
            set
            {
                volume = value;
                if (masterVol != null)
                    masterVol.Volume = volume;
            }
        }

        private Stopwatch audioPlaytime;
        private double prevElapsed;

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static AudioPlayer() { }

        private AudioPlayer() { }

        public static TimeSpan? GetAudioDuration(string songPath)
        {
            if (File.Exists(songPath))
                return GetCodec(songPath).GetLength();
            return null;
        }

        public static async Task<float[][]> GetWaveform(string songPath)
        {
            return await WaveformData.GetData(GetCodec(songPath));
        }

        /// <summary>
        /// Loads an audio file.
        /// </summary>
        public void Load(string songPath, bool useVarispeed = true, bool cache = true)
        {
            Stop();
            Dispose();

            CurrentFile = songPath;

            audioPlaytime = new Stopwatch();
            Position = 0;
            prevElapsed = 0;

            logger.Debug("Loading {file}", songPath);

            if (cache)
                sampleSource = new CachedSoundSource(GetCodec(songPath)).ToSampleSource();
            else
                sampleSource = GetCodec(songPath).ToSampleSource();

            TotalTime = sampleSource.GetLength();

            waveSource = sampleSource
                .AppendSource(x => new SoundTouchSource(x, 50), out speedSource)
                .AppendSource(x => new FadeInOut(x), out fadeSource)
                .AppendSource(x => new VolumeSource(x), out masterVol)
                .ToWaveSource();

            SetSoundTouchSpeed();
            fadeSource.FadeStrategy = new LinearFadeStrategy();
            masterVol.Volume = Config.GameSettings.Volume;

            #if WINDOWS
                soundOut = new WasapiOut();
            #else
                soundOut = new ALSoundOut();
            #endif
            soundOut.Initialize(waveSource);
        }

        private static IWaveSource GetCodec(string songPath)
        {
            #if WINDOWS
                return CodecFactory.Instance.GetCodec(songPath);
            #else
                return new CSCore.Ffmpeg.FfmpegDecoder(songPath);
            #endif
        }

        private void SetSoundTouchSpeed()
        {
            if (speedSource != null)
            {
                var tempo = (int)((playbackSpeed * 100) - 100);
                //speedSource.SetTempo(tempo);
                speedSource.SetRate(tempo);
            }
        }

        public void Play()
        {
            soundOut.Play();
            audioPlaytime?.Start();
        }

        public void Pause()
        {
            audioPlaytime?.Stop();
            soundOut.Pause();
        }

        public void Resume()
        {
            JumpTo(Position);
            soundOut.Resume();
            audioPlaytime?.Start();
        }

        public void Stop()
        {
            audioPlaytime?.Stop();
            try
            {
                soundOut?.Stop();
            } 
            catch (ObjectDisposedException)
            {
                // ignore
            }
        }

        /// <summary>
        /// Skips to the specificed position.
        /// </summary>
        /// <param name="newPos">New position in ms.</param>
        public void JumpTo(double newPos)
        {
            newPos = Math.Max(0, newPos);

            Position = newPos;
            sampleSource.SetPosition(TimeSpan.FromMilliseconds(Position));
            speedSource?.Seek();
        }

        /// <summary>
        /// Starts a fade in effect for the specified amount of time.
        /// </summary>
        /// <param name="duration">Duration in ms.</param>
        public void BeginFadeIn(double duration)
        {
            fadeSource?.FadeStrategy.StartFading(0, 1, duration);
        }

        /// <summary>
        /// Starts a fade out effect for the specified amount of time.
        /// </summary>
        /// <param name="duration">Duration in ms.</param>
        public void BeginFadeOut(double duration)
        {
            fadeSource?.FadeStrategy.StartFading(1, 0, duration);
        }

        public void Update(float gameSpeed)
        {
            if (!IsPlaying) return;

            var elapsed = audioPlaytime.Elapsed.TotalMilliseconds;
            var diff = elapsed - prevElapsed;
            Position += diff * gameSpeed;
            prevElapsed = elapsed;
        }

        public void Dispose()
        {
            try
            {
                soundOut?.Dispose();
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "sdfgh.");
            }
        }
    }
}