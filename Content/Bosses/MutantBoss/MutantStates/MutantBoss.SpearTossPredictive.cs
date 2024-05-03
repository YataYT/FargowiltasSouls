using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using FargowiltasSouls.Core.Systems;
using Luminance.Common.StateMachines;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public partial class MutantBoss : ModNPC
    {


        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.SpearTossPredictiveWithDestroyers)]
        public void SpearTossPredictive()
        {
            ref float throwTimer = ref MainAI1;
            ref float spearsThrown = ref MainAI2;
            ref float maxThrownSpears = ref MainAI3;
            ref float currentSpearAimRotation = ref MainAI4;
            ref float endTime = ref MainAI5;
            ref float appearance = ref MainAI6;
            ref float numWormsFired = ref MainAI7;

            // Spawn worms first
            int wormCap = AdjustValueForDifficulty(7, 5, 3);
            if (CurrentPhase == 1)
            {
                if (MasochistMode)
                    wormCap = 8;
                else
                    wormCap = 0;
            }

            // Determine how many spears to throw
            if (maxThrownSpears == 0)
            {
                if (CurrentPhase == 1)
                    maxThrownSpears = MasochistMode ? Main.rand.Next(2, 8) : 5;
                else
                {
                    if (EternityMode)
                        maxThrownSpears = Main.rand.Next((MasochistMode ? 3 : 5), 9);
                    else
                        maxThrownSpears = 5;
                }
                NPC.netUpdate = true;
            }

            if (numWormsFired < wormCap)
            {
                if (CurrentPhase == 1 && MasochistMode)
                    SpawnDestroyersMasoP1();
                else if (CurrentPhase == 2)
                    SpawnDestroyersP2();

                return;
            }

            float spearTrackTime = 85;              // Time to track the player before throwing spear
            float spearThrowBufferTime = 5;         // Amount of time to stop tracking the player before the throw
            float windUpTime = 60;                  // The amount of extra time at the start of the attack
            float trackingStrength = 30;

            // Update values for P2
            if (CurrentPhase == 2) {
                spearTrackTime = 60;
                windUpTime = 0;
                spearThrowBufferTime = 0;
            }

            // Movement
            Vector2 targetPos = Player.Center;
            targetPos.X += 500 * (NPC.Center.X < targetPos.X ? -1 : 1);
            if (NPC.Distance(targetPos) > 50)
                Movement(targetPos, CurrentPhase == 1 ? 0.5f : 0.8f);

            // Track player until right before throw
            if (throwTimer < windUpTime + spearTrackTime)
                currentSpearAimRotation = NPC.DirectionTo(Player.Center + Player.velocity * trackingStrength).ToRotation();

            // Throw spear
            if (throwTimer > windUpTime + spearTrackTime + spearThrowBufferTime && spearsThrown < maxThrownSpears) {
                spearsThrown++;
                throwTimer = windUpTime;

                if (HostCheck) {
                    Vector2 vel = currentSpearAimRotation.ToRotationVector2() * 25f;
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel, ModContent.ProjectileType<MutantSpearThrown>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.target);
                    if (MasochistMode || CurrentPhase == 2) {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Normalize(vel), ModContent.ProjectileType<MutantDeathray>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 0.8f), 0f, Main.myPlayer);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, -Vector2.Normalize(vel), ModContent.ProjectileType<MutantDeathray>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 0.8f), 0f, Main.myPlayer);
                    }
                }

                // Reset rotation
                currentSpearAimRotation = 0;
                NPC.netUpdate = true;
            }
            
            // Set the end time after all spears have been thrown
            if (spearsThrown == maxThrownSpears && endTime == 0)
                endTime = AttackTimer + spearTrackTime;

            // Spawn aim telegraphs
            if (throwTimer == windUpTime + 1 && (spearsThrown < maxThrownSpears || MasochistMode) && HostCheck)
            {
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, NPC.SafeDirectionTo(Player.Center + Player.velocity * 30f), ModContent.ProjectileType<MutantDeathrayAim>(), 0, 0f, Main.myPlayer, NPC.whoAmI, trackingStrength, spearTrackTime);
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MutantSpearAim>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.whoAmI, trackingStrength, spearTrackTime);
            }

             throwTimer++;
        }

        private void SpawnDestroyersMasoP1()
        {
            ref float appearance = ref MainAI6;
            ref float numWormsFired = ref MainAI7;

            if (AttackTimer == 1)
                appearance = Main.rand.Next(3);

            // Movement
            Vector2 targetPos = Player.Center;
            targetPos.X += 500 * (NPC.Center.X < targetPos.X ? -1 : 1);
            if (NPC.Distance(targetPos) > 50)
            {
                Movement(targetPos, NPC.localAI[3] > 0 ? 0.5f : 2f, true, NPC.localAI[3] > 0);
            }

            SoundEngine.PlaySound(SoundID.NPCDeath13, NPC.Center);

            // Spawn Calamity worm boss (worm boss (worm))
            if (HostCheck)
            {
                if (FargoSoulsUtil.AprilFools)
                    appearance = 0;

                // Spawn 8 worms (Did you know? Calamity adds 8 worm bosses)
                int numWorms = 8;
                for (int i = 0; i < numWorms; i++)
                {
                    numWormsFired++;
                    SpawnWorm(i++, numWorms);
                }
            }
        }

        private void SpawnDestroyersP2()
        {
            ref float wormShootTimer = ref MainAI1;
            ref float numWorms = ref MainAI7;
            ref float appearance = ref MainAI6;

            if (AttackTimer == 1)
                appearance = Main.rand.Next(3);

            float wormFireRate = MasochistMode ? 15 : 30;
            int wormCap = AdjustValueForDifficulty(7, 5, 3);

            // Movement
            if (EternityMode)
            {
                Vector2 targetPos = Player.Center + NPC.DirectionFrom(Player.Center) * 500;

                // Avoid crossing up the player
                if (MathF.Abs(targetPos.X - Player.Center.X) < 150)
                {
                    targetPos.X = Player.Center.X + 150 * MathF.Sign(targetPos.X - Player.Center.X);
                    Movement(targetPos, 0.3f);
                }
                if (NPC.Distance(targetPos) > 50)
                {
                    Movement(targetPos, 0.9f);
                }
            }
            else
            {
                Vector2 targetPos = Player.Center;
                targetPos.X += 500 * (NPC.Center.X < targetPos.X ? -1 : 1);
                if (NPC.Distance(targetPos) > 50)
                {
                    Movement(targetPos, 0.4f);
                }
            }

            // Spawn worm
            if (++wormShootTimer > wormFireRate)
            {
                NPC.netUpdate = true;
                wormShootTimer = 0;

                SoundEngine.PlaySound(SoundID.NPCDeath13, NPC.Center);

                if (HostCheck)
                {
                    SpawnWorm(numWorms++, wormCap);
                }
            }
        }

        // Approved by Calamity
        private void SpawnWorm(float wormNum, float wormCap)
        {
            ref float appearance = ref MainAI6;
            Vector2 vel = NPC.DirectionFrom(Player.Center).RotatedByRandom(MathHelper.ToRadians(120)) * 10f;

            // Spawn head (wormAI1 determines the speed multiplier of the worm)
            float wormAI1 = (0.8f + 0.4f * wormNum / 5f) + (MasochistMode && CurrentPhase == 2 ? 0.4f : 0f);
            int current = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel, ModContent.ProjectileType<MutantDestroyerHead>(),
                FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.target, wormAI1, appearance);
            Main.projectile[current].timeLeft = (int)(30f * (wormCap - wormNum) + 60f * MainAI3 + 30f + wormNum * 6f);

            // Spawn body segments
            int max = Main.rand.Next(8, 19);
            for (int j = 0; j < max; j++)
                current = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel, ModContent.ProjectileType<MutantDestroyerBody>(),
                    FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, Main.projectile[current].identity, 0f, appearance);

            // Spawn tail
            int previous = current;
            current = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel, ModContent.ProjectileType<MutantDestroyerTail>(),
                FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, Main.projectile[current].identity, 0f, appearance);
            Main.projectile[previous].localAI[1] = Main.projectile[current].identity;
            Main.projectile[previous].netUpdate = true;
        }
    }
}
