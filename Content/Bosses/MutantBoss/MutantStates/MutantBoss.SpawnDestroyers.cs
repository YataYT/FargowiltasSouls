using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
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
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public partial class MutantBoss : ModNPC
    {
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.SpawnDestroyers)]
        public void SpawnDestroyers() {
            // Normally this is a P2 exclusive attack, but Maso releases destroyers all at once in P1
            if (CurrentPhase == 0 && MasochistMode)
                SpawnDestroyersMasoP1();
            else
                SpawnDestroyersP2();
        }

        private void SpawnDestroyersMasoP1() {
            ref float appearance = ref LAI2;    // Make sure to assign a random number between 0-2 to LAI2 during transition!

            // Movement
            Vector2 targetPos = Player.Center;
            targetPos.X += 500 * (NPC.Center.X < targetPos.X ? -1 : 1);
            if (NPC.Distance(targetPos) > 50) {
                Movement(targetPos, NPC.localAI[3] > 0 ? 0.5f : 2f, true, NPC.localAI[3] > 0);
            }

            SoundEngine.PlaySound(SoundID.NPCDeath13, NPC.Center);

            // Spawn Calamity worm boss (worm boss (worm))
            if (HostCheck) {
                if (FargoSoulsUtil.AprilFools)
                    appearance = 0;

                // Spawn 8 worms (Did you know? Calamity adds 8 worm bosses)
                int numWorms = 8;
                for (int i = 0; i < numWorms; i++) {
                    SpawnWorm(i);
                }
            }
        }

        private void SpawnDestroyersP2() {
            ref float wormShootTimer = ref AI1;
            ref float numWorms = ref AI2;
            ref float appearance = ref LAI2;

            float wormFireRate = MasochistMode ? 15 : 30;
            int wormCap = CurrentPhase == 0 ? 8 : AdjustValueForDifficulty(7, 5, 3);

            // Movement
            if (WorldSavingSystem.EternityMode) {
                Vector2 targetPos = Player.Center + NPC.DirectionFrom(Player.Center) * 500;

                // Avoid crossing up the player
                if (Math.Abs(targetPos.X - Player.Center.X) < 150) {
                    targetPos.X = Player.Center.X + 150 * Math.Sign(targetPos.X - Player.Center.X);
                    Movement(targetPos, 0.3f);
                }
                if (NPC.Distance(targetPos) > 50) {
                    Movement(targetPos, 0.9f);
                }
            } else {
                Vector2 targetPos = Player.Center;
                targetPos.X += 500 * (NPC.Center.X < targetPos.X ? -1 : 1);
                if (NPC.Distance(targetPos) > 50) {
                    Movement(targetPos, 0.4f);
                }
            }

            // Spawn worm
            if (++wormShootTimer > wormFireRate) {
                NPC.netUpdate = true;
                wormShootTimer = 0;

                SoundEngine.PlaySound(SoundID.NPCDeath13, NPC.Center);

                if (HostCheck) {
                    SpawnWorm(numWorms++);
                }
            }
        }

        // Approved by Calamity
        private void SpawnWorm(float wormNum) {
            ref float appearance = ref LAI2;
            Vector2 vel = NPC.DirectionFrom(Player.Center).RotatedByRandom(MathHelper.ToRadians(120)) * 10f;

            // Spawn head
            float wormAI1 = (0.8f + 0.4f * wormNum / 5f) + (MasochistMode && CurrentPhase == 1 ? 0.4f : 0f);    // If Maso + P2, add 0.4f (what does AI1 do anyways)
            int current = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel, ModContent.ProjectileType<MutantDestroyerHead>(),
                FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.target, wormAI1, appearance);
            Main.projectile[current].timeLeft = 90;     // Tbh maybe just kill all the projectiles manually instead of setting timeLeft

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
