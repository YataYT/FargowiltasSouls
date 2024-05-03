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
    public class MutantSphereSmall : BaseMutantSphere
    {
        public override float ScaleMultiplier => 1.1f;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.timeLeft = 120;
        }

        // Set to -1 to make it non-homing
        public ref float PlayerToHomeIn => ref Projectile.ai[0];
        public ref float HomingCooldown => ref Projectile.localAI[1];

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

        public override void OnKill(int timeleft)
        {
            base.OnKill(timeleft);
            if (FargoSoulsUtil.HostCheck) //explosion
                Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<MutantBombSmall>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
    }
}