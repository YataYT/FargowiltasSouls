using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using System.IO;
using Microsoft.Xna.Framework;
using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework.Graphics;
using FargowiltasSouls.Content.Projectiles.Masomode;
using Terraria.Audio;

namespace FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles
{
    public abstract class BaseMutantTrueEye : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.AprilFools ?
            "FargowiltasSouls/Content/Bosses/MutantBoss/TextureAlts/MutantTrueEye_April" :
            "Terraria/Images/Projectile_650";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 42;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            CooldownSlot = 1;
            Projectile.penetrate = -1;
            Projectile.FargoSouls().DeletionImmuneRank = 1;
        }

        public override bool? CanDamage() => false;

        public abstract Vector2 HoverAbovePlayerDistance { get; }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[1]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[1] = reader.ReadSingle();
        }

        public virtual float SlowDownFactor => 0.95f;

        public abstract int MovementLength { get; }

        public virtual void SlowDownBehavior()
        {
            Projectile.velocity *= SlowDownFactor;

            // When it's slowed down enough, stop and move to next attack
            if (Projectile.velocity.Length() < 1f)
            {
                EyeTimer = 0f;
                Projectile.velocity = Vector2.Zero;
                Behavior++;
                Projectile.netUpdate = true;
            }
        }

        public abstract void ShootBehavior();

        public ref float PlayerTarget => ref Projectile.ai[0];
        public ref float Direction => ref Projectile.ai[1];
        public ref float Behavior => ref Projectile.ai[2];
        public ref float EyeTimer => ref Projectile.localAI[0];
        public ref float CurrentEyeAngle => ref Projectile.localAI[1];
        public ref float CurrentPupilOffset => ref Projectile.localAI[2];

        public override void AI()
        {
            Player player = Main.player[(int)PlayerTarget];

            switch ((int)Behavior)
            {
                // Movement
                case 0:
                    Vector2 vel = player.Center - Projectile.Center + HoverAbovePlayerDistance;
                    if (vel != Vector2.Zero)
                        Projectile.velocity = (Projectile.velocity * 29f + (vel.SafeNormalize(Vector2.One) * 24f)) / 30f;

                    if (Projectile.Distance(player.Center) < 150f)
                    {
                        Projectile.velocity.X += 0.25f * MathF.Sign(player.Center.X - Projectile.Center.X);
                        Projectile.velocity.X += 0.25f * MathF.Sign(player.Center.Y - Projectile.Center.Y);
                    }

                    if (EyeTimer > MovementLength)
                    {
                        EyeTimer = 0f;
                        Behavior++;
                        Projectile.netUpdate = true;
                    }

                    break;

                // Slow down
                case 1:
                    SlowDownBehavior();
                    break;

                // Shooting behavior
                case 2:
                    ShootBehavior();
                    break;
            }

            // Update frame
            if (++Projectile.frameCounter >= 4)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.frame = 0;
            }

            Projectile.rotation = MathHelper.WrapAngle(Projectile.rotation);

            // Update pupil movement when not in shooting phase
            if (Behavior != 2f)
                UpdatePupil();

            EyeTimer++;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 30; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch, Scale: 3f);
                Main.dust[d].noGravity = true;
                Main.dust[d].noLight = true;
            }

            SoundEngine.PlaySound(SoundID.Zombie102, Projectile.Center);
        }

        protected void UpdatePupil()
        {
            float maybeEyeOffset = MathF.IEEERemainder(CurrentPupilOffset, 1f);
            if (maybeEyeOffset < 0f)
                maybeEyeOffset++;

            float flooredValue = MathF.Floor(CurrentPupilOffset);
            float currentEyeAngle = (CurrentEyeAngle % MathHelper.TwoPi) - MathHelper.Pi;
            float angleTowardsPlayer = Projectile.AngleTo(Main.player[(int)PlayerTarget].Center);
            
            // Gradually angle eye towards the player
            CurrentEyeAngle = Vector2.Lerp(currentEyeAngle.ToRotationVector2(), angleTowardsPlayer.ToRotationVector2(), 0.1f).ToRotation() + 2f * MathHelper.TwoPi + MathHelper.Pi;
            CurrentPupilOffset = MathHelper.Clamp(maybeEyeOffset + 0.05f, 0f, 0.999f) + (flooredValue + MathF.Sign(-12f - flooredValue));
        }

        public override bool? CanCutTiles() => false;

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<CurseoftheMoonBuff>(), 360);
            if (WorldSavingSystem.EternityMode)
                target.AddBuff(ModContent.BuffType<MutantFangBuff>(), 180);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * Projectile.Opacity;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Utilities.DrawAfterimagesCentered(Projectile, 2, lightColor);

            Texture2D pupil = ModContent.Request<Texture2D>("FargowiltasSouls/Content/Projectiles/Minions/TrueEyePupil").Value;
            Vector2 pupilOffset = new Vector2(CurrentPupilOffset / 2f, 0f).RotatedBy(CurrentEyeAngle);
            pupilOffset += new Vector2(0f, -6f).RotatedBy(Projectile.rotation);

            Main.spriteBatch.Draw(pupil, pupilOffset + Projectile.Center - Main.screenPosition, pupil.Bounds, Color.White * Projectile.Opacity, 0f, pupil.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}
