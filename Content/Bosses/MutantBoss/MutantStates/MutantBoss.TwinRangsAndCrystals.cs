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
            ref float ai1 = ref MainAI1;
            ref float ai2 = ref MainAI2;
            ref float ai3 = ref MainAI3;
            ref float lai0 = ref MainAI4;
            ref float lai1 = ref MainAI5;
            ref float lai2 = ref MainAI6;
            ref float lai3 = ref MainAI7;

            NPC.velocity = Vector2.Zero;

            if (ai3 == 0) {
                lai0 = NPC.DirectionFrom(Player.Center).ToRotation();

                if (!MasochistMode && HostCheck) {
                    for (int i = 0; i < 4; i++) {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + Vector2.UnitX.RotatedBy(Math.PI / 2 * i) * 525, Vector2.Zero, ModContent.ProjectileType<GlowRingHollow>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, 1f);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + Vector2.UnitX.RotatedBy(Math.PI / 2 * i + Math.PI / 4) * 350, Vector2.Zero, ModContent.ProjectileType<GlowRingHollow>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, 2f);
                    }
                }
            }

            int ringDelay = MasochistMode ? 12 : 15;
            int ringMax = MasochistMode ? 5 : 4;
            if (ai3 % ringDelay == 0 && ai3 < ringDelay * ringMax) {
                if (HostCheck) {
                    float rotationOffset = MathHelper.TwoPi / ringMax * ai3 / ringDelay + lai0;
                    int baseDelay = 60;
                    float flyDelay = 120 + ai3 / ringDelay * (MasochistMode ? 40 : 50);
                    int p = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, 300f / baseDelay * Vector2.UnitX.RotatedBy(rotationOffset), ModContent.ProjectileType<MutantMark2>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, baseDelay, baseDelay + flyDelay);
                    if (p != Main.maxProjectiles) {
                        const int max = 5;
                        const float distance = 125f;
                        float rotation = MathHelper.TwoPi / max;
                        for (int i = 0; i < max; i++) {
                            float myRot = rotation * i + rotationOffset;
                            Vector2 spawnPos = NPC.Center + new Vector2(distance, 0f).RotatedBy(myRot);
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<MutantCrystalLeaf>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, Main.projectile[p].identity, myRot);
                        }
                    }
                }
            }

            if (++ai3 > 45 && --ai1 < 0) {
                NPC.netUpdate = true;
                ai1 = 20;
                ai2 = ai2 > 0 ? -1 : 1;

                SoundEngine.PlaySound(SoundID.Item92, NPC.Center);

                if (HostCheck && ai3 < 330) {
                    const float retiRad = 525;
                    const float spazRad = 350;
                    float retiSpeed = 2 * (float)Math.PI * retiRad / 300;
                    float spazSpeed = 2 * (float)Math.PI * spazRad / 180;
                    float retiAcc = retiSpeed * retiSpeed / retiRad * ai2;
                    float spazAcc = spazSpeed * spazSpeed / spazRad * -ai2;
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
