using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using Luminance.Common.StateMachines;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public partial class MutantBoss : ModNPC
    {
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.SpawnFishrons)]
        public void SpawnFishrons() {
            ref float orientation = ref MainAI2;
            ref float textureType = ref MainAI4;

            // Slow down
            NPC.velocity *= 0.97f;

            // Initialize
            if (AttackTimer == 0) {
                orientation = Main.rand.NextBool() ? 1 : 0;
                textureType = Main.rand.NextBool() ? 0 : 1;
            }

            int fishronDelay = 3;
            int maxFishronSets = MasochistMode ? 3 : 2;

            // Shoot fishrons
            if (AttackTimer % fishronDelay == 0 && AttackTimer <= fishronDelay * maxFishronSets) {
                if (HostCheck) {
                    int projType = textureType == 0 ? ModContent.ProjectileType<MutantFishron>() : ModContent.ProjectileType<MutantShadowHand>();

                    // Spawn on both sides of the player
                    for (int i = -1; i <= 1; i += 2) {
                        int max = (int)AttackTimer / fishronDelay;
                        for (int j = -max; j <= max; j++) {
                            // Only spawn the outmost ones
                            if (Math.Abs(i) != max)
                                continue;

                            float spread = MathHelper.Pi / 3 / (maxFishronSets + 1);
                            Vector2 offset = orientation == 0 ? Vector2.UnitY.RotatedBy(spread * j) * -450f * i : Vector2.UnitX.RotatedBy(spread * j) * 475f * i;
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, projType, FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, offset.X, offset.Y);
                        }
                    } 
                }

                // Effects
                for (int i = 0; i < 30; i++) {
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.IceTorch, 0f, 0f, 0, default, 3f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].noLight = true;
                    Main.dust[d].velocity *= 12f;
                }
            }
        }
    }
}
