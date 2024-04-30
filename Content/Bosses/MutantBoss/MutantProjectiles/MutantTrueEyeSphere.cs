using Microsoft.Xna.Framework;
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
    public class MutantTrueEyeSphere : BaseMutantTrueEye
    {
        public override int MovementLength => 90;

        public override Vector2 HoverAbovePlayerDistance => new(300f * Direction, -300f);

        public override void ShootBehavior()
        {
            Player target = Main.player[(int)PlayerTarget];

            if (EyeTimer == 1)
            {
                SoundEngine.PlaySound(SoundID.Zombie102, Projectile.Center);
                Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.One) * 24f;
                Projectile.netUpdate = true;
            }

            if (EyeTimer > 10f)
            {
                Projectile.Kill();
                return;
            }

            float newRotation = MathHelper.WrapAngle(Projectile.velocity.ToRotation() + MathHelper.PiOver2);
            Projectile.rotation = (Projectile.rotation * 11f + newRotation) / 11f;
        }

        public override void SlowDownBehavior()
        {
            Projectile.velocity *= 0.95f;

            if (EyeTimer > 60f)
            {
                Projectile.velocity = Vector2.Zero;
                EyeTimer = 0;
                Behavior++;
                return;
            }

            Projectile.rotation = MathHelper.WrapAngle(Projectile.rotation);
            Projectile.rotation = MathF.Abs(Projectile.rotation) >= 0.005f ? Projectile.rotation * 0.96f : 0;

            if (EyeTimer == 1)
            {
                int max = 6;
                float rotation = MathHelper.TwoPi / max;

                for (int i = 0; i < max; i++)
                {
                    Vector2 spawnPos = Projectile.Center - Vector2.UnitY * 6f + new Vector2(100f, 0f).RotatedBy(rotation * i);
                    if (FargoSoulsUtil.HostCheck)
                        Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), spawnPos, Vector2.Zero, ModContent.ProjectileType<MutantTrueEyeSphereProj>(),
                                    Projectile.damage, 0f, Projectile.owner, Projectile.identity, i);
                }
            }
        }
    }
}
