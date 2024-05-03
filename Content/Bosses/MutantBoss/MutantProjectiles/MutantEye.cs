using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public class MutantEye : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.AprilFools ?
            "FargowiltasSouls/Content/Bosses/MutantBoss/TextureAlts/MutantEye_April" :
            "Terraria/Images/Projectile_452";

        public virtual int TrailAdditive => 0;

        // Leave the option for children to enable if desired
        public virtual bool DieOutsideArena => Projectile.type == ModContent.ProjectileType<MutantEye>();

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 0;
            CooldownSlot = 1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            RitualID = -1;

            if (DieOutsideArena)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<MutantArena>())
                    {
                        RitualID = i;
                        break;
                    }
                }
            }
        }

        public ref float Timer => ref Projectile.localAI[0];
        public ref float CurrentTrailLength => ref Projectile.localAI[1];
        public ref float RitualID => ref Projectile.localAI[2];

        public override void AI()
        {
            // Adjust the rotation to face its velocity
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Make the trail grow in length as it travels
            if (CurrentTrailLength < ProjectileID.Sets.TrailCacheLength[Projectile.type])
                CurrentTrailLength += 0.1f;
            else
                CurrentTrailLength = ProjectileID.Sets.TrailCacheLength[Projectile.type];

            // Die if outside the arena (only applies to non-homing mutant eyes)
            if (DieOutsideArena)
            {
                Projectile ritual = FargoSoulsUtil.ProjectileExists(RitualID, ModContent.ProjectileType<MutantArena>());

                // Despawn when the projectile reaches the arena boundary
                if (ritual != null && Projectile.Distance(ritual.Center) > MutantArena.ArenaSize)
                    Projectile.timeLeft = 0;
            }

            Timer++;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (WorldSavingSystem.EternityMode)
            {
                target.FargoSouls().MaxLifeReduction += 100;
                target.AddBuff(ModContent.BuffType<OceanicMaulBuff>(), 5400);
                target.AddBuff(ModContent.BuffType<MutantFangBuff>(), 180);
            }
            target.AddBuff(ModContent.BuffType<CurseoftheMoonBuff>(), 360);
            Projectile.timeLeft = 0;
        }

        public override void OnKill(int timeleft)
        {
            SoundEngine.PlaySound(SoundID.Zombie103, Projectile.Center);

            for (int i = 0; i < 2; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Alpha: 100, Scale: 1.5f);

            for (int i = 0; i < 5; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Vortex, Alpha: 0, Scale: 2.5f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 3;

                int d2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Vortex, Alpha: 100, Scale: 1.5f);
                Main.dust[d2].noGravity = true;
                Main.dust[d2].velocity *= 2;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = ModContent.Request<Texture2D>("FargowiltasSouls/Content/Bosses/MutantBoss/MutantProjectiles/MutantEye_Glow").Value;
            int frameHeight = glow.Height / Main.projFrames[Projectile.type];
            Rectangle rect = new(0, frameHeight * Projectile.frame, glow.Width, frameHeight);
            Color glowColor = (FargoSoulsUtil.AprilFools ? new Color(255, 0, 0, TrailAdditive) : new Color(31, 187, 192, TrailAdditive)) * 0.3f;
            Vector2 baseDrawCenter = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 14;

            for (int i = 0; i < 3; i++) //create multiple transparent trail textures ahead of the projectile
            {
                Vector2 drawCenter = baseDrawCenter + (Projectile.velocity.SafeNormalize(Vector2.UnitX) * 8).RotatedBy(MathHelper.Pi / 5 - i * MathHelper.Pi / 5); // Use a normalized version of the projectile's velocity to offset it at different angles
                drawCenter -= Projectile.velocity.SafeNormalize(Vector2.UnitX) * 8; // Then move it backwards
                float scale = Projectile.scale + MathF.Sin(Timer / 4f) / 10;
                Main.spriteBatch.Draw(glow, drawCenter - Main.screenPosition, rect, glowColor, Projectile.velocity.ToRotation() + MathHelper.PiOver2, rect.Size() / 2f, scale, SpriteEffects.None, 0);
            }

            // Trail grows in length as projectile travels
            for (float i = CurrentTrailLength - 1; i > 0; i -= CurrentTrailLength / ProjectileID.Sets.TrailCacheLength[Projectile.type])
            {
                Color afterimageColor = glowColor * 0.6f;

                afterimageColor *= (int)((CurrentTrailLength - i) / CurrentTrailLength) ^ 2;
                float scale = Projectile.scale * (float)(CurrentTrailLength - i) / CurrentTrailLength + MathF.Sin(Timer) / 10;
                Vector2 afterimagePos = Projectile.oldPos[(int)i] - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 14;
                Main.EntitySpriteDraw(glow, afterimagePos + Projectile.Size / 2f - Main.screenPosition, rect, afterimageColor,
                    Projectile.velocity.ToRotation() + MathHelper.PiOver2, rect.Size() / 2f, scale * 0.8f, SpriteEffects.None, 0);
            }

            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            int frameHeight = tex.Height / Main.projFrames[Projectile.type];
            Rectangle rect = new(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, rect, Color.White, Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
        }
    }
}