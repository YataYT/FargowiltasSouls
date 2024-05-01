using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using UtfUnknown.Core.Models.SingleByte.Finnish;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public class MutantEyeHoming : MutantEye
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.timeLeft = 900;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Makes setting AI2 optional
            if (SpeedBonus == 0)
                SpeedBonus = 1;
        }

        /*public ref float AI0 => ref Projectile.ai[0];
        public ref float AI1 => ref Projectile.ai[1];
        public ref float AI2 => ref Projectile.ai[2];
        public ref float LAI0 => ref Projectile.localAI[0];
        public ref float LAI1 => ref Projectile.localAI[1];
        public ref float LAI2 => ref Projectile.localAI[2];*/

        public ref float MutantIndex => ref Projectile.ai[0];
        public ref float SpeedBonus => ref Projectile.ai[1];
        public ref float FlyAway => ref Projectile.ai[2];
        public ref float HomingTimer => ref Projectile.localAI[2];

        public override void AI()
        {
            int endHomingTime = -600;
            float maxSpeed = WorldSavingSystem.MasochistModeReal ? 15f : 10f;

            // Perish if the mutant or the target cannot be retrieved
            NPC mutant = FargoSoulsUtil.NPCExists(MutantIndex, ModContent.NPCType<MutantBoss>());
            if (mutant == null)
            {
                Projectile.Kill();
                return;
            }
            Player target = FargoSoulsUtil.PlayerExists(mutant.target);
            if (target == null)
            {
                Projectile.Kill();
                return;
            }

            // Fly away from the player if it's still homing (?)
            if ((FlyAway != 0 || HomingTimer > 0) && target != null && Projectile.Distance(target.Center) < 240)
            {
                float angle = Projectile.DirectionFrom(target.Center).ToRotation() - Projectile.velocity.ToRotation();
                angle = MathHelper.WrapAngle(angle);
                Projectile.velocity = Projectile.velocity.RotatedBy(angle * 0.05f);

                // Limit the lifetime
                if (Projectile.timeLeft > 180)
                    Projectile.timeLeft = 180;
            }
            else if (HomingTimer < 0 && HomingTimer > endHomingTime)
            {
                // Accelerate to max homing speed
                float homingMaxSpeed = maxSpeed * SpeedBonus;
                Vector2 targetPos = target.Center;
                float deactivateHomingRange = WorldSavingSystem.MasochistModeReal ? 360 : 480;
                if (Projectile.velocity.Length() < homingMaxSpeed)
                    Projectile.velocity *= 1.02f;

                // Home into the player if outside of the homing range
                if (Projectile.Distance(targetPos) > deactivateHomingRange)
                {
                    Vector2 distance = targetPos - Projectile.Center;

                    float angle = distance.ToRotation() - Projectile.velocity.ToRotation();
                    angle = MathHelper.WrapAngle(angle);
                    Projectile.velocity = Projectile.velocity.RotatedBy(angle * 0.1);
                }
                // Otherwise, deactivate homing
                else
                    HomingTimer = endHomingTime;
            }

            // Simply fly in one direction
            if (HomingTimer < endHomingTime && !Main.getGoodWorld)
            {
                if (Projectile.velocity.Length() > maxSpeed * SpeedBonus)
                    Projectile.velocity *= 0.96f;
            }

            HomingTimer--;

            base.AI();
        }
    }
}