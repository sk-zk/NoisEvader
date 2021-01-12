using Ionic.Zlib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace NoisEvader.Replays
{
    public class Replay
    {
        public const string ReplaysDir = "Replays";
        public const string ReplayExtension = ".rpyz";

        public const ushort ReplayVersion = 1;

        public const double TickRate = 1000.0 / 60;

        public string XmlHash { get; set; }

        public DateTime Timestamp { get; set; }

        public Mod Mod { get; set; }

        public float FinalScorePercent { get; set; }

        public int BurstSeed { get; set; }

        public List<TsVal<Vector2>> PlayerPositions { get; private set; }
            = new List<TsVal<Vector2>>();

        public List<TsVal<InvincibilityType>> Hits { get; private set; }
            = new List<TsVal<InvincibilityType>>();

        public List<float> Hearts { get; private set; }
            = new List<float>();

        public List<TsVal<bool>> SlomoToggles { get; private set; }
            = new List<TsVal<bool>>();

        public void Clear()
        {
            PlayerPositions.Clear();
            Hits.Clear();
            Hearts.Clear();
        }

        public static Replay Open(string path)
        {
            var compressed = File.ReadAllBytes(path);
            using var ms = new MemoryStream(ZlibStream.UncompressBuffer(compressed));
            using var r = new BinaryReader(ms);

            var repl = new Replay();

            var version = r.ReadUInt16();
            if (version != ReplayVersion)
                throw new InvalidDataException("Unsupported file version");

            repl.XmlHash = r.ReadString();
            repl.Timestamp = DateTimeOffset.FromUnixTimeSeconds(r.ReadInt64()).UtcDateTime;
            repl.Mod = ReadMod(r);
            repl.FinalScorePercent = r.ReadSingle();
            repl.BurstSeed = r.ReadInt32();

            repl.PlayerPositions = ReadList<TsVal<Vector2>>(r, (r, pos) =>
            {
                pos.Time = r.ReadSingle();
                pos.Value = new Vector2(r.ReadSingle(), r.ReadSingle());
            });
            repl.PlayerPositions = repl.PlayerPositions.OrderBy(x => x.Time).ToList();

            repl.Hits = ReadList<TsVal<InvincibilityType>>(r, (r, hit) =>
            {
                hit.Time = r.ReadSingle();
                hit.Value = (InvincibilityType)r.ReadInt32();
            });
            repl.Hits = repl.Hits.OrderBy(x => x.Time).ToList();

            repl.Hearts = ReadList(r, (r) => r.ReadSingle());
            repl.Hearts.Sort();

            repl.SlomoToggles = ReadList<TsVal<bool>>(r, (r, enabled) =>
            {
                enabled.Time = r.ReadSingle();
                enabled.Value = r.ReadBoolean();
            });
            repl.SlomoToggles = repl.SlomoToggles.OrderBy(x => x.Time).ToList();

            return repl;
        }

        public void Save(string path)
        {
            using var ms = new MemoryStream();
            using (var w = new BinaryWriter(ms))
            {
                w.Write(ReplayVersion);
                w.Write(XmlHash);
                w.Write(Timestamp.ToUnix());
                WriteMod(w);
                w.Write(FinalScorePercent);
                w.Write(BurstSeed);

                w.Write(PlayerPositions.Count);
                foreach (var pos in PlayerPositions)
                {
                    w.Write(pos.Time);
                    w.Write(pos.Value.X);
                    w.Write(pos.Value.Y);
                }

                w.Write(Hits.Count);
                foreach (var hit in Hits)
                {
                    w.Write(hit.Time);
                    w.Write((int)hit.Value);
                }

                w.Write(Hearts.Count);
                foreach (var heart in Hearts)
                {
                    w.Write(heart);
                }

                w.Write(SlomoToggles.Count);
                foreach (var slomo in SlomoToggles)
                {
                    w.Write(slomo.Time);
                    w.Write(slomo.Value);
                }

            }

            var compressed = ZlibStream.CompressBuffer(ms.ToArray());
            Directory.CreateDirectory(ReplaysDir);
            File.WriteAllBytes(path, compressed);
        }

        public static List<T> ReadList<T>(BinaryReader r, Action<BinaryReader, T> readOneItem) 
            where T : new() =>
            ReadListBase(r, (r) =>
            {
                var item = new T();
                readOneItem.Invoke(r, item);
                return item;
            });

        public static List<T> ReadList<T>(BinaryReader r, Func<BinaryReader, T> readOneItem) => 
            ReadListBase(r, readOneItem);

        public static List<T> ReadListBase<T>(BinaryReader r, Func<BinaryReader, T> iteratorBody)
        {
            var count = r.ReadInt32();
            var list = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                var item = iteratorBody.Invoke(r);
                list.Add(item);
            }
            return list;
        }

        private static Mod ReadMod(BinaryReader r)
        {
            var mod = new Mod
            {
                Flags = (ModFlags)r.ReadInt32(),
                GameSpeed = r.ReadSingle(),
            };
            var tr = r.ReadSingle();
            mod.TickRate = float.IsNaN(tr)
                ? (float?)null
                : tr;
            return mod;
        }

        private void WriteMod(BinaryWriter w)
        {
            w.Write((int)Mod.Flags);
            w.Write(Mod.GameSpeed);
            w.Write(Mod.TickRate ?? float.NaN);
        }
    }
}
