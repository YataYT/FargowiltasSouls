using FargowiltasSouls.Core.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using Luminance.Common.StateMachines;
using static FargowiltasSouls.Core.Systems.DashManager;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public partial class MutantBoss : ModNPC
    {
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.SpearDashPredictive)]
        public void SpearDashPredictive() {
            ref float numDashes = ref MainAI0;
            ref float numDashesDone = ref MainAI1;
            ref float dashDelayTimer = ref MainAI2;
            ref float isDashing = ref MainAI3;
            ref float dashTimer = ref MainAI4;
            ref float angleTowardsPlayer = ref MainAI5;
            ref float endAttack = ref MainAI6;

            int prepTime = 180;

            // Wield a spinning spear at the start
            if (AttackTimer == 1)
            {
                if (HostCheck)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MutantSpearSpin>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.whoAmI, prepTime, -480);
            }

            // Preparation stage
            if (AttackTimer <= prepTime)
            {
                Vector2 targetPos = Player.Center;
                targetPos.Y += 400f * MathF.Sign(NPC.Center.Y - Player.Center.Y);

                // Hover above or below the player, retreat back quickly if too close
                Movement(targetPos, 0.7f, false);
                if (NPC.Distance(Player.Center) < 200)
                    Movement(NPC.Center + NPC.DirectionFrom(Player.Center), 1.4f);

                return;
            }

            // Establish max number of dashes
            if (AttackTimer == prepTime + 1)
            {
                if (EternityMode)
                    numDashes = Main.rand.Next(MasochistMode ? 3 : 5, 9);
                else
                    numDashes = 5;
            }

            int trackTime = 55;
            float trackingStrength = 30f;
            int bufferTime = 5;
            float dashSpeed = 45f;
            int dashTime = 30;
            int endTime = trackTime + bufferTime;
            // Extended time for final super dash
            if (numDashesDone == numDashes - 1)
                endTime += 20;
            // Dashes immediately on first attack and the next attack begins immediately after the final dash concludes
            if (MasochistMode && (numDashesDone == 0 || numDashesDone >= numDashes))
                endTime = 0;

            // Telegraph and preparation before each dash
            if (dashDelayTimer == 0 && isDashing == 0)
            {
                if (numDashesDone == numDashes - 1)
                {
                    // Get closer for last dash
                    if (NPC.Distance(Player.Center) > 450)
                    {
                        Movement(Player.Center, 0.6f);
                        return;
                    }

                    // Try not to bump into the player
                    NPC.velocity *= 0.75f;

                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

                    // Also wield a spear on the final dash
                    if (HostCheck)
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MutantSpearAim>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.whoAmI, trackingStrength, 80);
                }

                // Spawn telegraph
                if (numDashesDone < numDashes)
                    if (HostCheck)
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, NPC.SafeDirectionTo(Player.Center + Player.velocity * 30f), ModContent.ProjectileType<MutantDeathrayAim>(), 0, 0f, Main.myPlayer, NPC.whoAmI, trackingStrength, 60);
            }

            // Slow down when not dashing
            if (isDashing == 0)
                NPC.velocity *= 0.9f;

            // Track player up until just before dash
            if (dashDelayTimer < trackTime)
                angleTowardsPlayer = NPC.SafeDirectionTo(Player.Center + Player.velocity * 30f).ToRotation();

            if (dashDelayTimer > endTime)
            {
                isDashing = 69;
                dashDelayTimer = 0;
                numDashesDone++;

                // We do it this way for the sake of adding a delay at the end of the final dash
                if (numDashesDone > numDashes)
                {
                    endAttack++;
                    return;
                }
                else
                {
                    // If it's the final dash, input -1 to the AI as a way of telling it to use the special variant
                    float spearAI = numDashesDone == numDashes ? -2 : -1;
                    NPC.velocity = angleTowardsPlayer.ToRotationVector2() * dashSpeed;

                    if (HostCheck)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Normalize(NPC.velocity), ModContent.ProjectileType<MutantDeathray>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, -Vector2.Normalize(NPC.velocity), ModContent.ProjectileType<MutantDeathray>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MutantSpearDash>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.whoAmI, spearAI, 180);
                    }
                }

                angleTowardsPlayer = 0;
            }

            // Dashing
            if (isDashing > 0)
            {
                NPC.direction = NPC.spriteDirection = Math.Sign(NPC.velocity.X);
                dashTimer++;

                // Reset values after dashing is over
                if (dashTimer > dashTime)
                {
                    dashTimer = isDashing = 0;

                    // Kill all previous dashing projectiles
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].type == ModContent.ProjectileType<MutantSpearDash>())
                            Main.projectile[i].Kill();
                    }
                }
            }
            // If not dashing, increment the delay timer
            else
                dashDelayTimer++;
        }
    }
}
