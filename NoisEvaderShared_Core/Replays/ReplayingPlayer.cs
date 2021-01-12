using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NoisEvader.Replays
{
    class ReplayingPlayer : Player
    {
        private Replay replay;

        TimeIter<TsVal<Vector2>> positionIterator;
        TimeIter<float> heartIterator;

        public ReplayingPlayer(Replay replay) : base()
        {
            this.replay = replay;
            positionIterator = new TimeIter<TsVal<Vector2>>(
                replay.PlayerPositions, x => x.Time, OnNewPosition);
            heartIterator = new TimeIter<float>(
                replay.Hearts, x => x, OnNewHeart);
        }

        public override void Update(LevelTime levelTime, CircleSprite arenaCircle, ArenaCamera camera,
            bool confineCursorToScreen)
        {
            UpdatePosition(levelTime, arenaCircle, camera, confineCursorToScreen);
            heartIterator.Update(levelTime);
            UpdateInvincibility(levelTime);
        }

        protected override void UpdatePosition(LevelTime levelTime, CircleSprite arenaCircle,
            ArenaCamera camera, bool confineCursorToScreen)
        {
            positionIterator.Update(levelTime);
        }

        private void OnNewHeart(TimeIterActionArgs<float> args)
        {
            HasHeart = true;
        }

        private void OnNewPosition(TimeIterActionArgs<TsVal<Vector2>> args)
        {
            prevCenter = Center;
            Center = args.List[args.Index].Value;
            if (args.Index > 0)
            {
                var deltaT = args.List[args.Index].Time - args.List[args.Index - 1].Time;
                if (deltaT != 0)
                    velocityPerMs = (prevCenter - base.Center) / deltaT;
                else
                    velocityPerMs = Vector2.Zero;
            }
        }
    }
}
