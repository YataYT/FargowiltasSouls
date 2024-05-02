using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Core.Systems;
using Luminance.Common.StateMachines;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Humanizer.On;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public partial class MutantBoss : ModNPC
    {
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.BoundaryBulletHell)]
        public void BoundaryBulletHell()
        {
            ref float endTime = ref MainAI0;
            ref float ai1 = ref MainAI1;
            ref float ai2 = ref MainAI2;
            ref float currentRotation = ref MainAI3;
            ref float direction = ref MainAI4;
            ref float lai1 = ref MainAI5;
            ref float lai2 = ref MainAI6;
            ref float lai3 = ref MainAI7;

            int fireRate = 3;
            float rotationVelocity = MathHelper.Pi / 77f;
            int eyesPerShot = 4;
            endTime = MasochistMode ? 360 : 240;
            int pauseAtStart = 0;
            float eyeSpeed = 7f;

            // Initialization
            if (AttackTimer == 1)
            {
                direction = Math.Sign(NPC.Center.X - Player.Center.X);

                if (CurrentPhase == 2 && MasochistMode)
                    currentRotation = Main.rand.NextFloat(MathHelper.TwoPi);

                if (CurrentPhase == 2)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<GlowRing>(), 0, 0f, Main.myPlayer, NPC.whoAmI, -2);
            }

            // Different rotational velocity in Maso P1
            if (CurrentPhase == 1 && MasochistMode)
                rotationVelocity = MathHelper.Pi / 3840 * (AttackTimer - 300) * direction;

            // Phase 1 changes
            if (CurrentPhase == 1)
            {
                rotationVelocity = MathHelper.Pi / 8 / 480 * AttackTimer * direction;
                pauseAtStart = 60;
                endTime = 360 + pauseAtStart;
                eyeSpeed = 6;

                if (EternityMode)
                    eyesPerShot++;
                if (MasochistMode)
                    eyesPerShot++;
            }

            // Phase 2 changes
            if (CurrentPhase == 2)
            {
                fireRate = 4;
                eyesPerShot = MasochistMode ? 10 : 8;
                rotationVelocity = MathHelper.Pi / 2100 * AttackTimer * direction * (MasochistMode ? 2 : 1);
                eyeSpeed = 6;
                endTime = 360;

                if (MasochistMode)
                    endTime += 360;
            }

            // Stop moving
            NPC.velocity = Vector2.Zero;

            // Fire eyes
            if (AttackTimer % fireRate == 0 && AttackTimer > pauseAtStart)
            {
                SoundEngine.PlaySound(SoundID.Item12, NPC.Center);
                currentRotation += rotationVelocity;
                currentRotation = MathHelper.WrapAngle(currentRotation);

                if (HostCheck)
                {
                    for (int i = 0; i < eyesPerShot; i++)
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitY.RotatedBy(currentRotation + MathHelper.TwoPi / eyesPerShot * i) * -eyeSpeed,
                            ModContent.ProjectileType<MutantEye>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer);
                }
            }
        }
    }
}
