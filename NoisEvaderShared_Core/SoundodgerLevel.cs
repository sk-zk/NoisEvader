using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace NoisEvader
{
    public class SoundodgerLevel
    {
        public string XmlPath { get; set; }

        public string XmlFile => Path.GetFileName(XmlPath);

        public string AudioPath => Path.Combine(Path.GetDirectoryName(XmlPath), Info.AudioFile);

        public LevelInfo Info { get; set; }

        public LevelScript Script { get; set; }

        private TimeSpan? audioDuration;
        public TimeSpan AudioDuration
        {
            get => audioDuration ?? TimeSpan.Zero;
            private set => audioDuration = value;
        }

        public bool UsesCenterSpawnerGlitch { get; set; }

        public string XmlHash { get; private set; }

        protected readonly CultureInfo culture = CultureInfo.InvariantCulture;

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public SoundodgerLevel() { }

        public SoundodgerLevel(string xmlPath)
        {
            XmlPath = xmlPath;
            LoadLevel(xmlPath);
        }

        /// <summary>
        /// Loads a Soundodger+ level.
        /// </summary>
        private void LoadLevel(string xmlPath)
        {
            var xml = LoadXml(xmlPath);
            TryLoadLevelFromString(xml, xmlPath, false);
        }

        private string LoadXml(string xmlPath)
        {
            string xml;
            using (var fs = new FileStream(xmlPath, FileMode.Open))
            {
                XmlHash = BitConverter.ToString(SHA1.Create().ComputeHash(fs)).Replace("-", "").ToLower();

                fs.Position = 0;
                using var sr = new StreamReader(fs);
                xml = sr.ReadToEnd();
            }

            return xml;
        }

        public static CachedLevelData LoadInfoOnly(string xmlPath)
        {
            var lvl = new SoundodgerLevel();
            var xml = lvl.LoadXml(xmlPath);

            var data = Database.GetLevelData(lvl.XmlHash);
            if (data != null && data.Info != null)
            {
                data.XmlPath = xmlPath;
                return data;
            }

            lvl.TryLoadLevelFromString(xml, xmlPath, true);
            var cld = new CachedLevelData()
            {
                XmlPath = xmlPath,
                XmlHash = lvl.XmlHash,
                Info = lvl.Info,
                AudioDuration = lvl.audioDuration,
            };
            Database.InsertLevelData(cld);
            return cld;
        }

        private void TryLoadLevelFromString(string xml, string xmlPath, bool infoOnly)
        {
            try
            {
                LoadLevelFromString(xml, xmlPath, infoOnly);
            }
            catch (XmlException xmlEx)
            {
                // Double hyphens in XML comments are against the spec 
                // and make the .NET XML parser a n g e r y.
                // Flash apparently doesn't mind it though so some dinguses
                // put them in their levels.
                // If that happens, we need to filter them out here.
                if (xmlEx.Message.Contains("--"))
                {
                    xml = FixXmlComments(xml);
                    TryLoadLevelFromString(xml, xmlPath, infoOnly);
                    logger.Warn(xmlEx, "Level {xmlpath} contains double hyphens in comments; corrected", xmlPath);
                }
                // Another thing the Flash parser is totally OK with
                // is attributes without spaces between them, e.g.:
                // ` lifespan="0"amount1="1" `
                // This fix was brought to you by Gonna Fear Now
                else if (xmlEx.Message.Contains("Expecting whitespace"))
                {
                    xml = FixAttributeWhitespace(xml);
                    TryLoadLevelFromString(xml, xmlPath, infoOnly);
                    logger.Warn(xmlEx, "Level {xmlpath} contains attributes without spaces separating them; corrected", xmlPath);
                }
                else
                {
                    throw;
                }
            }
            catch (ArgumentException)
            {
                throw;
            }
        }

        private string FixAttributeWhitespace(string xml)
        {
            const string pattern = @"""()\w*?=";
            var replaced = Regex.Replace(xml, pattern,
                new MatchEvaluator(match => match.Value.Insert(1, " ")));
            return replaced;
        }

        private void LoadLevelFromString(string xml, string xmlPath, bool infoOnly)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            if (doc.DocumentElement.Name != "Song")
                throw new ArgumentException("Document root is not <Song>.");

            ParseInfo(xmlPath, doc);

            if (!infoOnly)
                ParseScript(doc);
        }

        private void ParseInfo(string xmlPath, XmlDocument doc)
        {
            var info = doc.DocumentElement.SelectSingleNode("//Info");
            var attribs = new Dictionary<string, string>();
            foreach (XmlAttribute attrib in info.Attributes)
                attribs.Add(attrib.Name, attrib.Value);

            Info = LevelInfo.ParseInfo(attribs, Path.GetDirectoryName(xmlPath));
        }

        private string FixXmlComments(string xml)
        {
            // via https://stackoverflow.com/a/28119330
            const string pattern = "(?<!<!)--+(?!-?>)(?=(?:(?!-->).)*-->)";
            string replaced = Regex.Replace(xml, pattern, "~~", RegexOptions.Singleline);
            return replaced;
        }

        /// <summary>
        /// Parses the script of the level (bullets, spinrate, timewarp).
        /// </summary>
        private void ParseScript(XmlDocument doc)
        {
            Script = new LevelScript();

            // Time warp
            var timeWarpNodes = doc.DocumentElement.SelectNodes("//Script[@warpType='timeWarp']");
            var timeWarpInfo = new List<TsVal<float>>(timeWarpNodes.Count);
            foreach (XmlNode timeWarpNode in timeWarpNodes)
            {
                var time = float.Parse(timeWarpNode.Attributes["time"].Value, culture) * 1000;
                var val = float.Parse(timeWarpNode.Attributes["val"].Value, culture);
                timeWarpInfo.Add(new TsVal<float>(time, val));
            }
            Script.TimeWarpNodes = timeWarpInfo;

            // Spin rate
            var spinRateNodes = doc.DocumentElement.SelectNodes("//Script[@warpType='spinRate']");
            var spinRateInfo = new List<TsVal<float>>(spinRateNodes.Count);
            foreach (XmlNode spinRateNode in spinRateNodes)
            {
                var time = float.Parse(spinRateNode.Attributes["time"].Value, culture) * 1000;
                var val = float.Parse(spinRateNode.Attributes["val"].Value, culture);
                spinRateInfo.Add(new TsVal<float>(time, val));
            }
            Script.SpinRateNodes = spinRateInfo;
            // Check if the center spawner glitch is used in this level.
            // I don't know the exact conditions that trigger it but it's something like this
            if (float.IsNaN(Script.SpinRateNodes[1].Time))
            {
                UsesCenterSpawnerGlitch = true;
                Script.SpinRateNodes.RemoveAt(1);
            }

            // Bullets
            var shotNodes = doc.DocumentElement.SelectNodes("//Script[@shotType]");
            var shots = new List<Shot>(shotNodes.Count);
            foreach (XmlNode shotNode in shotNodes)
            {
                var shotType = shotNode.Attributes["shotType"].Value;
                Shot shot = shotType switch
                {
                    "wave" => new WaveShot(shotNode, Info.Colors),
                    "stream" => new StreamShot(shotNode, Info.Colors),
                    "burst" => new BurstShot(shotNode, Info.Colors),
                    "normal" => new Shot(shotNode, Info.Colors),
                    _ => new Shot(shotNode, Info.Colors),
                };
                shots.Add(shot);
            }

            shots.OrderBy(x => x.Time);
            Script.Shots = shots;
        }
    }
}