using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles
{
    public class MutantTrueEyeDeathray : BaseMutantTrueEye
    {
        public override int MovementLength => 120;

        public override Vector2 HoverAbovePlayerDistance => new(0f, -300f);

        public override void SetDefaults()
        {
            base.SetDefaults();

            Projectile.hide = true;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
        }

        public override void ShootBehavior()
        {
            Player target = Main.player[(int)PlayerTarget];

            // Fire lasor
            if (EyeTimer == 1)
            {
                float rotationSpeed = MathHelper.TwoPi / 270f * MathF.Sign(Projectile.Center.X - target.Center.X);
                CurrentEyeAngle -= rotationSpeed * 60f;
                CurrentPupilOffset = rotationSpeed;
                Vector2 speed = -Vector2.UnitX.RotatedBy(CurrentEyeAngle);

                if (FargoSoulsUtil.HostCheck)
                    Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center - Vector2.UnitY * 6f, speed, ModContent.ProjectileType<MutantTrueEyeDeathrayProj>(),
                                Projectile.damage, 0f, Projectile.owner, rotationSpeed);
            }

              if (EyeTimer > 90)
                Projectile.Kill();

            // Adjust angle
            else
                CurrentEyeAngle += CurrentPupilOffset;
        }
    }
}
