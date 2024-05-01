using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Content.Projectiles.Masomode;
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
using Terraria.Social.WeGame;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public partial class MutantBoss : ModNPC
    {
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.MutantSword)]
        public void MutantSword() {
            ref float swordsToSwing = ref AI1;
            ref float swingAngleLength = ref AI2;
            ref float currentSwingAngle = ref AI3;
            ref float endOfLastSwingTime = ref LAI0;
            ref float direction = ref LAI1;
            ref float currentSwordsSwung = ref LAI2;
            ref float swingChargeValue = ref LAI3;

            // Alternates direction on 2nd swing in Phase 2
            float swingDirection = currentSwordsSwung % 2 == 1 ? -1 : 1;
            int preparationTime = 30;
            int swordSpacing = 80;
            int swordMax = 12;
            int swingTime = 20;
            int endTime = 90;
            float swingAngleMultiplier = -4f;    // The swing angle is calculated as PI/4 * swingAngleMultiplier. This value should be negative!
            int totalTime = swingTime + endTime;
            swordsToSwing = MasochistMode ? 2 : 1;

            // Disable dodge in P1
            if (CurrentPhase == 0 && Main.LocalPlayer.active && NPC.Distance(Main.LocalPlayer.Center) < 3000f && Main.expertMode)
                Main.LocalPlayer.AddBuff(ModContent.BuffType<PurgedBuff>(), 2);

            // First, move in range of the player so they can see
            if (swingAngleLength == 0)
            {
                Vector2 targetPos = Player.Center + new Vector2(420 * MathF.Sign(NPC.Center.X - Player.Center.X), -210 * swingDirection);
                Movement(targetPos, 1.2f);

                // If it's been long enough (ignored in Masomode) and the NPC is close enough to the target position, wield the sword
                if ((AttackTimer > preparationTime + endOfLastSwingTime || MasochistMode) && NPC.Distance(targetPos) < 64)
                {
                    // Stop moving
                    NPC.velocity = Vector2.Zero;

                    // Rawr >.<
                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

                    // Determine swing angles, as well as set swingAngleLength so we know the sword is already spawned
                    direction = MathF.Sign(Player.Center.X - NPC.Center.X);
                    float startAngle = MathHelper.PiOver4 * -direction;
                    swingAngleLength = startAngle * swingAngleMultiplier / swingTime * swingDirection;
                    if (swingDirection < 0)
                        startAngle += MathHelper.PiOver2 * -direction;

                    // Make the sword
                    if (HostCheck)
                    {
                        Vector2 offset = Vector2.UnitY.RotatedBy(startAngle) * -swordSpacing;

                        for (int i = 0; i < swordMax; i++)
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + offset * i, Vector2.Zero, ModContent.ProjectileType<MutantSword>(),
                                FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 3f), 0f, Main.myPlayer, NPC.whoAmI, swordSpacing * i, totalTime);

                        for (int i = -1; i <= 1; i += 2)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + offset.RotatedBy(MathHelper.ToRadians(26.5f * i)), Vector2.Zero, ModContent.ProjectileType<MutantSword>(),
                                FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 3f), 0f, Main.myPlayer, NPC.whoAmI, 60 * 3, totalTime);

                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + offset.RotatedBy(MathHelper.ToRadians(40f * i)), Vector2.Zero, ModContent.ProjectileType<MutantSword>(),
                                FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 3f), 0f, Main.myPlayer, NPC.whoAmI, 60 * 4, totalTime);
                        }

                    }

                    return;
                }
                // Otherwise don't do anything more beyond this point
                else
                    return;
            }

            // This is the intermediary phase where the sword is spawned, but not swung yet
            if (swingAngleLength != 0 && swingChargeValue <= endTime)
            {
                NPC.velocity = Vector2.Zero;
                NPC.direction = NPC.spriteDirection = MathF.Sign(direction);

                FancyFireballs((int)(swingChargeValue / endTime * 60f));

                if (swingChargeValue == endTime)
                {
                    // Set the velocity towards the player. This will not be touched during the duration of the swing.
                    Vector2 targetPos = Player.Center - Vector2.UnitX * 300 * swingAngleLength;
                    NPC.velocity = (targetPos - NPC.Center) / swingTime;
                    endOfLastSwingTime = AttackTimer + swingTime;
                }

                swingChargeValue++;
            }
            else
            {
                currentSwingAngle += swingAngleLength;
                NPC.direction = NPC.spriteDirection = MathF.Sign(direction);

                // Swing impact effects
                if (AttackTimer == endOfLastSwingTime)
                {
                    // Screen shake
                    if (!Main.dedServ && Main.LocalPlayer.active)
                        ScreenShakeSystem.StartShake(15, shakeStrengthDissipationIncrement: 15f / 30);

                    // Moon chain explosions
                    if (EternityMode && CurrentPhase == 1 || MasochistMode)
                    {
                        SoundEngine.PlaySound(SoundID.Thunder with { Pitch = -0.5f }, NPC.Center);

                        float lookSign = MathF.Sign(direction);
                        float arcSign = MathF.Sign(swingAngleLength);
                        Vector2 offset = lookSign * Vector2.UnitX.RotatedBy(MathHelper.PiOver4 * arcSign);
                        float length = swordSpacing * swordMax / 2f;
                        Vector2 spawnPos = NPC.Center + length * offset;
                        Vector2 baseDirection = Player.DirectionFrom(spawnPos);

                        int max = 8;
                        for (int i = 0; i < max; i++)
                        {
                            Vector2 angle = baseDirection.RotatedBy(MathHelper.TwoPi / max * i);
                            float projAI1 = (i <= 2 || i == max - 2) ? 48 : 24;
                            if (HostCheck)
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPos + Main.rand.NextVector2Circular(NPC.width / 2, NPC.height / 2), Vector2.Zero,
                                    FargoSoulsUtil.AprilFools ? ModContent.ProjectileType<MoonLordSunBlast>() : ModContent.ProjectileType<MoonLordMoonBlast>(),
                                    FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 3f), 0f, Main.myPlayer, MathHelper.WrapAngle(angle.ToRotation()), projAI1);
                        }
                    }
                }

                // Reset values for next swing
                if (AttackTimer > endOfLastSwingTime + 5 && currentSwordsSwung < swordsToSwing)
                {
                    // Increment the amount of swords swung
                    currentSwordsSwung++;
                    endOfLastSwingTime = AttackTimer;
                    swingAngleLength = 0f;
                    currentSwingAngle = 0f;
                    swingChargeValue = 0f;
                    direction = 0f;
                }
            }
        }

        private void FancyFireballs(int repeats)
        {
            float modifier = 0;
            for (int i = 0; i < repeats; i++)
                modifier = MathHelper.Lerp(modifier, 1f, 0.08f);

            float distance = 1600 * (1f - modifier);
            float rotation = MathHelper.TwoPi * modifier;
            int max = 6;
            for (int i = 0; i < max; i++)
            {
                int d = Dust.NewDust(NPC.Center + distance * Vector2.UnitX.RotatedBy(rotation + MathHelper.TwoPi / max * i), 0, 0, FargoSoulsUtil.AprilFools ? DustID.SolarFlare : DustID.Vortex,
                    NPC.velocity.X * 0.3f, NPC.velocity.Y * 0.3f, newColor: Color.White);
                Main.dust[d].noGravity = true;
                Main.dust[d].scale = 6f - 4f * modifier;
            }
        }
    }
}
