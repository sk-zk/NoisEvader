using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LilyPath;
using Microsoft.Xna.Framework;

namespace NoisEvader
{
    public class LiveSpawner : Spawner
    {
        // TODO: Unfuck TransIn

        public float ActiveZone { get; set; } = MathHelper.ToRadians(360);
        public float TransitionZone { get; set; } = 0;

        private SpawnerTransition transOut;
        private SpawnerTransition transIn;

        public LiveSpawner() : base() { }

        public LiveSpawner(float angle, ArenaCircle arena)
            : base(angle, arena) { }

        public override void Update(LevelTime levelTime, float spawnerSpeed)
        {
            base.Update(levelTime, spawnerSpeed);

            if (transOut != null)
            {
                const double epsilon = 0.05; // this better be large enough
                if (transOut.Radius <= epsilon)
                    transOut = null;
                else
                    transOut.Update(levelTime, spawnerSpeed);
            }

            if (transIn != null)
            {
                const double epsilon = 0.01;
                if (transIn.Radius >= SpawnerRadius - epsilon)
                   transIn = null;
                else
                  transIn.Update(levelTime, spawnerSpeed);
            }
        }

        protected override void UpdateAngle(float spawnerSpeed)
        {
            base.UpdateAngle(spawnerSpeed);

            // Check if it's time to transition
            UpdateTransition(spawnerSpeed);
        }

        private void UpdateTransition(float spawnerSpeed)
        {
            if (ActiveZone == MathHelper.ToRadians(360))
                return;

            var minArc = MathHelper.ToRadians(-90) - (ActiveZone / 2);
            var maxArc = MathHelper.ToRadians(-90) + (ActiveZone / 2);
            var prevAngle = Angle;
            
            // if the spawner is about to switch to the other side of the screen,
            // start the fade-in animation
            /*if (Angle > maxArc - TransitionZone && Angle <= maxArc
                && transIn is null)
            {
                transIn = CreateTransIn(prevAngle, spawnerSpeed);
            }
            else if (Angle < minArc + TransitionZone && Angle >= minArc
                && transIn is null)
            {
                transIn = CreateTransIn(prevAngle, spawnerSpeed);
            }*/

            // if it's time to switch, move the spawner and start the fade-out animation
            if (Angle > maxArc)
            {
                Angle -= ActiveZone;
                transOut = CreateTransOut(prevAngle, spawnerSpeed);
            }
            else if (Angle < minArc)
            {
                Angle += ActiveZone;
                transOut = CreateTransOut(prevAngle, spawnerSpeed);
            }
        }

        private SpawnerTransition CreateTransOut(float prevAngle, float spawnerSpeed)
        {
            var startAngle = prevAngle;
            var endAngle = prevAngle;
            if (spawnerSpeed < 0)
                endAngle -= TransitionZone;
            else
                endAngle += TransitionZone;

            return new SpawnerTransition(startAngle, Arena, startAngle, endAngle, Radius, 0)
            {
                FillColor = FillColor,
                BorderColor = BorderColor,
                LineThickness = LineThickness,
                Subdivisions = Subdivisions,
            };
        }

        private SpawnerTransition CreateTransIn(float prevAngle, float spawnerSpeed)
        {
            var endAngle = prevAngle;
            var startAngle = prevAngle;
            if (spawnerSpeed > 0)
            {
                startAngle += ActiveZone - TransitionZone;
                endAngle += ActiveZone;
            }
            else
            {
                startAngle -= ActiveZone + TransitionZone;
                endAngle -= ActiveZone;
            }

            return new SpawnerTransition(startAngle, Arena, startAngle, endAngle, 0, Radius)
            {
                FillColor = FillColor,
                BorderColor = BorderColor,
                LineThickness = LineThickness,
                Subdivisions = Subdivisions,
            };
        }

        public override void Draw(DrawBatch drawBatch)
        {
            //transIn?.Draw(drawBatch);
            transOut?.Draw(drawBatch);
            base.Draw(drawBatch);
        }

    }
}
