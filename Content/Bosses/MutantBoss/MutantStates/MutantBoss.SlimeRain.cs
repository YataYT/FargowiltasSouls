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
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public partial class MutantBoss : ModNPC
    {
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.SlimeRain)]
        public void SlimeRain() {
            ref float ai1 = ref AI1;
            ref float ai2 = ref AI2;
            ref float lai0 = ref LAI0;
            ref float lai1 = ref LAI1;

            
            if (AttackTimer == 0) {
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                if (HostCheck)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MutantSlimeRain>(),
                        FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.whoAmI);
            }

            if (ai1 == 0) {
                bool first = lai0 == 0;
                lai0 = Main.rand.Next(5, 9) * 120;

                // Always start on the same side as the player
                if (first) {
                    if (Player.Center.X < NPC.Center.X && lai0 > 1200)
                        lai0 += 1200;
                    else if (Player.Center.X > NPC.Center.X && lai0 > 1200)
                        lai0 -= 1200;
                } else {
                    if (Player.Center.X < NPC.Center.X && lai0 < 1200)
                        lai0 += 1200;
                    else if (Player.Center.X > NPC.Center.X && lai0 > 1200)
                        lai0 -= 1200;
                }

                lai0 += 60;

                Vector2 basePos = NPC.Center;
                basePos.X -= 1200;

                // Spawn telegraphs
                for (int i = -360; i <= 2760; i += 120)
                {
                    if (HostCheck) {
                        if (i + 60 == (int)lai0)
                            continue;
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), basePos.X + i + 60, basePos.Y, 0f, 0f, ModContent.ProjectileType<MutantReticle>(), 0, 0f, Main.myPlayer);
                    }
                }

                if (MasochistMode) {
                    ai1 += 20; //less startup
                    ai2 += 20; //stay synced
                }
            }

            if (ai1 > 120 && ai1 % 5 == 0) //rain down slime balls
            {
                SoundEngine.PlaySound(SoundID.Item34, Player.Center);
                if (HostCheck) {
                    

                    Vector2 basePos = NPC.Center;
                    basePos.X -= 1200;
                    float yOffset = -1300;

                    const float safeRange = 110;
                    for (int i = -360; i <= 2760; i += 75) {
                        float xOffset = i + Main.rand.Next(75);
                        if (Math.Abs(xOffset - lai0) < safeRange) //dont fall over safespot
                            continue;

                        Vector2 spawnPos = basePos;
                        spawnPos.X += xOffset;
                        Vector2 velocity = Vector2.UnitY * Main.rand.NextFloat(15f, 20f);

                        SpawnSlimes(spawnPos, yOffset, velocity);
                    }

                    //spawn right on safespot borders
                    SpawnSlimes(basePos + Vector2.UnitX * (lai0 + safeRange), yOffset, Vector2.UnitY * 20f);
                    SpawnSlimes(basePos + Vector2.UnitX * (lai0 - safeRange), yOffset, Vector2.UnitY * 20f);
                }
            }

            if (++ai1 > 180)
                ai1 = 0;

            int masoMovingRainAttackTime = 180 * 3 - 60;
            if (MasochistMode && ai1 == 120 && ai2 < masoMovingRainAttackTime && Main.rand.NextBool(3))
                ai2 = masoMovingRainAttackTime;

            NPC.velocity = Vector2.Zero;

            int timeToMove = 240;

            if (MasochistMode) {
                if (ai2 == masoMovingRainAttackTime) {
                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                }

                if (ai2 > masoMovingRainAttackTime + 30) {
                    if (ai1 > 170)
                        ai1 -= 30;

                    if (lai1 == 0) {
                        float safeSpotX = NPC.Center.X - 1200f + lai0;
                        lai1 = Math.Sign(NPC.Center.X - safeSpotX);
                    }

                    NPC.Center += Vector2.UnitX * 1000f / timeToMove * lai1;
                }
            }
        }

        private void SpawnSlimes(Vector2 pos, float off, Vector2 vel) {
            ref float ai2 = ref AI2;

            // Don't flip in maso wave 3
            int flip = MasochistMode && ai2 < 180 * 2 && Main.rand.NextBool() ? -1 : 1;
            Vector2 spawnPos = pos + off * Vector2.UnitY * flip;
            float ai0 = 0;
            // float ai0 = FargoSoulsUtil.ProjectileExists(RitualProj, ModContent.ProjectileType<MutantRitual>()) == null ? 0f : NPC.Distance(Main.projectile[RitualProj].Center);
            Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPos, vel * flip * 2 /* x2 to compensate for removed extraUpdates */, ModContent.ProjectileType<MutantSlimeBall>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, ai0);
        }
    }
}
