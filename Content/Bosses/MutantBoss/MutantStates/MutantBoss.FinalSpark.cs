using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using FargowiltasSouls.Content.Projectiles;
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
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.FinalSpark)]
        public void FinalSpark() {
            ref float ai1 = ref MainAI1;
            ref float ai2 = ref MainAI2;
            ref float ai3 = ref MainAI3;
            ref float lai0 = ref MainAI4;
            ref float lai1 = ref MainAI5;
            ref float lai2 = ref MainAI6;
            ref float lai3 = ref MainAI7;

            if (--lai0 < 0) //just visual explosions
            {
                lai0 = Main.rand.Next(30);
                if (HostCheck) {
                    Vector2 spawnPos = NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height));
                    int type = ModContent.ProjectileType<MutantBombSmall>();
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPos, Vector2.Zero, type, 0, 0f, Main.myPlayer);
                }
            }

            bool harderRings = MasochistMode && ai2 >= 420 - 90;
            int ringTime = harderRings ? 100 : 120;
            if (++ai1 > ringTime) {
                ai1 = 0;

                if (HostCheck) {
                    int max = /*harderRings ? 11 :*/ 10;
                    int damage = FargoSoulsUtil.ScaledProjectileDamage(NPC.damage);
                    SpawnSphereRing(max, 6f, damage, 0.5f);
                    SpawnSphereRing(max, 6f, damage, -.5f);
                }
            }

            if (ai2 == 0) {
                if (!MasochistMode)
                    lai1 = 1;
            } else if (ai2 == 420 - 90) //dramatic telegraph
              {
                if (lai1 == 0) //maso do ordinary spark
                {
                    lai1 = 1;
                    ai2 -= 600 + 180;

                    //bias in one direction
                    ai3 -= MathHelper.ToRadians(20);

                    if (HostCheck) {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitX.RotatedBy(ai3),
                            ModContent.ProjectileType<MutantGiantDeathray2>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 0.5f), 0f, Main.myPlayer, 0, NPC.whoAmI);
                    }

                    NPC.netUpdate = true;
                } else {
                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

                    if (HostCheck) {
                        const int max = 8;
                        for (int i = 0; i < max; i++) {
                            float offset = i - 0.5f;
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, (ai3 + MathHelper.TwoPi / max * offset).ToRotationVector2(), ModContent.ProjectileType<GlowLine>(), 0, 0f, Main.myPlayer, 13f, NPC.whoAmI);
                        }
                    }
                }
            }

            if (ai2 < 420) {
                //disable it while doing maso's first ray
                if (lai1 == 0 || ai2 > 420 - 90)
                    ai3 = NPC.DirectionFrom(Player.Center).ToRotation(); //hold it here for glow line effect
            } else {
                if (!Main.dedServ)
                    ShaderManager.GetFilter("FargowiltasSouls.FinalSpark").Activate();

                if (ai1 % 3 == 0 && HostCheck) {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, 24f * Vector2.UnitX.RotatedBy(ai3), ModContent.ProjectileType<MutantEyeWavy>(), 0, 0f, Main.myPlayer,
                      Main.rand.NextFloat(0.5f, 1.25f) * (Main.rand.NextBool() ? -1 : 1), Main.rand.Next(10, 60));
                }
            }

            int endTime = 1020;
            if (MasochistMode)
                endTime += 180;
            if (ai2 == 420) {
                NPC.netUpdate = true;

                //bias it in one direction
                ai3 += MathHelper.ToRadians(20) * (MasochistMode ? 1 : -1);

                if (HostCheck) {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitX.RotatedBy(ai3),
                        ModContent.ProjectileType<MutantGiantDeathray2>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 0.5f), 0f, Main.myPlayer, 0, NPC.whoAmI);
                }
            } else if (ai2 < 300 && lai1 != 0) //charging up dust
              {
                float num1 = 0.99f;
                if (ai2 >= 60)
                    num1 = 0.79f;
                if (ai2 >= 120)
                    num1 = 0.58f;
                if (ai2 >= 180)
                    num1 = 0.43f;
                if (ai2 >= 240)
                    num1 = 0.33f;
                for (int i = 0; i < 9; ++i) {
                    if (Main.rand.NextFloat() >= num1) {
                        float f = Main.rand.NextFloat() * 6.283185f;
                        float num2 = Main.rand.NextFloat();
                        Dust dust = Dust.NewDustPerfect(NPC.Center + f.ToRotationVector2() * (110 + 600 * num2), 229, (f - 3.141593f).ToRotationVector2() * (14 + 8 * num2), 0, default, 1f);
                        dust.scale = 0.9f;
                        dust.fadeIn = 1.15f + num2 * 0.3f;
                        //dust.color = new Color(1f, 1f, 1f, num1) * (1f - num1);
                        dust.noGravity = true;
                        //dust.noLight = true;
                    }
                }
            }

            SpinLaser(MasochistMode && ai2 >= 420);

            NPC.velocity = Vector2.Zero; //prevents mutant from moving despite calling AliveCheck()
        }

        private void SpinLaser(bool useMasoSpeed) {
            ref float ai3 = ref MainAI3;

            float newRotation = NPC.DirectionTo(Main.player[NPC.target].Center).ToRotation();
            float difference = MathHelper.WrapAngle(newRotation - ai3);
            float rotationDirection = 2f * (float)Math.PI * 1f / 6f / 60f;
            rotationDirection *= useMasoSpeed ? 1.1f : 1f;
            float change = Math.Min(rotationDirection, Math.Abs(difference)) * Math.Sign(difference);
            if (useMasoSpeed) {
                change *= 1.1f;
                float angleLerp = ai3.AngleLerp(newRotation, 0.015f) - ai3;
                if (Math.Abs(MathHelper.WrapAngle(angleLerp)) > Math.Abs(MathHelper.WrapAngle(change)))
                    change = angleLerp;
            }
            ai3 += change;
        }
    }
}
