using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using FargowiltasSouls.Core.Systems;
using Luminance.Common.StateMachines;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.SpearDashDirect)]
        public void SpearDashDirect()
        {
            ref float isDashing = ref MainAI0;
            ref float dashTimer = ref MainAI1;
            ref float dashDelayTimer = ref MainAI2;
            ref float numDashesDone = ref MainAI3;

            ref float endTime = ref MainAI4;
            ref float dashDelay = ref MainAI5;
            ref float numDashes = ref MainAI6;
            ref float dashTime = ref MainAI7;

            bool IsPhaseOne = CurrentPhase == 1;
            int startTime = 240;

            // Spawn the spear projectile at the start of the attack
            if (AttackTimer == 1)
            {
                // Initialize values
                if (CurrentPhase == 1)
                    dashDelay = MasochistMode ? Main.rand.Next(3, 15) : 10;
                else
                    dashDelay = EternityMode ? 5 : 20;

                if (EternityMode && CurrentPhase == 2)
                    numDashes = Main.rand.Next(MasochistMode ? 3 : 5, 9);
                else
                    numDashes = 5;

                dashTime = CurrentPhase == 2 ? 20 : 30;

                if (HostCheck)
                {
                    int mutantEyeDuration = (int)(startTime / 2f + (dashDelay + dashTime) * numDashes);
                    Projectile proj = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MutantSpearSpin>(),
                        FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.whoAmI, startTime, mutantEyeDuration);
                    proj.localAI[2] = CurrentPhase;
                }
            }

            // The first few seconds of the attack is the preparation stage, and cuts off the rest of the function until the dashing is ready.
            if (AttackTimer < startTime)
            {
                // Movement handling per phase
                if (CurrentPhase == 0)
                {
                    Vector2 targetPos = Player.Center;

                    // If the NPC is below the player, offset the X to prevent the boss from flying straight through the player
                    if (NPC.Top.Y < Player.Bottom.Y)
                        targetPos.X += 600f * Math.Sign(NPC.Center.X - Player.Center.X);
                    targetPos.Y += 400;
                    Movement(targetPos, 0.7f, false);
                }
                else
                {
                    Vector2 targetPos = Player.Center;
                    targetPos.Y += 450f * MathF.Sign(NPC.Center.Y - Player.Center.Y);
                    Movement(targetPos, 0.7f, false);

                    // Quickly fly away if Mutant gets too close to the player
                    if (NPC.Distance(Player.Center) < 200)
                        Movement(NPC.Center + NPC.DirectionFrom(Player.Center), 1.4f);
                }

                return;
            }

            // Slow down when not dashing
            if (isDashing == 0)
                NPC.velocity *= 0.9f;

            // Reset the delay timer and dash
            if (dashDelayTimer >= dashDelay && numDashesDone < numDashes)
            {
                NPC.netUpdate = true;
                isDashing = 69;
                dashDelayTimer = 0;
                float additionalSpeedP2 = IsPhaseOne ? 0 : 15;  // Speed boost in P2
                numDashesDone++;

                // Get crammed in one line (P1 has a tiny bit of predictiveness to it, P2 has additional speed)
                NPC.velocity = NPC.DirectionTo(Target.Center + (IsPhaseOne ? Target.Velocity : Vector2.Zero)) * ((MasochistMode ? 45f : 30f) + additionalSpeedP2);
                if (HostCheck)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MutantSpearDash>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.whoAmI);
                    if (MasochistMode || !IsPhaseOne) // Always summons deathrays in Masomode, only summons deathrays in P2 outside Maso
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Normalize(NPC.velocity), ModContent.ProjectileType<MutantDeathray>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, -Vector2.Normalize(NPC.velocity), ModContent.ProjectileType<MutantDeathray>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer);
                    }
                }
            }
            
            // Dashing
            if (isDashing > 0)
            {
                NPC.direction = NPC.spriteDirection = Math.Sign(NPC.velocity.X);
                dashTimer++;

                // Reset values after dashing is over
                if (dashTimer > dashTime)
                {
                    dashTimer = isDashing = 0;

                    // Kill all previous dashing projectiles
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].type == ModContent.ProjectileType<MutantSpearDash>())
                            Main.projectile[i].Kill();
                    }
                }
            }
            // If not dashing, increment the delay timer
            else
                dashDelayTimer++;

            // Once enough dashes have been performed, set endTime to what AttackTimer was + other timers
            if (numDashesDone >= numDashes && endTime == 0)
                endTime = AttackTimer + dashDelay + dashTime;
        }

        private void Prep()
        {

        }
    }
}
 