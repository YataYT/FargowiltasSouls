using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles
{
    public class MutantTrueEyeBolt : BaseMutantTrueEye
    {
        public override int MovementLength => 60;

        public override Vector2 HoverAbovePlayerDistance => new(-200f * Direction, -200f);

        public override void ShootBehavior()
        {
            int numBoltsToShoot = 2;
            int timeBetweenBolts = 7;

            // Perish shortly after the last bolt is fired
            if (EyeTimer >= (numBoltsToShoot + 1) * timeBetweenBolts)
            {
                Projectile.Kill();
                return;
            }

            // Shoot bolts
            if (EyeTimer % timeBetweenBolts == 0)
            {
                ShootBolt();

                // Play a sound on the first shot
                if (EyeTimer == timeBetweenBolts)
                    SoundEngine.PlaySound(SoundID.NPCDeath6, Projectile.Center);
            }
        }

        private void ShootBolt()
        {
            Player target = Main.player[(int)PlayerTarget];
            Vector2 spawnPos = Projectile.Center - Vector2.UnitY * 6f;
            Vector2 boltVelocity = (target.Center + target.velocity * 15f - spawnPos).SafeNormalize(Vector2.One) * 8f;
            if (FargoSoulsUtil.HostCheck)
                Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), spawnPos, boltVelocity, ProjectileID.PhantasmalBolt, Projectile.damage, 0f, Projectile.owner);
        }
    }
}
