using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace NoisEvader
{
    /// <summary>
    /// Describes one shot (= one script tag).
    /// </summary>
    public class Shot
    {
        public double Time { get; set; }
        public int[] Enemies { get; set; }
        public BulletType BulletType { get; set; }
        public Aim Aim { get; set; }
        public double HomingLifespan { get; set; } // homing only
        public double Offset0 { get; set; }
        public int Amount0 { get; set; }
        public double Speed0 { get; set; }
        public double Angle0 { get; set; }
        public Color Color { get; set; }

        protected static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        public Shot() { }

        public Shot(XmlNode node, LevelColors colors)
        {
            Time = double.Parse(node.Attributes["time"].Value, Culture) * 1000;
            var enemiesStr = node.Attributes["enemies"].Value.Split(',');
            Enemies = Array.ConvertAll(enemiesStr, s => int.Parse(s, Culture) - 1);
            BulletType = ParseBulletType(node.Attributes["bulletType"].Value);
            Aim = ParseAim(node.Attributes["aim"].Value);
            Offset0 = double.Parse(node.Attributes["offset0"].Value, Culture) * MathEx.DegToRad;
            Speed0 = double.Parse(node.Attributes["speed0"].Value, Culture);
            Angle0 = double.Parse(node.Attributes["angle0"].Value, Culture) * MathEx.DegToRad;

            if (node.Attributes["amount0"] is null)
                Amount0 = int.Parse(node.Attributes["amount"].Value, Culture); // special case for streams
            else
                Amount0 = int.Parse(node.Attributes["amount0"].Value, Culture);

            if (BulletType == BulletType.Homing)
            {
                var lifespanAttrib = node.Attributes["lifespan"];
                HomingLifespan = lifespanAttrib is null
                    ? Homing.DefaultLifespan
                    : double.Parse(lifespanAttrib.Value, Culture) / 30 * 1000;
            }

            Color = GetBulletColor(BulletType, colors);
        }

        public virtual List<Wave> GetWaves()
        {
            var waves = new List<Wave>(1)
            {
                new Wave()
                {
                    Enemies = Enemies,
                    BulletType = BulletType,
                    Amount = Amount0,
                    Speed = Speed0,
                    Angle = Angle0,
                    Offset = Offset0,
                    Aim = Aim,
                    HomingLifespan = HomingLifespan,
                },
            };
            return waves;
        }

        /// <summary>
        /// Converts the value of a bulletType attribute to a BulletType.
        /// </summary>
        public static BulletType ParseBulletType(string input)
        {
            switch (input.ToLower(Culture))
            {
                case "nrm":
                    return BulletType.Normal;
                case "nrm2":
                    return BulletType.Normal2;
                case "homing":
                    return BulletType.Homing;
                case "bubble":
                    return BulletType.Bubble;
                case "heart":
                    return BulletType.Heart;
                case "hug":
                    return BulletType.Hug;
                default:
                    return BulletType.Error;
            }
        }

        /// <summary>
        /// Converts the value of an aim attribute to an Aim.
        /// </summary>
        private Aim ParseAim(string input)
        {
            switch (input.ToLower(Culture))
            {
                case "pl":
                    return Aim.Player;
                case "mid":
                default:
                    return Aim.Center;
            }
        }

        /// <summary>
        /// Returns the color of the bullet type in a shot.
        /// </summary>
        private Color GetBulletColor(BulletType type, LevelColors colors)
        {
            return type switch
            {
                BulletType.Normal => colors.Normal,
                BulletType.Normal2 => colors.Normal2,
                BulletType.Homing => colors.Homing,
                BulletType.Bubble => colors.Bubble,
                BulletType.Hug => colors.Hug,
                BulletType.Heart => Color.Red,
                _ => colors.Normal,
            };
        }
    }

    public class WaveShot : Shot
    {
        public int Rows { get; set; }
        public double Offset1 { get; set; }
        public int Amount1 { get; set; }
        public double Speed1 { get; set; }
        public double Angle1 { get; set; }

        public WaveShot() { }

        public WaveShot(XmlNode node, LevelColors colors) : base(node, colors)
        {
            Offset1 = double.Parse(node.Attributes["offset1"].Value, Culture) * MathEx.DegToRad; ;
            Amount1 = int.Parse(node.Attributes["amount1"].Value, Culture);
            Angle1 = double.Parse(node.Attributes["angle1"].Value, Culture) * MathEx.DegToRad;
            Speed1 = double.Parse(node.Attributes["speed1"].Value, Culture);
            Rows = int.Parse(node.Attributes["rows"].Value, Culture);
        }

        public override List<Wave> GetWaves()
        {
            var waves = new List<Wave>(Enemies.Length * Rows);

            for (int i = 0; i < Rows; i++)
            {
                var deltaX = Rows - 1;
                var amount = Amount0 + (i * ((float)(Amount1 - Amount0) / deltaX));
                var speed = Speed0 + (i * ((Speed1 - Speed0) / deltaX));
                var angle = Angle0 + (i * ((Angle1 - Angle0) / deltaX));
                var offset = Offset0 + (i * ((Offset1 - Offset0) / deltaX));
                waves.Add(new Wave()
                {
                    Enemies = Enemies,
                    BulletType = BulletType,
                    Amount = (int)amount,
                    Speed = speed,
                    Angle = angle,
                    Offset = offset,
                    Aim = Aim,
                    HomingLifespan = HomingLifespan,
                });
            }

            return waves;
        }
    }

    public class StreamShot : Shot
    {
        public double Offset1 { get; set; }
        public double Speed1 { get; set; }
        public double Angle1 { get; set; }
        public double Duration { get; set; }
        public double TimeSinceLastShot { get; set; } // used ingame only
        public const double FireFrequency = 166.6666666f;

        public StreamShot() { }

        public StreamShot(XmlNode node, LevelColors colors) : base(node, colors)
        {
            Angle1 = double.Parse(node.Attributes["angle1"].Value, Culture) * MathEx.DegToRad;
            Speed1 = double.Parse(node.Attributes["speed1"].Value, Culture);
            Offset1 = double.Parse(node.Attributes["offset1"].Value, Culture) * MathEx.DegToRad;
            Duration = double.Parse(node.Attributes["duration"].Value, Culture) * 1000;
        }
    }

    public class BurstShot : Shot
    {
        public double Speed1 { get; set; }

        public BurstShot() { }

        private static int seed;
        private static Random random;

        public static int Seed => seed;

        static BurstShot()
        {
            seed = Environment.TickCount;
            random = new Random(seed);
        }

        public static void InitRandom(int newSeed)
        {
            seed = newSeed;
            random = new Random(seed);
        }

        public BurstShot(XmlNode node, LevelColors colors) : base(node, colors)
        {
            Speed1 = double.Parse(node.Attributes["speed1"].Value, Culture);
        }

        public override List<Wave> GetWaves()
        {
            var waves = new List<Wave>(Enemies.Length);

            var angleMax = Angle0 / 2;
            var angleMin = -angleMax;

            for (int i = 0; i < Amount0; i++)
            {
                var speed = random.Next(Speed0, Speed1);
                var angle = random.Next(angleMin, angleMax);
                waves.Add(new Wave()
                {
                    Enemies = Enemies,
                    BulletType = BulletType,
                    Amount = 1,
                    Speed = speed,
                    Offset = Offset0 + angle,
                    Aim = Aim,
                    HomingLifespan = HomingLifespan,
                });
            }

            return waves;
        }
    }
}
