using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles
{
    public class MutantTrueEyeSphereProj : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.AprilFools ?
            "FargowiltasSouls/Content/Bosses/MutantBoss/TextureAlts/MutantTrueEyeSphere_April" :
            "Terraria/Images/Projectile_454";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Phantasmal Sphere");
            Main.projFrames[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 46;
            Projectile.height = 46;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.hostile = true;
            Projectile.timeLeft = 360;
            CooldownSlot = 1;
        }

        public override bool CanHitPlayer(Player target)
        {
            return target.hurtCooldowns[1] == 0;
        }

        public ref float TrueEyeIdentity => ref Projectile.ai[0];
        public ref float AI1 => ref Projectile.ai[1];
        public ref float AI2 => ref Projectile.ai[2];
        public ref float LAI0 => ref Projectile.localAI[0];
        public ref float LAI1 => ref Projectile.localAI[1];
        public ref float Timer => ref Projectile.localAI[2];

        public override void AI()
        {
            int byIdentity = FargoSoulsUtil.GetProjectileByIdentity(Projectile.owner, (int)TrueEyeIdentity, ModContent.ProjectileType<MutantTrueEyeSphere>());

            if (byIdentity != -1)
            {
                // Timed to shoot right as the True Eye rams
                if (Projectile.timeLeft > 295)
                {
                    // Stop following true eye if true eye has lost the target and isn't preparing to charge
                    if (Main.projectile[byIdentity].ai[1] == 0f) 
                    {
                        Projectile.ai[0] = -1f;
                        Projectile.velocity = Vector2.Zero;
                        Projectile.netUpdate = true;
                    }
                    else
                        Projectile.velocity = Main.projectile[byIdentity].velocity;
                }
            }

            // Fade in
            float fadeInTime = 40f;
            Projectile.Opacity = Utilities.InverseLerp(0f, fadeInTime, Timer);
            Projectile.scale = Utilities.InverseLerp(0f, fadeInTime, Timer);

            // Update frame
            if (++Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame > 1)
                    Projectile.frame = 0;
            }

            Timer++;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (WorldSavingSystem.EternityMode)
            {
                target.FargoSouls().MaxLifeReduction += 100;
                target.AddBuff(ModContent.BuffType<OceanicMaulBuff>(), 5400);
            }
            target.AddBuff(ModContent.BuffType<CurseoftheMoonBuff>(), 360);
            target.AddBuff(ModContent.BuffType<MutantFangBuff>(), 180);
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
                Main.spriteBatch.Draw(glow, afterimagePos - Main.screenPosition, rect, afterimageColor, Projectile.rotation, rect.Size() / 2f, scale * 1.5f, SpriteEffects.None, 0);
            }

            Main.spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition, rect, Color.White * 0.85f, Projectile.rotation,
                rect.Size() / 2f, Projectile.scale * 1.25f, SpriteEffects.None, 0);

            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            int frameHeight = tex.Height / Main.projFrames[Projectile.type];
            Rectangle rect = new(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, rect, Color.White * Projectile.Opacity, Projectile.rotation, rect.Size() / 2f, Projectile.scale * 0.9f, SpriteEffects.None, 0f);
        }

        public override void OnKill(int timeleft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath6, Projectile.Center);

            for (int i = 0; i < 2; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Alpha: 100, Scale: 1.5f);

            for (int i = 0; i < 4; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Vortex, Alpha: Main.rand.Next(100), Scale: Main.rand.NextFloat(1.5f, 2.5f));
                Main.dust[d].noGravity = true;
            }
        }
    }
}