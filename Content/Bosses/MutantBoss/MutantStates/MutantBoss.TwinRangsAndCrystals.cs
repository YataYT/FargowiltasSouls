using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Luminance.Common.StateMachines;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public partial class MutantBoss : ModNPC
    {
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.TwinRangsAndCrystals)]
        public void TwinRangsAndCrystals() {
            ref float rangTimer = ref MainAI1;
            ref float alternatingDirections = ref MainAI2;
            ref float endTime = ref MainAI3;
            ref float initialDirection = ref MainAI4;
            ref float lai1 = ref MainAI5;
            ref float lai2 = ref MainAI6;
            ref float prepTimeDone = ref MainAI7;

            float retirangRadius = 525;
            float spazmarangRadius = 350;
            int prepTime = 45;
            endTime = 450;
            

            // Preparation movement
            if (AttackTimer <= prepTime && prepTimeDone == 0)
            {
                Vector2 targetPos = Player.Center;
                targetPos.X += 500 * MathF.Sign(NPC.Center.X - targetPos.X);
                if (NPC.Distance(targetPos) > 50)
                    Movement(targetPos, 0.8f);

                if (AttackTimer == prepTime)
                {
                    AttackTimer = 0;
                    prepTimeDone += 69;
                }

                return;
            }

            // Don't move
            NPC.velocity = Vector2.Zero;

            // Initialize for the attack
            if (AttackTimer == 1) {
                initialDirection = NPC.DirectionFrom(Player.Center).ToRotation();

                if (!MasochistMode && HostCheck) {
                    for (int i = 0; i < 4; i++) {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + Vector2.UnitX.RotatedBy(MathHelper.PiOver2 * i) * retirangRadius, Vector2.Zero, ModContent.ProjectileType<GlowRingHollow>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, 1f);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + Vector2.UnitX.RotatedBy(MathHelper.PiOver2 * i + MathHelper.PiOver4) * spazmarangRadius, Vector2.Zero, ModContent.ProjectileType<GlowRingHollow>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, 2f);
                    }
                }
            }

            int ringDelay = MasochistMode ? 12 : 15;
            int ringMax = MasochistMode ? 5 : 4;
            if (AttackTimer % ringDelay == 0 && AttackTimer <= ringDelay * ringMax) {
                if (HostCheck) {
                    float rotationOffset = MathHelper.TwoPi / ringMax * AttackTimer / ringDelay + initialDirection + MathHelper.Pi;
                    int baseDelay = 60;
                    float flyDelay = 120 + AttackTimer / ringDelay * (MasochistMode ? 40 : 50);
                    int p = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, 300f / baseDelay * Vector2.UnitY.RotatedBy(rotationOffset), ModContent.ProjectileType<MutantMark2>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, baseDelay, baseDelay + flyDelay);
                    if (p != Main.maxProjectiles) {
                        const int leafMax = 5;
                        const float leafDistanceOffset = 125f;
                        float rotation = MathHelper.TwoPi / leafMax;
                        for (int i = 0; i < leafMax; i++) {
                            float myRot = rotation * i + rotationOffset;
                            Vector2 spawnPos = NPC.Center + new Vector2(leafDistanceOffset, 0f).RotatedBy(myRot);
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<MutantCrystalLeaf>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, Main.projectile[p].identity, myRot);
                        }
                    }
                }
            }

            if (AttackTimer > 45 && --rangTimer < 0) {
                rangTimer = 20;
                alternatingDirections = alternatingDirections > 0 ? -1 : 1;

                SoundEngine.PlaySound(SoundID.Item92, NPC.Center);

                if (HostCheck && AttackTimer < 330) {
                    const float retiRad = 525;
                    const float spazRad = 350;
                    float retiSpeed = 2 * (float)Math.PI * retiRad / 300;
                    float spazSpeed = 2 * (float)Math.PI * spazRad / 180;
                    float retiAcc = retiSpeed * retiSpeed / retiRad * alternatingDirections;
                    float spazAcc = spazSpeed * spazSpeed / spazRad * -alternatingDirections;
                    float rotationOffset = MasochistMode ? MathHelper.PiOver4 : 0;
                    for (int i = 0; i < 4; i++) {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitX.RotatedBy(Math.PI / 2 * i + rotationOffset) * retiSpeed, ModContent.ProjectileType<MutantRetirang>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, retiAcc, 300);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitX.RotatedBy(Math.PI / 2 * i + Math.PI / 4 + rotationOffset) * spazSpeed, ModContent.ProjectileType<MutantSpazmarang>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, spazAcc, 180);
                    }
                }
            }
        }
    }
}
