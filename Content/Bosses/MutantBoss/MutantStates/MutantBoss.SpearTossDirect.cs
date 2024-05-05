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
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public partial class MutantBoss : ModNPC
    {
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.SpearTossDirect)]
        public void SpearTossDirect() {
            ref float spearAttackTimer = ref MainAI1;
            ref float spearsThrown = ref MainAI2;
            ref float initialAngleOffset = ref MainAI4;
            ref float maxSpearsThrown = ref MainAI5;
            ref float rotationDirection = ref MainAI6;

            float fireRate = MasochistMode ? 15 : 30;
            float windUpTime = 150;

            // Initialization
            if (spearAttackTimer == 0) {
                // Remember initial angle offset
                initialAngleOffset = MathHelper.WrapAngle((NPC.Center - Player.Center).ToRotation());

                // Random number of tosses in Eternity, plus a random extra in Maso
                if (EternityMode)
                    maxSpearsThrown = Main.rand.Next(WorldSavingSystem.MasochistModeReal ? 3 : 5, 9) + (MasochistMode ? Main.rand.Next(6) : 0);
                else    // Only 5 outside Eternity
                    maxSpearsThrown = 5;

                // Random circle direction
                rotationDirection = Main.rand.NextBool() ? -1 : 1;
                NPC.netUpdate = true;
            }

            // Slowly rotate around the player
            Vector2 targetPos = Player.Center + 500f * Vector2.UnitX.RotatedBy(MathHelper.TwoPi / 300 * AttackTimer * rotationDirection + initialAngleOffset);
            if (NPC.Distance(targetPos) > 25)
                Movement(targetPos, 0.6f);

            // Throw spear
            if (++spearAttackTimer > windUpTime + fireRate) {
                spearAttackTimer = windUpTime;
                spearsThrown++;

                // This edge case makes things a bit convoluted, but basically end the attack shortly after the last spear is thrown.
                // In masomode however, the spear is thrown right as he transitions into the next attack
                if (spearsThrown <= maxSpearsThrown || MasochistMode)
                    SpearTossDirectAttack();
            }
            else if (spearAttackTimer == windUpTime + 1)
            {
                if (spearsThrown > 0 && (spearsThrown <= maxSpearsThrown || MasochistMode) && HostCheck)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MutantSpearAim>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.whoAmI, 0, fireRate);
            }
            else if (AttackTimer == 1)
                if (HostCheck)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MutantSpearAim>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.whoAmI, 0, windUpTime + fireRate);
        }

        private void SpearTossDirectAttack() {
            if (HostCheck) {
                Vector2 vel = NPC.DirectionTo(Player.Center) * 30f;
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Normalize(vel), ModContent.ProjectileType<MutantDeathray>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 0.8f), 0f, Main.myPlayer, 180);
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, -Vector2.Normalize(vel), ModContent.ProjectileType<MutantDeathray>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 0.8f), 0f, Main.myPlayer, 180);
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel, ModContent.ProjectileType<MutantSpearThrown>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.target);
            }
        }
    }
}
