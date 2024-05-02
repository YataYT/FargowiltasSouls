using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using Luminance.Common.StateMachines;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public partial class MutantBoss : ModNPC
    {
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.VoidRays)]
        public void VoidRays() {
            ref float numberOfRevolutions = ref MainAI0;
            ref float currentRotation = ref MainAI1;
            ref float currentNumLasersShot = ref MainAI2;
            ref float currentAmountOfRevolutions = ref MainAI3;
            ref float startAttack = ref MainAI4;

            // In P1 and P2, 20 lasers per revolution. In P3, 40 lasers per revolution.
            float lasersPerRevolution = CurrentPhase <= 2 ? 20 : 40;
            // No approach time in P3
            int approachTime = CurrentPhase < 3 ? 180 : 0;

            // 2/3 cycles in P1, 3 cycles in P2, and 3/4 cycles in P3
            numberOfRevolutions = 2;
            if (CurrentPhase >= 2 || MasochistMode)
                numberOfRevolutions++;
            if (CurrentPhase == 3 && MasochistMode)
                numberOfRevolutions++;

            // Determines how much to rotate per laser. In P1 and P2, rotate by PI/10. In P3, rotate by PI/20.
            float rotationAmount = CurrentPhase <= 2 ? MathHelper.Pi / 10 : MathHelper.Pi / 20;

            // Frame delay between each laser, determining the rate of fire of lasers. This indirectly affects spacing.
            float delayBetweenLasers = CurrentPhase == 1 ? (MasochistMode ? 3 : 5) : 3; // In P1, 3/5. In P2, 3.
            if (CurrentPhase == 3)
                delayBetweenLasers = 1; // In P3, 1, aka shoot super fast

            // Approach the player first
            if (startAttack == 0)
            {
                Vector2 targetPos = Player.Center + Player.SafeDirectionTo(NPC.Center) * 250;
                if (AttackTimer < approachTime && NPC.Distance(targetPos) > 50)
                {
                    currentRotation = Player.SafeDirectionTo(NPC.Center).ToRotation();
                    Movement(targetPos, 0.5f);
                    return;
                }
                else
                {
                    startAttack = 69;
                    if (Player.Center.X < NPC.Center.X)
                        rotationAmount *= -1f;
                }
            }

            // Don't move
            NPC.velocity = Vector2.Zero;

            // Fire laser
            if (AttackTimer % delayBetweenLasers == 0) {
                currentNumLasersShot++;

                if (HostCheck)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(2, 0).RotatedBy(currentRotation),
                        ModContent.ProjectileType<MutantMark1>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer);
                currentRotation += rotationAmount;

                if (currentNumLasersShot % lasersPerRevolution == 0) {
                    NPC.netUpdate = true;
                    currentAmountOfRevolutions++;
                    // Offset the rotation by half/third after each revolution
                    currentRotation -= rotationAmount / (MasochistMode ? 3 : 2);
                }
            }
        }
    }
}
