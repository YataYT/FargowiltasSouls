using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using FargowiltasSouls.Core.Systems;
using Luminance.Common.StateMachines;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public partial class MutantBoss : ModNPC
    {
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.SpearTossPredictive)]
        public void SpearTossPredictive()
        {
            ref float maxThrownSpears = ref AI3;
            ref float spearsThrown = ref AI2;
            ref float spearThrowTimer = ref AI1;
            ref float currentSpearAimRotation = ref LAI0;

            float spearTrackTime = 85;              // Time to track the player before throwing spear
            float spearThrowBufferTime = 5;         // Amount of time to stop tracking the player before the throw
            float windUpTime = 60;                  // The amount of extra time at the start of the attack

            // Update values for P2
            if (CurrentPhase == 1) {
                spearTrackTime = 60;
                windUpTime = 0;
                spearThrowBufferTime = 0;
            }

            // Movement
            Vector2 targetPos = Player.Center;
            targetPos.X += 500 * (NPC.Center.X < targetPos.X ? -1 : 1);
            if (NPC.Distance(targetPos) > 50)
                Movement(targetPos, CurrentPhase == 0 ? 0.5f : 0.8f);

            // Determine how many spears to throw
            if (maxThrownSpears == 0) {
                if (CurrentPhase == 0)
                    maxThrownSpears = MasochistMode ? Main.rand.Next(2, 8) : 5;
                else {
                    if (EternityMode)
                        maxThrownSpears = Main.rand.Next((MasochistMode ? 3 : 5), 9);
                    else
                        maxThrownSpears = 5;
                }
                NPC.netUpdate = true;
            }

            // Track player until right before throw
            if (spearThrowTimer < windUpTime + spearTrackTime)
                currentSpearAimRotation = NPC.DirectionTo(Player.Center + Player.velocity * 30f).ToRotation();

            // Throw spear
            if (spearThrowTimer > windUpTime + spearTrackTime + spearThrowBufferTime) {
                spearsThrown++;
                spearThrowTimer = windUpTime;

                if (HostCheck) {
                    Vector2 vel = currentSpearAimRotation.ToRotationVector2() * 25f;
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel, ModContent.ProjectileType<MutantSpearThrown>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.target);
                    if (MasochistMode || CurrentPhase == 1) {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Normalize(vel), ModContent.ProjectileType<MutantDeathray2>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 0.8f), 0f, Main.myPlayer);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, -Vector2.Normalize(vel), ModContent.ProjectileType<MutantDeathray2>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 0.8f), 0f, Main.myPlayer);
                    }
                }

                // Reset rotation
                currentSpearAimRotation = 0;
                NPC.netUpdate = true;
            }
        }
    }
}
