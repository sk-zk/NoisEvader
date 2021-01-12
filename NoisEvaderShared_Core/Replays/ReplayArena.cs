using LilyPath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NoisEvader.Replays
{
    public class ReplayArena : Arena
    {
        public Replay Replay { get; private set; }

        private TimeIter<TsVal<InvincibilityType>> hitIterator;
        private TimeIter<TsVal<bool>> slomoIterator;

        private SimpleText watchingReplayText;

        public ReplayArena(InputHelper inputHelper) : base(inputHelper)
        {
        }

        public void LoadReplayAndStart(SoundodgerLevel level, Replay replay,
            LevelSettings settings)
        {
            player = new ReplayingPlayer(replay);
            Replay = replay;
            ActiveMod = replay.Mod;

            hitIterator = new TimeIter<TsVal<InvincibilityType>>(
                replay.Hits, x => x.Time, OnNewHit);
            slomoIterator = new TimeIter<TsVal<bool>>(
                replay.SlomoToggles, x => x.Time, OnNewSlomo);

            BurstShot.InitRandom(replay.BurstSeed);

            watchingReplayText = new SimpleText
            {
                Text = $"[ replay ]",
                Font = Fonts.Content.Orkney13,
                Color = new Color(Color.Gray, 0.8f),
                Position = new Vector2(NoisEvader.ScreenBounds.Width * 0.5f,
                    NoisEvader.ScreenBounds.Height * 0.95f),
                YOrigin = YOrigin.Center,
                XOrigin = XOrigin.Center
            };

            InitAndStart(level, ActiveMod, settings);
        }

        protected override void UpdatePlayer(LevelTime levelTime)
        {
            player.Update(levelTime, arena.Circle, camera, true);
            hitIterator.Update(levelTime);
            bulletMgr.CheckPlayerCollisions(player, true);
        }

        protected override void CollisionOccurred()
        {
            bulletMgr.MarkAllBulletsHit();
            if (Config.GameSettings.DrawParticles)
                particles.CreatePlayerHitParticles(player.Center);
            DoHitScreenShake();
        }

        protected override void UpdateSlomo()
        {
            var lt = new LevelTime
            {
                SongPosition = songPosition
            };
            slomoIterator.Update(lt);
        }

        private void OnNewHit(TimeIterActionArgs<TsVal<InvincibilityType>> args)
        {
            var invType = args.List[args.Index].Value;
            player.Hit(invType);
            if (invType == InvincibilityType.Normal)
            {
                CollisionOccurred();
            }
            else if (invType == InvincibilityType.Heart)
            {
                player.HeartLost();
            }
        }

        private void OnNewSlomo(TimeIterActionArgs<TsVal<bool>> args)
        {
            SetSlomo(args.List[args.Index].Value);
        }

        protected override void DrawOverlays(SpriteBatch spriteBatch)
        {
            base.DrawOverlays(spriteBatch);
            watchingReplayText.Draw(spriteBatch);
        }

    }
}
