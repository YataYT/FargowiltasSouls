using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using FargowiltasSouls.Content.Projectiles.Masomode;
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
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.PillarDunk)]
        public void PillarDunk() {
            ref float ai1 = ref MainAI1;
            ref float ai2 = ref MainAI2;
            ref float ai3 = ref MainAI3;
            ref float centerX = ref MainAI4;
            ref float centerY = ref MainAI5;
            ref float lai2 = ref MainAI6;
            ref float endTime = ref MainAI7;

            ref float lai0 = ref NPC_LAI0;

            int pillarAttackDelay = 60;
            int buildupTime = 240;
            int baseEndTime = 240 + pillarAttackDelay * 4 + 60;
            if (MasochistMode)
                baseEndTime += pillarAttackDelay * 2;
            endTime = baseEndTime;

            // Get the player confused sometime throughout the attack
            if (Main.zenithWorld && AttackTimer > 180)
                Player.confused = true;

            // Set up clones first
            if (centerX == 0 && centerY == 0)
            {
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

                // Spawn clones that'll shoot a pillar
                if (HostCheck)
                {
                    SpawnPillarDunkClone(-1, 1, pillarAttackDelay * 4);
                    SpawnPillarDunkClone(1, -1, pillarAttackDelay * 2);
                    SpawnPillarDunkClone(1, 1, pillarAttackDelay * 3);

                    // Spawn 1 more in masomode
                    if (MasochistMode)
                    {
                        SpawnPillarDunkClone(1, 1, pillarAttackDelay * 6);

                        // Spawn 2 more in GFB
                        if (Main.getGoodWorld)
                        {
                            SpawnPillarDunkClone(-1, 1, pillarAttackDelay * 7);
                            SpawnPillarDunkClone(1, -1, pillarAttackDelay * 8);
                        }
                    }
                }

                centerX = NPC.Center.X;
                centerY = NPC.Center.Y;

                // Orient the attack around the center of the arena
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<MutantArena>() && Main.projectile[i].ai[1] == NPC.whoAmI)
                    {
                        centerX = Main.projectile[i].Center.X;
                        centerY = Main.projectile[i].Center.Y;
                        break;
                    }
                }

                Vector2 offset = 1000f * Vector2.UnitX.RotatedBy(MathHelper.PiOver4);

                // Always go to a side the player isn't in, but pick a way to do it randomly
                if (Main.rand.NextBool())
                {
                    if (Player.Center.X > centerX)
                        offset.X *= -1;
                    if (Main.rand.NextBool())
                        offset.Y *= -1;
                }
                else
                {
                    if (Main.rand.NextBool())
                        offset.X *= -1;
                    if (Player.Center.Y > centerY)
                        offset.Y *= -1;
                }

                centerX = offset.Length();
                centerY = offset.ToRotation();
            }

            // Movement
            Vector2 targetPos = Player.Center;
            targetPos += new Vector2(700 * MathF.Sign(NPC.Center.X - Player.Center.X), AttackTimer < buildupTime ? 400 : 150);
            if (NPC.Distance(targetPos) > 50)
                Movement(targetPos, 1f);

            // Set the NPC's local AI so the pillars know when to despawn, along with staggered despawning afterwards
            lai0 = endTime - AttackTimer;
            lai0 += 60f + 60f * (1f - AttackTimer / endTime);

            // The boss itself will shoot first
            if (AttackTimer == pillarAttackDelay)
            {
                if (HostCheck)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitY * -5, ModContent.ProjectileType<MutantPillar>(),
                        FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 3f), 0, Main.myPlayer, 3, NPC.whoAmI);
            }
            // Shoot again in masomode
            else if (MasochistMode && AttackTimer == pillarAttackDelay * 5)
            {
                if (HostCheck)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitY * -5, ModContent.ProjectileType<MutantPillar>(),
                        FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 3f), 0, Main.myPlayer, 1, NPC.whoAmI);
            }
        }

        private void SpawnPillarDunkClone(float ai1, float ai2, float ai3)
        {
            FargoSoulsUtil.NewNPCEasy(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<MutantIllusion>(), NPC.whoAmI, NPC.whoAmI, ai1, ai2, ai3);
        }
    }
}
