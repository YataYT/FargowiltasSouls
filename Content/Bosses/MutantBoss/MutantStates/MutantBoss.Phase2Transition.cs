using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using FargowiltasSouls.Core.Systems;
using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
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
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.Phase2Transition)]
        public void Phase2Transition() {
            ref float ai0 = ref MainAI0;
            ref float ai1 = ref MainAI1;
            ref float ai2 = ref MainAI2;
            ref float totalTransitionTime = ref MainAI3;

            int animationTime = 240;
            int midAnimationEffectTime = 150;
            totalTransitionTime = animationTime + 30;

            CurrentPhase = 2;
            NPC.dontTakeDamage = true;

            if (NPC.buffType[0] != 0)
                NPC.DelBuff(0);

            // Initialization
            if (AttackTimer == 1)
            {
                // Clear the lowly P1 clutter
                FargoSoulsUtil.ClearAllProjectiles(2, NPC.whoAmI);

                if (EternityMode)
                {
                    // Start of transition effects (heal, sound, explosions, etc)
                    DramaticTransitionPhase2();

                    // Create mutant arena
                    if (HostCheck)
                        CurrentRitualProjectile = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero,
                            ModContent.ProjectileType<MutantArena>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, 0f, NPC.whoAmI);
                }
            }

            // Activate sky and music sceneries
            EModeSpecialEffects();

            // Screen shake
            if (AttackTimer < 60 && !Main.dedServ && Main.LocalPlayer.active)
                if (ScreenShakeSystem.OverallShakeIntensity < 7)
                    ScreenShakeSystem.SetUniversalRumble(7);

            // Makes every player nearby stop attacking during the transition
            if (AttackTimer < animationTime)
            {
                if (Main.LocalPlayer.active && !Main.LocalPlayer.dead && !Main.LocalPlayer.ghost && NPC.Distance(Main.LocalPlayer.Center) < 3000)
                {
                    Main.LocalPlayer.controlUseTile = false;
                    Main.LocalPlayer.controlUseItem = false;
                    Main.LocalPlayer.FargoSouls().NoUsingItems = 2;
                }
            }

            if (AttackTimer == midAnimationEffectTime)
            {
                // Rawr #218943895498 >.<
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

                // Spawn hollow glow ring
                if (HostCheck)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<Projectiles.GlowRingHollow>(), 0, 0f, Main.myPlayer, 5);

                // Progress towards 
                if (EternityMode && WorldSavingSystem.SkipMutantP1 <= Phase1SkipThreshold)
                {
                    WorldSavingSystem.SkipMutantP1++;
                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendData(MessageID.WorldData);
                }

                // Dust!!!!!!!!!!!!
                for (int i = 0; i < 50; i++)
                {
                    int d = Dust.NewDust(Main.LocalPlayer.position, Main.LocalPlayer.width, Main.LocalPlayer.height, FargoSoulsUtil.AprilFools ? DustID.SolarFlare : DustID.Vortex, Scale: 2.5f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].noLight = true;
                    Main.dust[d].velocity *= 9f;
                }
            }
        }

        private void DramaticTransitionPhase2()
        {
            int maxHealProjectiles = 40;
            float totalAmountToHeal = NPC.lifeMax - NPC.life + NPC.lifeMax * 0.1f;

            // Stop moving
            NPC.velocity = Vector2.Zero;

            // Most well-described Relogic variable name:
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 1.5f }, NPC.Center);

            // Bombs for effect unless skipping Phase 1
            if (HostCheck && !SkipPhase1)
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MutantBomb>(), 0, 0f, Main.myPlayer);

            for (int i = 0; i < maxHealProjectiles; i++)
            {
                // More controlled when skipping Phase 1, more chaotic on normal transition
                Vector2 vel = SkipPhase1
                    ? 0.1f * -Vector2.UnitY.RotatedBy(MathHelper.TwoPi / maxHealProjectiles * i)
                    : Main.rand.NextFloat(2f, 18f) * -Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi);
                int projHealAmount = (int)(Main.rand.NextFloat(0.9f, 1.1f) * totalAmountToHeal / maxHealProjectiles);
                float projAI0 = NPC.whoAmI;
                float projAI1 = vel.Length() / Main.rand.Next(150, 180);

                // Summon heal projectile
                if (HostCheck)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel, ModContent.ProjectileType<MutantHeal>(), projHealAmount, 0f, Main.myPlayer, projAI0, projAI1);
            }
        }
    }
}
