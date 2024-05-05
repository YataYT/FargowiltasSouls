using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Masomode;
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
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.Phase3Transition)]
        public void Phase3Transition()
        {
            ref float ai1 = ref MainAI1;
            ref float ai2 = ref MainAI2;
            ref float ai3 = ref MainAI3;
            ref float lai0 = ref MainAI4;
            ref float lai1 = ref MainAI5;
            ref float lai2 = ref MainAI6;
            ref float endTransition = ref MainAI7;

            CurrentPhase = 3;

            DoLifeDrain = true;
            if (NPC.buffType[0] != 0)
                NPC.DelBuff(0);

            // Initial effects
            if (AttackTimer == 1)
            {
                NPC.life = NPC.lifeMax;

                DramaticTransitionPhase3();
            }

            // Screen shake
            if (AttackTimer > 60 && !Main.dedServ && Main.LocalPlayer.active)
                if (ScreenShakeSystem.OverallShakeIntensity < 7)
                    ScreenShakeSystem.SetUniversalRumble(7);

            // Random ahh roar
            if (NPC.ai[1] == 360)
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

            // Start moving towards the target position for the first attack
            if (AttackTimer > 480)
            {
                // Don't life drain, keep things synced
                DoLifeDrain = false;

                // Movement
                Vector2 targetPos = Player.Center - Vector2.UnitY * 300f;
                Movement(targetPos, 1f, true, false);

                // End the transition
                if (NPC.Distance(targetPos) < 50 || AttackTimer > 720)
                {
                    endTransition = 22;
                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                }
            }
            else
            {
                // Slow down
                NPC.velocity *= 0.9f;

                // Makes all players stop attacking
                if (Main.LocalPlayer.active && !Main.LocalPlayer.dead && !Main.LocalPlayer.ghost && NPC.Distance(Main.LocalPlayer.Center) < 3000)
                {
                    Main.LocalPlayer.controlUseItem = false;
                    Main.LocalPlayer.controlUseTile = false;
                    Main.LocalPlayer.FargoSouls().NoUsingItems = 2;
                }

                if (--lai2 < 0)
                {
                    lai2 = Main.rand.Next(15);
                    if (HostCheck)
                    {
                        Vector2 spawnPos = NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height));
                        int type = ModContent.ProjectileType<MutantBombSmall>();
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPos, Vector2.Zero, type, 0, 0f, Main.myPlayer);
                    }
                }
            }

            // Dust!!
            for (int i = 0; i < 5; i++)
            {
                int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, FargoSoulsUtil.AprilFools ? DustID.SolarFlare : DustID.Vortex, 0f, 0f, 0, default, 1.5f);
                Main.dust[d].noGravity = true;
                Main.dust[d].noLight = true;
                Main.dust[d].velocity *= 4f;
            }
        }

        private void DramaticTransitionPhase3()
        {
            NPC.velocity = Vector2.Zero;

            // Apply buffs
            Main.player[NPC.target].ClearBuff(ModContent.BuffType<MutantFangBuff>());
            Main.player[NPC.target].ClearBuff(ModContent.BuffType<AbomRebirthBuff>());

            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 1.5f }, NPC.Center);

            // Boom
            if (FargoSoulsUtil.HostCheck)
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MutantBomb>(), 0, 0f, Main.myPlayer);

            int max = 40;
            float totalAmountToHeal = Main.player[NPC.target].statLifeMax2 / 4f;

            for (int i = 0; i < max; i++)
            {
                int heal = (int)(Main.rand.NextFloat(0.9f, 1.1f) * totalAmountToHeal / max);
                Vector2 vel = Main.rand.NextFloat(2f, 18f) * -Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi);

                float projAI0 = -Main.player[NPC.target].whoAmI - 1;
                float projAI1 = vel.Length() / Main.rand.Next(90, 180); // Window in which they begin homing in
                if (HostCheck)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel, ModContent.ProjectileType<MutantHeal>(), heal, 0f, Main.myPlayer, projAI0, projAI1);
            }
        }
    }
}
