using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles
{
    public class MutantAura : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.AprilFools ?
            "FargowiltasSouls/Content/Bosses/MutantBoss/TextureAlts/MutantSphere_April" :
            "Terraria/Images/Projectile_454";

        private const float RotationPerTick = MathHelper.Pi / 57f;
        private const float Threshold = 350;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 2400;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 46;
            Projectile.height = 46;
            Projectile.scale = 2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.netImportant = true;
            Projectile.alpha = 255;
            Projectile.FargoSouls().TimeFreezeImmune = true;
        }

        public ref float CurrentRotation => ref Projectile.ai[0];
        public ref float MutantIndex => ref Projectile.ai[1];
        public ref float AI2 => ref Projectile.ai[2];
        public ref float Timer => ref Projectile.localAI[0];
        public ref float LAI1 => ref Projectile.localAI[1];
        public ref float LAI2 => ref Projectile.localAI[2];

        public override void AI()
        {
            NPC mutant = FargoSoulsUtil.NPCExists(MutantIndex, ModContent.NPCType<MutantBoss>());

            // Stick to the boss and slowly fade in opacity
            if (mutant is not null)
            {
                Projectile.Center = mutant.Center;
                Projectile.alpha -= 4;
                if (Projectile.alpha < 0)
                    Projectile.alpha = 0;

            }
            // Stop moving, start shrinking, and fade away
            else
            {
                MutantIndex = -1;
                Projectile.velocity = Vector2.Zero;
                Projectile.alpha += 4;
                if (Projectile.alpha > 255)
                {
                    Projectile.Kill();
                    return;
                }
            }

            // Update visuals, frames, and other values
            Projectile.timeLeft = 2;
            Projectile.scale = (1f - Projectile.alpha / 255f) * 0.5f;
            CurrentRotation = MathHelper.WrapAngle(CurrentRotation + RotationPerTick);
            if (++Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame > 1)
                    Projectile.frame = 0;
            }

            Timer++;
        }

        public override bool? CanDamage() => false;

        public override bool PreDraw(ref Color lightColor)
        {
            // Base texture
            Texture2D baseTex = ModContent.Request<Texture2D>(Texture).Value;
            int baseFrameHeight = baseTex.Height / Main.projFrames[Projectile.type];
            Rectangle baseRect = new(0, baseFrameHeight * Projectile.frame, baseTex.Width, baseFrameHeight);
            Color baseColor = Projectile.GetAlpha(lightColor);

            // Glow texture
            Texture2D glow = ModContent.Request<Texture2D>("FargowiltasSouls/Content/Bosses/MutantBoss/MutantProjectiles/MutantSphereGlow").Value;
            Rectangle glowRect = new(0, 0, glow.Width, glow.Height);
            Color glowColor = (FargoSoulsUtil.AprilFools ? Color.Red : new Color(196, 247, 255, 0)) * 0.6f;

            // Draw the entire arena
            int numEyes = 7;
            for (int i = 0; i < numEyes; i++)
            {
                Vector2 drawOffset = new Vector2(Threshold * Projectile.scale / 2f, 0f).RotatedBy(CurrentRotation + MathHelper.TwoPi / numEyes * i);

                // Draw afterimages
                float trailCacheLength = ProjectileID.Sets.TrailCacheLength[Projectile.type];
                for (int j = 0; j < (int)trailCacheLength; j++)
                {
                    float afterimageRatio = (trailCacheLength - j) / trailCacheLength;
                    Color baseAfterimageColor = baseColor * afterimageRatio;
                    Color glowAfterimageColor = glowColor * afterimageRatio;
                    Vector2 drawPos = Projectile.oldPos[j] + Projectile.Hitbox.Size() / 2f + drawOffset.RotatedBy(RotationPerTick * -j) - Main.screenPosition;

                    Main.spriteBatch.Draw(glow, drawPos, glowRect, glowAfterimageColor, Projectile.rotation, glowRect.Size() / 2f, Projectile.scale * 1.4f, SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(baseTex, drawPos, baseRect, baseAfterimageColor, Projectile.rotation, baseRect.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);
                }

                // Draw the base projectile
                Main.spriteBatch.Draw(glow, Projectile.Center + drawOffset - Main.screenPosition, glowRect, glowColor,
                    Projectile.rotation, glowRect.Size() / 2f, Projectile.scale * 1.4f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(baseTex, Projectile.Center + drawOffset - Main.screenPosition, baseRect, baseColor,
                    Projectile.rotation, baseRect.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);
            }

            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * Projectile.Opacity * 0.8f;
        }
    }
}