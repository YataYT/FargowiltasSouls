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

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public partial class MutantBoss : ModNPC
    {
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.SpearDashPredictive)]
        public void SpearDashPredictive() {
            if (NPC.localAI[1] == 0) //max number of attacks
            {
                if (WorldSavingSystem.EternityMode)
                    NPC.localAI[1] = Main.rand.Next(WorldSavingSystem.MasochistModeReal ? 3 : 5, 9);
                else
                    NPC.localAI[1] = 5;
            }

            if (NPC.ai[1] == 0) //telegraph
            {
                if (NPC.ai[2] == NPC.localAI[1] - 1) {
                    if (NPC.Distance(Player.Center) > 450) //get closer for last dash
                    {
                        Movement(Player.Center, 0.6f);
                        return;
                    }

                    NPC.velocity *= 0.75f; //try not to bump into player
                }
                if (NPC.ai[2] < NPC.localAI[1]) {
                    if (FargoSoulsUtil.HostCheck)
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, NPC.DirectionTo(Player.Center + Player.velocity * 30f), ModContent.ProjectileType<MutantDeathrayAim>(), 0, 0f, Main.myPlayer, 55, NPC.whoAmI);

                    if (NPC.ai[2] == NPC.localAI[1] - 1) {
                        SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

                        if (FargoSoulsUtil.HostCheck)
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MutantSpearAim>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.whoAmI, 4);
                    }
                }
            }

            NPC.velocity *= 0.9f;

            if (NPC.ai[1] < 55) //track player up until just before dash
            {
                NPC.localAI[0] = NPC.DirectionTo(Player.Center + Player.velocity * 30f).ToRotation();
            }

            int endTime = 60;
            if (NPC.ai[2] == NPC.localAI[1] - 1)
                endTime = 80;
            if (WorldSavingSystem.MasochistModeReal && (NPC.ai[2] == 0 || NPC.ai[2] >= NPC.localAI[1]))
                endTime = 0;
            if (++NPC.ai[1] > endTime) {
                NPC.netUpdate = true;
                NPC.ai[1] = 0;
                NPC.ai[3] = 0;
                NPC.velocity = NPC.localAI[0].ToRotationVector2() * 45f;
                float spearAi = 0f;
                if (NPC.ai[2] == NPC.localAI[1])
                    spearAi = -2f;

                if (FargoSoulsUtil.HostCheck) {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Normalize(NPC.velocity), ModContent.ProjectileType<MutantDeathray2>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer);
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, -Vector2.Normalize(NPC.velocity), ModContent.ProjectileType<MutantDeathray2>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer);
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MutantSpearDash>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.whoAmI, spearAi);
                }
                NPC.localAI[0] = 0;
            }
        }
    }
}
