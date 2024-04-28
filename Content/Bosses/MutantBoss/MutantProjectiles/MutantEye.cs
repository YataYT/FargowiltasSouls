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
                    if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<MutantRitual>())
                    {
                        RitualID = i;
                        break;
                    }
                }
            }
        }

        public ref float Timer => ref Projectile.localAI[0];
        public ref float RitualID => ref Projectile.localAI[1];

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (DieOutsideArena)
            {
                Projectile ritual = FargoSoulsUtil.ProjectileExists(RitualID, ModContent.ProjectileType<MutantRitual>());

                // Despawn when the projectile reaches the arena boundary
                if (ritual != null && Projectile.Distance(ritual.Center) > 1200f)
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
            Color glowColor = (FargoSoulsUtil.AprilFools ? new Color(255, 0, 0, TrailAdditive) : new Color(31, 187, 192, TrailAdditive)) * 0.74f;
            Vector2 drawCenter = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 14f;

            for (int i = 0; i < 3; i++) //create multiple transparent trail textures ahead of the projectile
            {
                Vector2 drawCenter2 = drawCenter + (Projectile.velocity.SafeNormalize(Vector2.UnitX) * 8).RotatedBy(MathHelper.Pi / 5 - i * MathHelper.Pi / 5);
                drawCenter2 -= Projectile.velocity.SafeNormalize(Vector2.UnitX) * 8;
                Main.EntitySpriteDraw(glow, drawCenter2 - Main.screenPosition, rect, glowColor, Projectile.velocity.ToRotation() + MathHelper.PiOver2,
                    rect.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            }

            Utilities.DrawAfterimagesCentered(Projectile, 2, lightColor);

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