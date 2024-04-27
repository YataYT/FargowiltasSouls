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
            ref float isDashing = ref AI0;
            ref float dashTimer = ref AI1;
            ref float dashDelayTimer = ref AI2;
            ref float numDashesDone = ref AI3;

            ref float endTime = ref LAI0;
            ref float dashDelay = ref LAI1;
            ref float numDashes = ref LAI2;
            ref float dashTime = ref LAI3;

            bool IsPhaseOne = CurrentPhase == 0;

            // Spawn the spear projectile at the start of the attack
            if (AttackTimer == 1)
            {
                // Initialize values
                if (CurrentPhase == 0)
                    dashDelay = MasochistMode ? Main.rand.Next(3, 15) : 10;
                else
                    dashDelay = EternityMode ? 5 : 20;

                if (EternityMode && CurrentPhase == 1)
                    numDashes = Main.rand.Next(MasochistMode ? 3 : 5, 9);
                else
                    numDashes = 5;

                dashTime = CurrentPhase == 1 ? 20 : 30;

                if (HostCheck)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MutantSpearSpin>(),
                        FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.whoAmI, 240);
            }

            // The first few seconds of the attack is the preparation stage, and cuts off the rest of the function until the dashing is ready.
            if (AttackTimer <= 240)
            {
                Vector2 targetPos = Player.Center;
                
                // If the NPC is below the player, offset the X to prevent the boss from flying straight through the player
                if (NPC.Top.Y < Player.Bottom.Y)
                    targetPos.X += 600f * Math.Sign(NPC.Center.X - Player.Center.X);
                targetPos.Y += 400;
                Movement(targetPos, 0.7f, false);

                return;
            }

            // Slow down when not dashing
            if (isDashing == 0)
                NPC.velocity *= 0.9f;

            // Reset the delay timer and dash
            if (dashDelayTimer > dashDelay)
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
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Normalize(NPC.velocity), ModContent.ProjectileType<MutantDeathray2>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, -Vector2.Normalize(NPC.velocity), ModContent.ProjectileType<MutantDeathray2>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer);
                    }
                }
            }
            
            // Dashing
            if (isDashing > 0)
            {
                NPC.direction = NPC.spriteDirection = Math.Sign(NPC.velocity.X);

                // Reset values after dashing is over
                if (++dashTimer > dashTime)
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
 