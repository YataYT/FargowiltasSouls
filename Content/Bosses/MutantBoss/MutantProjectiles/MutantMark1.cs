using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public class MutantMark1 : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.AprilFools ?
            "FargowiltasSouls/Content/Bosses/MutantBoss/TextureAlts/MutantSphere_April" :
            "Terraria/Images/Projectile_454";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 46;
            Projectile.height = 46;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.timeLeft = 90;
            Projectile.aiStyle = -1;
            Projectile.scale = 0.5f;
            Projectile.alpha = 0;
            Projectile.penetrate = -1;
            CooldownSlot = 1;

            Projectile.FargoSouls().DeletionImmuneRank = 1;
        }

        public override bool CanHitPlayer(Player target)
        {
            return target.hurtCooldowns[1] == 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (FargoSoulsUtil.HostCheck)
                Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center + Projectile.velocity * Projectile.timeLeft,
                    Vector2.Normalize(Projectile.velocity), ModContent.ProjectileType<MutantDeathraySmall>(), Projectile.damage, 0f, Projectile.owner);
        }

        public override void AI()
        {
            // Loop frames
            if (++Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame > 1)
                    Projectile.frame = 0;
            }
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
            SoundEngine.PlaySound(SoundID.NPCDeath6, Projectile.Center);

            if (FargoSoulsUtil.HostCheck)
                Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center,
                    Vector2.Normalize(Projectile.velocity), ModContent.ProjectileType<MutantDeathray>(), Projectile.damage, 0f, Projectile.owner);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * Projectile.Opacity;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = ModContent.Request<Texture2D>("FargowiltasSouls/Content/Bosses/MutantBoss/MutantProjectiles/MutantSphereGlow").Value;
            Color glowColor = (FargoSoulsUtil.AprilFools ? Color.Red : new Color(255, 255, 255, 0)) * 0.85f;

            Utilities.DrawAfterimagesCentered(Projectile, 2, lightColor);

            Main.spriteBatch.Draw(glow, Projectile.position + Projectile.Size / 2f - Main.screenPosition, null,
                glowColor, 0, glow.Size() / 2f, Projectile.scale * 1.5f, SpriteEffects.None, 0);

            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            int frameHeight = tex.Height / Main.projFrames[Projectile.type];
            Rectangle rect = new(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, rect, Color.White * Projectile.Opacity,
                Projectile.rotation, rect.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
        }
    }
}