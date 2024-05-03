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
    public class MutantSword : BaseMutantSphere
    {
        public override float ScaleMultiplier => 1.4f;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.FargoSouls().DeletionImmuneRank = 2;
            Projectile.FargoSouls().TimeFreezeImmune = true;
            Projectile.timeLeft = 110;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Intersects(targetHitbox))
                return true;

            Rectangle trailHitbox = projHitbox;
            trailHitbox.X = (int)Projectile.oldPosition.X;
            trailHitbox.Y = (int)Projectile.oldPosition.Y;
            if (trailHitbox.Intersects(targetHitbox))
                return true;

            return false;
        }

        public ref float MutantIndex => ref Projectile.ai[0];
        public ref float OffsetDistance => ref Projectile.ai[1];
        public ref float TotalTime => ref Projectile.ai[2];
        public ref float InitialDirection => ref Projectile.localAI[1];

        public override void AI()
        {
            NPC npc = FargoSoulsUtil.NPCExists(MutantIndex, ModContent.NPCType<MutantBoss>());
            if (npc != null)
            {
                // Initialize rotation
                if (Timer == 0)
                {
                    // Set defaults just in case
                    if (TotalTime == 0)
                        TotalTime = 110;

                    InitialDirection = Projectile.DirectionFrom(npc.Center).ToRotation();
                    Projectile.timeLeft = (int)TotalTime;
                }

                Vector2 offset = new Vector2(OffsetDistance, 0).RotatedBy(npc.localAI[0] + InitialDirection);
                Projectile.Center = npc.Center + offset;
            }
            else
            {
                Projectile.Kill();
                return;
            }

            // Fade in
            float fadeInTime = 30f;
            Projectile.Opacity = Utilities.InverseLerp(0f, fadeInTime, Timer);
            Projectile.scale = Utilities.InverseLerp(0f, fadeInTime, Timer);

            // Cycle frames
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
            target.velocity.X = target.Center.X < Main.npc[(int)Projectile.ai[0]].Center.X ? -15f : 15f;
            target.velocity.Y = -10f;
            
            base.OnHitPlayer(target, info);
        }

        public override void OnKill(int timeleft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath6, Projectile.Center);

            int dust = FargoSoulsUtil.AprilFools ? DustID.SolarFlare : DustID.Vortex;

            // Smoke
            for (int i = 0; i < 2; ++i)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0.0f, 0.0f, 100, new Color(), 1.5f);
                Main.dust[d].position = new Vector2(Projectile.width / 2, 0f).RotatedBy(MathHelper.TwoPi * Main.rand.NextFloat()) * Main.rand.NextFloat() + Projectile.Center;
            }

            // Sphere-specific dust
            for (int i = 0; i < 5; ++i)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dust, Alpha: Main.rand.Next(100), Scale: Main.rand.NextFloat(1.5f, 2.5f));
                Main.dust[d].position = new Vector2(Projectile.width / 2, 0f).RotatedBy(MathHelper.TwoPi * Main.rand.NextFloat()) * Main.rand.NextFloat() + Projectile.Center;
                Main.dust[d].noGravity = true;
            }
            for (int i = 0; i < 10; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dust, Alpha: 100, Scale: 3f);
                Main.dust[d].velocity *= 1.4f;
            }
            for (int i = 0; i < 10; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dust, Alpha: 100, Scale: Main.rand.NextFloat(1f, 2f));
                Main.dust[d].velocity *= Main.rand.NextFloat(12f, 21f);
                Main.dust[d].noGravity = true;
                Main.dust[d].noLight = true;
            }
            for (int i = 0; i < 10; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dust, Alpha: 100, Scale: Main.rand.NextFloat(2f, 3.5f));
                if (Main.rand.NextBool(3))
                    Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= Main.rand.NextFloat(9f, 12f);
                Main.dust[d].position = Projectile.Center;
            }

            // Torch dust
            for (int i = 0; i < 5; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Alpha: 100, Scale: Main.rand.NextFloat(1.5f, 3.5f));
                Main.dust[d].noGravity = Main.rand.NextBool();
                Main.dust[d].velocity *= Main.rand.NextFloat(3f, 7f);
            }

            // Explosion
            if (FargoSoulsUtil.HostCheck)
                Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<MutantBombSmall>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
    }
}