using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Golf;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public class MutantSphereSmall : ModProjectile
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
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 200;
            CooldownSlot = 1;
        }

        public override bool CanHitPlayer(Player target)
        {
            return target.hurtCooldowns[1] == 0;
        }

        

        // Set to -1 to make it non-homing
        public ref float PlayerToHomeIn => ref Projectile.ai[0];
        public ref float AI1 => ref Projectile.ai[1];
        public ref float AI2 => ref Projectile.ai[2];
        public ref float Timer => ref Projectile.localAI[0];
        public ref float HomingCooldown => ref Projectile.localAI[1];
        public ref float LAI2 => ref Projectile.localAI[2];

        public override void AI()
        {
            // Cycle frames
            if (++Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame > 1)
                    Projectile.frame = 0;
            }

            // Fade in
            float fadeInTime = 12f;
            Projectile.Opacity = Utilities.InverseLerp(0f, fadeInTime, Timer);
            Projectile.scale = Utilities.InverseLerp(0f, fadeInTime, Timer);

            // Home in if the player to home in is a valid index (otherwise it just stays straight at its initial velocity)
            if (PlayerToHomeIn > -1 && PlayerToHomeIn < Main.maxPlayers)
            {
                int homingDelay = 20;
                float desiredFlySpeed = 5f;
                if (++HomingCooldown > homingDelay)
                {
                    Player p = Main.player[(int)PlayerToHomeIn];
                    Vector2 desiredVelocity = Projectile.SafeDirectionTo(p.Center) * desiredFlySpeed;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.05f);
                }
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

            if (FargoSoulsUtil.HostCheck) //explosion
                Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<MutantBombSmall>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * Projectile.Opacity;
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
                Main.spriteBatch.Draw(glow, afterimagePos - Main.screenPosition, rect, afterimageColor, Projectile.rotation, rect.Size() / 2f, scale * 1.25f, SpriteEffects.None, 0);
            }

            Main.spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition, rect, Color.White * 0.85f, Projectile.rotation,
                rect.Size() / 2f, Projectile.scale * 1.25f, SpriteEffects.None, 0);

            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int frameHeight = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
            Rectangle rect = new(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, rect, Color.White * Projectile.Opacity,
                Projectile.rotation, rect.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
        }
    }
}