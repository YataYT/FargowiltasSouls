using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles
{
    public class BaseMutantSphere : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.AprilFools ?
            "FargowiltasSouls/Content/Bosses/MutantBoss/TextureAlts/MutantSphere_April" :
            "Terraria/Images/Projectile_454";

        public virtual float ScaleMultiplier => 1f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = (int)(40 * ScaleMultiplier);
            Projectile.height = (int)(40 * ScaleMultiplier);
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 480;
            Projectile.alpha = 0;
            CooldownSlot = 1;
        }

        // Make sure to increment this in the inherited projectile AI if you're using this
        public ref float Timer => ref Projectile.localAI[0];

        public override bool CanHitPlayer(Player target)
        {
            return target.hurtCooldowns[1] == 0;
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
        }

        public override void OnKill(int timeleft)
        {
            // Play the death sound randomly, taking into account how many spheres are alive to control volume levels
            if (Main.rand.NextBool(Main.player[Projectile.owner].ownedProjectileCounts[Projectile.type] / 10 + 1))
                SoundEngine.PlaySound(SoundID.NPCDeath6, Projectile.Center);

            for (int i = 0; i < 2; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Alpha: 100, Scale: 1.5f);

            for (int i = 0; i < 4; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Vortex, Alpha: Main.rand.Next(100), Scale: Main.rand.NextFloat(1.5f, 2.5f));
                Main.dust[d].noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = ModContent.Request<Texture2D>("FargowiltasSouls/Content/Bosses/MutantBoss/MutantProjectiles/MutantSphereGlow").Value;
            Color glowColor = (FargoSoulsUtil.AprilFools ? Color.Red : new Color(196, 247, 255, 0)) * 0.9f * Projectile.Opacity;
            Rectangle rect = new(0, 0, glow.Width, glow.Height);

            int trailCacheLength = ProjectileID.Sets.TrailCacheLength[Projectile.type];
            for (int i = 0; i < trailCacheLength; i++)
            {
                float afterimageRatio = (trailCacheLength - i) / trailCacheLength;
                Color afterimageColor = glowColor * afterimageRatio;
                float scale = Projectile.scale * afterimageRatio;
                Vector2 afterimagePos = Projectile.oldPos[i] - Vector2.Normalize(Projectile.velocity) * i * trailCacheLength + Projectile.Size / 2f;
                Main.spriteBatch.Draw(glow, afterimagePos - Main.screenPosition, rect, afterimageColor, Projectile.rotation, rect.Size() / 2f, scale * ScaleMultiplier, SpriteEffects.None, 0);
            }

            Main.spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition, rect, Color.White * 0.85f, Projectile.rotation,
                rect.Size() / 2f, Projectile.scale * ScaleMultiplier, SpriteEffects.None, 0);

            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int frameHeight = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
            Rectangle rect = new(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, rect, Color.White * Projectile.Opacity,
                Projectile.rotation, rect.Size() / 2f, Projectile.scale * (ScaleMultiplier * 0.7f), SpriteEffects.None, 0);
        }
    }
}
