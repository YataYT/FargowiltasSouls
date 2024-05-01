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
            ref float laserTimer = ref AI1;
            ref float laserSweepDirection = ref AI2;
            ref float skullShootTimer = ref LAI0;

            // Don't move
            NPC.velocity = Vector2.Zero;

            // Choose a random direction, up or down
            if (laserSweepDirection == 0)
                laserSweepDirection = Main.rand.NextBool() ? -1 : 1;

            // Initialize with making the red telegraph
            if (AttackTimer == 0 && HostCheck) {
                int max = 7;
                for (int i = 0; i < 7; i++) {
                    Vector2 dir = Vector2.UnitX.RotatedBy(laserSweepDirection * i * MathHelper.Pi / max) * 6;
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + dir, Vector2.Zero, ModContent.ProjectileType<MutantGlowything>(),
                        0, 0f, Main.myPlayer, dir.ToRotation(), NPC.whoAmI);
                }
            }

            int laserShootingLength = 180;
            int endTime = 60 + laserShootingLength + 150;

            // Start shooting lasers within a time window
            if (AttackTimer > (MasochistMode ? 45 : 60) && AttackTimer < 60 + laserShootingLength && ++laserTimer > 10) {
                laserTimer = 0;
                if (HostCheck) {
                    float rotation = MathHelper.ToRadians(245) * laserSweepDirection / 80f;
                    int timeBeforeAttackEnds = endTime - (int)AttackTimer;

                    // Method to spawn a red laser
                    void SpawnMechRayLaser(Vector2 pos, float angleInDegrees, float turnRotation) {
                        int p = Projectile.NewProjectile(NPC.GetSource_FromThis(), pos, MathHelper.ToRadians(angleInDegrees).ToRotationVector2(),
                                        ModContent.ProjectileType<MutantDeathray3>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0, Main.myPlayer, turnRotation, NPC.whoAmI);
                        if (p != Main.maxProjectiles && Main.projectile[p].timeLeft > timeBeforeAttackEnds)
                            Main.projectile[p].timeLeft = timeBeforeAttackEnds;
                    }

                    // Spawn normal set of lasers
                    SpawnMechRayLaser(NPC.Center, 8 * laserSweepDirection + 180, rotation);
                    SpawnMechRayLaser(NPC.Center, -8 * laserSweepDirection, -rotation);

                    // Spawn another set of lasers facing the other way in maso
                    if (MasochistMode) {
                        Vector2 spawnPos = NPC.Center + laserSweepDirection * Vector2.UnitY * -1200;
                        SpawnMechRayLaser(spawnPos, 8 * laserSweepDirection + 180, rotation);
                        SpawnMechRayLaser(spawnPos, -8 * laserSweepDirection, -rotation);
                    }
                }
            }

            // Spam skeletron primes at the start
            if (AttackTimer < 180 && ++skullShootTimer > 1) {
                skullShootTimer = 0;
                float varianceInDegrees = 15;
                float rotationInDegrees = 0;

                SoundEngine.PlaySound(SoundID.Item21, NPC.Center);

                // Spawn prime skull
                if (HostCheck) {
                    float spawnOffset = (Main.rand.NextBool() ? -1 : 1) * Main.rand.NextFloat(1400, 1800);
                    float maxVariance = MathHelper.ToRadians(varianceInDegrees);
                    Vector2 aimPoint = NPC.Center - Vector2.UnitY * NPC.ai[2] * 600;
                    Vector2 spawnPos = aimPoint + spawnOffset * Vector2.UnitY.RotatedByRandom(maxVariance).RotatedBy(MathHelper.ToRadians(rotationInDegrees));
                    Vector2 vel = 32f * Vector2.Normalize(aimPoint - spawnPos);
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPos, vel, ModContent.ProjectileType<MutantGuardian>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 3f), 0f, Main.myPlayer);
                }
            }
        }
    }
}
