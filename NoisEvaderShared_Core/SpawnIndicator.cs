using LilyPath;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoisEvader
{
    // TODO: Rewrite this ancient mess

    class SpawnIndicator
    {
        private CircleSprite circle;
        private List<IndicatorInfo> active = new List<IndicatorInfo>();
        private List<IndicatorInfo> indicators = new List<IndicatorInfo>();
        private float maxIndicatorPeriod = 5000f;

        private readonly Spawner parent;

        public SpawnIndicator(Spawner parent)
        {
            this.parent = parent;
            circle = new CircleSprite()
            {
                Radius = 0f,
                LineThickness = 0f,
                Subdivisions = parent.Subdivisions,
            };
        }

        public void StartNextIndicator(float current)
        {
            if (indicators.Exists(x =>
                (x.ShotLaunchTime > current) &&
                (x.ShotLaunchTime < current + maxIndicatorPeriod)))
            {
                var indicator = indicators.First(x => x.ShotLaunchTime > current);
                indicator.Tweener = new Tweener(x => x);
                indicator.Tweener.Animate(0, 1, (float)(indicator.ShotLaunchTime - current));
                active.Add(indicator);
            }
        }

        public void AddToIndicatorList(double shotLaunchTime, Color color)
        {
            indicators.Add(new IndicatorInfo(shotLaunchTime, color));
        }

        public void Update(LevelTime levelTime)
        {
            if (active.Count != 0)
            {
                var indicator = active[0];

                indicator.Tweener.Update((float)levelTime.ElapsedMs);
                circle.FillColor = indicator.Color;

                if (indicator.Tweener.Progress < 0.99999f)
                {
                    circle.Radius = parent.InnerRadius * indicator.Tweener.CurrentValue;
                }
                else
                {
                    active[0].Tweener = null;
                    active.RemoveAt(0);
                    circle.Radius = 0f;
                    StartNextIndicator((float)levelTime.SongPosition);
                }
            }
            else
            {
                StartNextIndicator((float)levelTime.SongPosition);
            }

            circle.Center = parent.Center;
        }

        public void Reset()
        {
            active.Clear();
        }

        public void Draw(DrawBatch batch)
        {
            if (active.Count > 0)
                circle.Draw(batch);
        }
    }

    public class IndicatorInfo
    {
        public double ShotLaunchTime { get; set; }
        public Color Color { get; set; }
        public Tweener Tweener { get; set; }

        public IndicatorInfo(double shotLaunchTime, Color color)
        {
            ShotLaunchTime = shotLaunchTime;
            Color = color;
        }
    }

}
