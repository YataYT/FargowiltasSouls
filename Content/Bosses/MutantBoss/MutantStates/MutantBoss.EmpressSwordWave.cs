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
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.EmpressSwordWave)]
        public void EmpressSwordWave() {
            ref float swordBarrageTimer = ref AI2;
            ref float rotation = ref AI3;
            ref float playerX = ref LAI0;
            ref float playerY = ref LAI1;

            // Don't move
            NPC.velocity = Vector2.Zero;

            // Amount of time in-between sword walls
            int attackThreshold = MasochistMode ? 48 : 60;

            // Number of sword wall barrages
            int timesToAttack = 4;

            // Amount of time to start up
            int startUp = 90;

            // Initialization
            if (AttackTimer == 0) {
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            }

            // Summon sword wave
            if (AttackTimer >= startUp && AttackTimer < startUp + attackThreshold * timesToAttack && --swordBarrageTimer < 0) {
                // Reset sword attack timer
                swordBarrageTimer = attackThreshold;

                SoundEngine.PlaySound(SoundID.Item163, Player.Center);

                // uhhhhhhhh
                if (MathF.Abs(MathHelper.WrapAngle(NPC.DirectionFrom(Player.Center).ToRotation() - rotation)) > MathHelper.PiOver2)
                    rotation += MathHelper.Pi;

                int maxHorizSpread = 1600 * 2;
                int arenaRadius = 1200;
                int max = MasochistMode ? 16 : 12;
                float gap = maxHorizSpread / max;

                float attackAngle = rotation;
                Vector2 spawnOffset = -attackAngle.ToRotationVector2();
                Vector2 focusPoint = Player.Center;
                Vector2 home = NPC.Center;

                for (float i = 0; i < arenaRadius; i += gap) {
                    Vector2 newFocusPoint = focusPoint + gap * attackAngle.ToRotationVector2();
                    if ((home - newFocusPoint).Length() > (home - focusPoint).Length())
                        break;
                    focusPoint = newFocusPoint;
                }

                float spawnDistance = 0;
                while (spawnDistance < arenaRadius)
                    spawnDistance += gap;

                float mirrorLength = 2f * (float)Math.Sqrt(2f * spawnDistance * spawnDistance);
                int swordCounter = 0;

                for (int i = -max; i <= max; i++) {
                    Vector2 spawnPos = focusPoint + spawnOffset * spawnDistance + spawnOffset.RotatedBy(MathHelper.PiOver2) * gap * i;
                    float swordAI1 = swordCounter++ / (max * 2f + 1);

                    Vector2 randomOffset = Main.rand.NextVector2Unit();
                    if (WorldSavingSystem.MasochistModeReal) {
                        if (randomOffset.Length() < 0.5f)
                            randomOffset = 0.5f * randomOffset.SafeNormalize(Vector2.UnitX);
                        randomOffset *= 2f;
                    }

                    SpawnEmpressSword(spawnPos, attackAngle + MathHelper.PiOver4, swordAI1, randomOffset);
                    SpawnEmpressSword(spawnPos, attackAngle - MathHelper.PiOver4, swordAI1, randomOffset);

                    if (MasochistMode) {
                        SpawnEmpressSword(spawnPos + mirrorLength * (attackAngle + MathHelper.PiOver4).ToRotationVector2(), attackAngle + MathHelper.PiOver4 + MathHelper.Pi, swordAI1, randomOffset);
                        SpawnEmpressSword(spawnPos + mirrorLength * (attackAngle - MathHelper.PiOver4).ToRotationVector2(), attackAngle - MathHelper.PiOver4 + MathHelper.Pi, swordAI1, randomOffset);
                    }
                }

                rotation += MathHelper.PiOver4 * (Main.rand.NextBool() ? 1 : -1) + Main.rand.NextFloat(MathHelper.PiOver4 / 2) * (Main.rand.NextBool() ? 1 : -1);

                NPC.netUpdate = true;
            }

            int swordSwarmTime = startUp + attackThreshold * timesToAttack + 40;
            if (AttackTimer == swordSwarmTime) {
                MegaSwordSwarm(Player.Center);
                playerX = Player.Center.X;
                playerY = Player.Center.Y;
            }

            if (MasochistMode && AttackTimer == swordSwarmTime + 30) {
                for (int i = 0; i <= 1; i++)
                    MegaSwordSwarm(new Vector2(playerX, playerY) + 600 * i * rotation.ToRotationVector2());
            }
        }

        private void SpawnEmpressSword(Vector2 pos, float ai0, float ai1, Vector2 vel) {
            if (HostCheck) {
                Projectile.NewProjectile(NPC.GetSource_FromThis(), pos - vel * 60f, vel,
                    ProjectileID.FairyQueenLance, FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, ai0, ai1);
            }
        }

        private void MegaSwordSwarm(Vector2 target) {
            SoundEngine.PlaySound(SoundID.Item164, Player.Center);

            float safeAngle = NPC.ai[3];
            float safeRange = MathHelper.ToRadians(10);
            int max = 60;
            for (int i = 0; i < max; i++) {
                float rotationOffset = Main.rand.NextFloat(safeRange, MathHelper.Pi - safeRange);
                Vector2 offset = Main.rand.NextFloat(600f, 2400f) * (safeAngle + rotationOffset).ToRotationVector2();
                if (Main.rand.NextBool())
                    offset *= -1;

                Vector2 spawnPos = target + offset;
                Vector2 vel = (target - spawnPos) / 60f;
                SpawnEmpressSword(spawnPos, vel.ToRotation(), (float)i / max, -vel * 0.75f);
            }
        }
    }
}
