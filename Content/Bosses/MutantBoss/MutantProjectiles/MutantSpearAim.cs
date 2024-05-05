using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Intrinsics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles
{
    public class MutantSpearAim : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.AprilFools ?
            "FargowiltasSouls/Content/Bosses/MutantBoss/TextureAlts/MutantSpear_April" :
            "FargowiltasSouls/Content/Projectiles/BossWeapons/HentaiSpear";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.timeLeft = 85;
            CooldownSlot = 1;
            Projectile.FargoSouls().TimeFreezeImmune = true;
            Projectile.FargoSouls().DeletionImmuneRank = 2;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Intersects(targetHitbox))
                return true;

            float length = 200;
            float dummy = 0f;
            Vector2 offset = length / 2 * Projectile.scale * (Projectile.rotation - MathHelper.ToRadians(135f)).ToRotationVector2();
            Vector2 end = Projectile.Center - offset;
            Vector2 tip = Projectile.Center + offset;

            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), end, tip, 8f * Projectile.scale, ref dummy))
                return true;

            return false;
        }

        public ref float MutantIndex => ref Projectile.ai[0];
        public ref float TrackingStrength => ref Projectile.ai[1];      // Functions to check if the spear aim is predictive by setting it to any value above 0. The non-zero value corresponds to the tracking strength. 
        public ref float SpearAimDuration => ref Projectile.ai[2];
        public ref float Timer => ref Projectile.localAI[0];

        public override void AI()
        {
            NPC mutant = Main.npc[(int)MutantIndex];

            if (mutant.active && mutant.type == ModContent.NPCType<MutantBoss>())
            {
                // Stick to Mutant
                Projectile.Center = mutant.Center;

                // Initialization
                if (Timer == 0)
                {
                    Projectile.rotation = mutant.SafeDirectionTo(Main.player[mutant.target].Center).ToRotation();
                    Projectile.timeLeft = (int)SpearAimDuration;
                }

                // If the tracking strength is above 0, it's a predictive spear, so track the player accordingly
                if (TrackingStrength > 0)
                {
                    Projectile.rotation = Projectile.rotation.AngleLerp(mutant.SafeDirectionTo(Main.player[mutant.target].Center + Main.player[mutant.target].velocity * 30).ToRotation(), 0.2f);
                }
                // Otherwise, just aim at the player
                else
                {
                    Projectile.rotation = mutant.SafeDirectionTo(Main.player[mutant.target].Center - Main.player[mutant.target].velocity.SafeNormalize(Vector2.Zero) * 10).ToRotation();
                }
            }
            else
                Projectile.Kill();

            Timer++;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), target.Center + Main.rand.NextVector2Circular(100, 100), Vector2.Zero, ModContent.ProjectileType<MutantBombSmall>(), 0, 0f, Projectile.owner);
            if (WorldSavingSystem.EternityMode)
            {
                target.FargoSouls().MaxLifeReduction += 100;
                target.AddBuff(ModContent.BuffType<OceanicMaulBuff>(), 5400);
                target.AddBuff(ModContent.BuffType<MutantFangBuff>(), 180);
            }
            target.AddBuff(ModContent.BuffType<CurseoftheMoonBuff>(), 600);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            int frameHeight = tex.Height / Main.projFrames[Projectile.type];
            Rectangle rect = new(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            float rotationOffset = MathHelper.ToRadians(135);

            int trailCacheLength = ProjectileID.Sets.TrailCacheLength[Projectile.type];
            for (int i = 0; i < trailCacheLength; i++)
            {
                float afterimageRatio = (trailCacheLength - i) / trailCacheLength;
                Color afterimageColor = Color.White * Projectile.Opacity * afterimageRatio;
                float scale = Projectile.scale * afterimageRatio;
                Main.spriteBatch.Draw(tex, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, rect, afterimageColor, Projectile.oldRot[i] + rotationOffset, rect.Size() / 2f, scale, SpriteEffects.None, 0f);
            }

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, rect, Color.White * Projectile.Opacity, Projectile.rotation + rotationOffset, rect.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);

            Texture2D glow = ModContent.Request<Texture2D>("FargowiltasSouls/Content/Bosses/MutantBoss/MutantProjectiles/MutantSpearAimGlow").Value;
            float modifier = Projectile.timeLeft / SpearAimDuration;
            Color glowColor = FargoSoulsUtil.AprilFools ? new Color(255, 191, 51, 210) : new(51, 255, 191, 210);
            if (TrackingStrength != 0)
                glowColor = FargoSoulsUtil.AprilFools ? new Color(255, 0, 0, 210) : new Color(0, 0, 255, 210);
            glowColor *= 1f - modifier;
            float glowScale = Projectile.scale * 8f * modifier;
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, glow.Bounds, glowColor, 0, glow.Bounds.Size() / 2f, glowScale, SpriteEffects.None, 0);

            return false;
        }
    }
}