using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
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
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.MechRayFan)]
        public void MechRayFan() {
            ref float attackEndTime = ref MainAI0;
            ref float laserTimer = ref MainAI1;
            ref float laserSweepDirection = ref MainAI2;
            ref float attackStartTime = ref MainAI3;
            ref float skullShootTimer = ref MainAI4;

            int telegraphTime = 30;
            int prepTime = 150;

            // In masomode, skip the entire preparation stage
            if (AttackTimer == 1)
                if (MasochistMode)
                    AttackTimer = telegraphTime + 1;

            if (AttackTimer == telegraphTime)
            {
                SoundEngine.PlaySound(SoundID.ForceRoarPitched, NPC.Center);

                if (FargoSoulsUtil.HostCheck)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<Projectiles.GlowRing>(), 0, 0f, Main.myPlayer, NPC.whoAmI, NPCID.Retinazer);
            }

            Vector2 targetPos;
            if (AttackTimer < telegraphTime)
            {
                // Circle around the player briefly
                targetPos = Player.Center + NPC.DirectionFrom(Player.Center).RotatedBy(MathHelper.ToRadians(15)) * 500f;
                if (NPC.Distance(targetPos) > 50)
                    Movement(targetPos, 0.3f);
            }
            else
            {
                // DUST!!
                for (int i = 0; i < 3; i++)
                {
                    int d = Dust.NewDust(NPC.Center, 0, 0, DustID.Torch, Scale: 3f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].noLight = true;
                    Main.dust[d].velocity *= 12f;
                }

                // Move to the side of the player
                targetPos = Player.Center;
                targetPos.X += 600 * (NPC.Center.X < targetPos.X ? -1 : 1);
                Movement(targetPos, 1.2f, false);
            }

            // Prepare for the attack at the end of prep time, otherwise return early if prep isn't done yet
            if ((AttackTimer >= prepTime || MasochistMode) && NPC.Distance(targetPos) < 64 && attackStartTime == 0)
            {
                attackStartTime = AttackTimer;
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
            }
            else if (AttackTimer < prepTime)
                return;

            // Don't move
            NPC.velocity = Vector2.Zero;

            // Choose a random direction, up or down
            if (laserSweepDirection == 0)
                laserSweepDirection = Main.rand.NextBool() ? -1 : 1;

            // Initialize with making the red telegraph
            if (AttackTimer == attackStartTime + 1 && HostCheck) {
                int max = 7;
                for (int i = 0; i < 7; i++) {
                    Vector2 dir = Vector2.UnitX.RotatedBy(laserSweepDirection * i * MathHelper.Pi / max) * 6;
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + dir, Vector2.Zero, ModContent.ProjectileType<MutantGlowything>(),
                        0, 0f, Main.myPlayer, dir.ToRotation(), NPC.whoAmI);
                }
            }

            int laserStartTime = (int)attackStartTime + (MasochistMode ? 45 : 60);
            int laserShootingLength = 180;
            int laserEndTime = (int)attackStartTime + 60 + laserShootingLength;
            attackEndTime = laserEndTime + 150;

            // Start shooting lasers within a time window
            if (AttackTimer > laserStartTime && AttackTimer < laserEndTime && ++laserTimer > 10) {
                laserTimer = 0;

                if (HostCheck) {
                    float rotation = MathHelper.ToRadians(245) * laserSweepDirection / 80f;
                    int timeBeforeAttackEnds = laserEndTime - AttackTimer + (int)attackStartTime;
                    float startRotationInDegrees = 8;

                    // Method to spawn a red laser
                    void SpawnMechRayLaser(Vector2 pos, float angleInDegrees, float turnRotation) {
                        int p = Projectile.NewProjectile(NPC.GetSource_FromThis(), pos, MathHelper.ToRadians(angleInDegrees).ToRotationVector2(),
                                        ModContent.ProjectileType<MutantDeathray3>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0, Main.myPlayer, turnRotation, NPC.whoAmI);
                        if (p != Main.maxProjectiles && Main.projectile[p].timeLeft > timeBeforeAttackEnds)
                            Main.projectile[p].timeLeft = timeBeforeAttackEnds;
                    }

                    // Spawn normal set of lasers
                    SpawnMechRayLaser(NPC.Center, startRotationInDegrees * laserSweepDirection + 180, rotation);
                    SpawnMechRayLaser(NPC.Center, -startRotationInDegrees * laserSweepDirection, -rotation);

                    // Spawn another set of lasers facing the other way in maso
                    if (MasochistMode) {
                        Vector2 spawnPos = NPC.Center + laserSweepDirection * Vector2.UnitY * -1200;
                        SpawnMechRayLaser(spawnPos, startRotationInDegrees * laserSweepDirection + 180, rotation);
                        SpawnMechRayLaser(spawnPos, -startRotationInDegrees * laserSweepDirection, -rotation);
                    }
                }
            }

            int primeShootTime = 180;

            // Spam skeletron primes at the start
            if (AttackTimer < attackStartTime + primeShootTime && ++skullShootTimer > 1) {
                skullShootTimer = 0;
                float varianceInDegrees = 15;
                float rotationInDegrees = 0;

                SoundEngine.PlaySound(SoundID.Item21, NPC.Center);

                // Spawn prime skull
                if (HostCheck) {
                    float spawnOffset = (Main.rand.NextBool() ? -1 : 1) * Main.rand.NextFloat(1400, 1800);
                    float maxVariance = MathHelper.ToRadians(varianceInDegrees);
                    Vector2 aimPoint = NPC.Center - Vector2.UnitY * laserSweepDirection * 600;
                    Vector2 spawnPos = aimPoint + spawnOffset * Vector2.UnitY.RotatedByRandom(maxVariance).RotatedBy(MathHelper.ToRadians(rotationInDegrees));
                    Vector2 vel = 32f * Vector2.Normalize(aimPoint - spawnPos);
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPos, vel, ModContent.ProjectileType<MutantGuardian>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 3f), 0f, Main.myPlayer);
                }
            }
        }
    }
}
