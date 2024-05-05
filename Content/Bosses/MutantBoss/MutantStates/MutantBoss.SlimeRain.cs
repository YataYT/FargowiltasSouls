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
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.SlimeRain)]
        public void SlimeRain() {
            ref float slimeRainTimer = ref MainAI1;
            ref float slimeRainMoveTimer = ref MainAI2;
            ref float offsetMaybe = ref MainAI4;
            ref float lai1 = ref MainAI5;
            ref float endTime = ref MainAI0;

            int prepTime = 30;

            // Preparation stage
            if (AttackTimer <= prepTime)
            {
                Vector2 targetPos = Player.Center;
                targetPos += new Vector2(700 * MathF.Sign(NPC.Center.X - targetPos.X), 200);
                Movement(targetPos, 2f);

                // Or skip the rest of the preparation stage
                if (MasochistMode && NPC.Distance(targetPos) < 64)
                    AttackTimer = prepTime;

                return;
            }

            if (AttackTimer == prepTime + 1) {
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                if (HostCheck)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MutantSlimeRain>(),
                        FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.whoAmI);
            }

            if (slimeRainTimer == 0) {
                bool first = offsetMaybe == 0;
                offsetMaybe = Main.rand.Next(5, 9) * 120;

                // Always start on the same side as the player
                if (first) {
                    if (Player.Center.X < NPC.Center.X && offsetMaybe > 1200)
                        offsetMaybe += 1200;
                    else if (Player.Center.X > NPC.Center.X && offsetMaybe > 1200)
                        offsetMaybe -= 1200;
                } else {
                    if (Player.Center.X < NPC.Center.X && offsetMaybe < 1200)
                        offsetMaybe += 1200;
                    else if (Player.Center.X > NPC.Center.X && offsetMaybe > 1200)
                        offsetMaybe -= 1200;
                }

                offsetMaybe += 60;

                Vector2 basePos = NPC.Center;
                basePos.X -= 1200;

                // Spawn telegraphs
                for (int i = -360; i <= 2760; i += 120)
                {
                    if (HostCheck) {
                        if (i + 60 == (int)offsetMaybe)
                            continue;
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), basePos.X + i + 60, basePos.Y, 0f, 0f, ModContent.ProjectileType<MutantReticle>(), 0, 0f, Main.myPlayer);
                    }
                }

                if (MasochistMode) {
                    slimeRainTimer += 20; //less startup
                    slimeRainMoveTimer += 20; //stay synced
                }
            }

            if (slimeRainTimer > 120 && slimeRainTimer % 5 == 0) //rain down slime balls
            {
                SoundEngine.PlaySound(SoundID.Item34, Player.Center);
                if (HostCheck) {
                    

                    Vector2 basePos = NPC.Center;
                    basePos.X -= 1200;
                    float yOffset = -1300;

                    const float safeRange = 110;
                    for (int i = -360; i <= 2760; i += 75) {
                        float xOffset = i + Main.rand.Next(75);
                        if (Math.Abs(xOffset - offsetMaybe) < safeRange) //dont fall over safespot
                            continue;

                        Vector2 spawnPos = basePos;
                        spawnPos.X += xOffset;
                        Vector2 velocity = Vector2.UnitY * Main.rand.NextFloat(15f, 20f);

                        SpawnSlimes(spawnPos, yOffset, velocity);
                    }

                    //spawn right on safespot borders
                    SpawnSlimes(basePos + Vector2.UnitX * (offsetMaybe + safeRange), yOffset, Vector2.UnitY * 20f);
                    SpawnSlimes(basePos + Vector2.UnitX * (offsetMaybe - safeRange), yOffset, Vector2.UnitY * 20f);
                }
            }

            if (++slimeRainTimer > 180)
                slimeRainTimer = 0;

            int masoMovingRainAttackTime = 180 * 3 - 60;
            if (MasochistMode && slimeRainTimer == 120 && slimeRainMoveTimer < masoMovingRainAttackTime && Main.rand.NextBool(3))
                slimeRainMoveTimer = masoMovingRainAttackTime;

            NPC.velocity = Vector2.Zero;

            int timeToMove = 240;

            if (MasochistMode) {
                if (slimeRainMoveTimer == masoMovingRainAttackTime) {
                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                }

                if (slimeRainMoveTimer > masoMovingRainAttackTime + 30) {
                    if (slimeRainTimer > 170)
                        slimeRainTimer -= 30;

                    if (lai1 == 0) {
                        float safeSpotX = NPC.Center.X - 1200f + offsetMaybe;
                        lai1 = Math.Sign(NPC.Center.X - safeSpotX);
                    }

                    NPC.Center += Vector2.UnitX * 1000f / timeToMove * lai1;
                }
            }

            endTime = prepTime + 180 * 3 + (MasochistMode ? timeToMove - 30 : 0);
        }

        private void SpawnSlimes(Vector2 pos, float off, Vector2 vel) {
            ref float ai2 = ref MainAI2;

            // Don't flip in maso wave 3
            int flip = MasochistMode && ai2 < 180 * 2 && Main.rand.NextBool() ? -1 : 1;
            Vector2 spawnPos = pos + off * Vector2.UnitY * flip;
            float ai0 = 0;
            // float ai0 = FargoSoulsUtil.ProjectileExists(RitualProj, ModContent.ProjectileType<MutantRitual>()) == null ? 0f : NPC.Distance(Main.projectile[RitualProj].Center);
            Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPos, vel * flip * 2 /* x2 to compensate for removed extraUpdates */, ModContent.ProjectileType<MutantSlimeBall>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, ai0);
        }
    }
}
