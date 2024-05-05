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
            ref float dashDirection = ref MainAI3;
            ref float dashType = ref MainAI0;
            ref float textureType = ref MainAI4;
            ref float endTime = ref MainAI1;

            int prepTime = 60;
            endTime = prepTime + (MasochistMode ? 60 : 120);

            // Preparation stage
            if (AttackTimer <= prepTime)
            {
                // Determine what kind of dash to do
                if (AttackTimer == 1)
                    dashType = Main.rand.Next(2);

                // Dash towards the same side you started on
                if (dashDirection == 0)
                    dashDirection = MathF.Sign(NPC.Center.X - Player.Center.X);

                Vector2 targetPos;
                // Dash a certain way
                if (dashType == 0)
                {
                    targetPos = new(Player.Center.X, Player.Center.Y + 600 * MathF.Sign(NPC.Center.Y - Player.Center.Y));
                    Movement(targetPos, 1.4f, false);
                }
                else
                {
                    targetPos = Player.Center;
                    targetPos += new Vector2(400f * dashDirection, 400f * MathF.Sign(NPC.Center.Y - targetPos.Y));
                    Movement(targetPos, 0.9f);
                }

                // Start the attack
                if (AttackTimer == prepTime || NPC.Distance(targetPos) < 64)
                {
                    AttackTimer = prepTime;

                    if (dashType == 0)
                        NPC.velocity = new(30f * dashDirection, 0);
                    else
                        NPC.velocity = new(35f * MathF.Sign(Player.Center.X - NPC.Center.X), 10f);
                }

                return;
            }

            // Slow down
            NPC.velocity *= 0.97f;

            // Initialize
            if (AttackTimer == prepTime + 1) {
                orientation = Main.rand.Next(2);
                textureType = Main.rand.Next(2);
            }

            int fishronDelay = 3;
            int maxFishronSets = MasochistMode ? 3 : 2;

            // Shoot fishrons
            if (AttackTimer % fishronDelay == 1 && AttackTimer - 1 <= prepTime + fishronDelay * maxFishronSets)
            {
                if (HostCheck)
                {
                    int projType = textureType == 0 ? ModContent.ProjectileType<MutantFishron>() : ModContent.ProjectileType<MutantShadowHand>();

                    // Spawn on both sides of the player
                    for (int i = -1; i <= 1; i += 2) {
                        int max = (AttackTimer - prepTime) / fishronDelay;
                        for (int j = -max; j <= max; j++) {
                            // Only spawn the outmost ones
                            if (MathF.Abs(j) != max)
                                continue;

                            float spread = MathHelper.Pi / 3f / (maxFishronSets + 1);
                            Vector2 offset = orientation == 0 ? Vector2.UnitY.RotatedBy(spread * j) * -450f * i : Vector2.UnitX.RotatedBy(spread * j) * 475f * i;
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, projType, FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, offset.X, offset.Y);
                        }
                    } 
                }

                // Effects
                for (int i = 0; i < 30; i++) {
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.IceTorch, Scale: 3f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].noLight = true;
                    Main.dust[d].velocity *= 12f;
                }
            }
        }
    }
}
