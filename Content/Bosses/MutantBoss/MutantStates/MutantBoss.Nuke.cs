using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public partial class MutantBoss : ModNPC
    {
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.Nuke)]
        public void Nuke()
        {
            ref float endTime = ref MainAI0;
            ref float ai1 = ref MainAI1;
            ref float ai2 = ref MainAI2;
            ref float ai3 = ref MainAI3;
            ref float ai4 = ref MainAI4;
            ref float ai5 = ref MainAI5;

            int prepTime = 60;

            // Preparation stage
            if (AttackTimer <= prepTime)
            {
                Vector2 targetPos = Player.Center;
                targetPos += new Vector2(400 * MathF.Sign(NPC.Center.X - Player.Center.X), -400);
                Movement(targetPos, 1.2f, false);

                if (AttackTimer == prepTime)
                {
                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

                    if (HostCheck)
                    {
                        float gravity = 0.2f;
                        float time = MasochistMode ? 120f : 180f;
                        Vector2 distance = Player.Center - NPC.Center;
                        distance.X /= time;
                        distance.Y = distance.Y / time - 0.5f * gravity * time;
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, distance, ModContent.ProjectileType<MutantNuke>(), MasochistMode ? FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 3f) : 0, 0f, Main.myPlayer, gravity);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MutantFishronRitual>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 3f), 0f, Main.myPlayer, NPC.whoAmI);
                    }

                    if (MathF.Sign(Player.Center.X - NPC.Center.X) == MathF.Sign(NPC.velocity.X))
                        NPC.velocity.X *= -1f;
                    if (NPC.velocity.Y < 0)
                        NPC.velocity.Y *= -1f;
                    NPC.velocity = NPC.velocity.SafeNormalize(Vector2.One) * 3f;
                }

                return;
            }

            // Movement
            Vector2 targetShiftPos = NPC.Bottom.Y < Player.Top.Y
                ? Player.Center + 300f * Vector2.UnitX * MathF.Sign(NPC.Center.X - Player.Center.X)
                : NPC.Center + 30 * NPC.DirectionFrom(Player.Center).RotatedBy(MathHelper.ToRadians(60) * MathF.Sign(Player.Center.X - NPC.Center.X));
            Movement(targetShiftPos, 0.1f);
            int maxSpeed = MasochistMode ? 3 : 2;
            if (NPC.velocity.Length() > maxSpeed)
                NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * maxSpeed;

            int chargeUpTime = prepTime + (MasochistMode ? 120 : 180);
            float safetyRadius = 150;
            endTime = prepTime + 360;

            // Start blowing up the entire world outside the safety bubble
            if (AttackTimer > chargeUpTime)
            {
                if (!Main.dedServ && Main.LocalPlayer.active)
                    if (ScreenShakeSystem.OverallShakeIntensity < 7)
                        ScreenShakeSystem.SetUniversalRumble(7);

                // Spawn explosions
                if (HostCheck)
                {
                    int explosionFrequency = 3;
                    float safeRange = safetyRadius + 200;
                    Vector2 safeZone = NPC.Center;
                    safeZone.Y -= 100;
                    for (int i = 0; i < explosionFrequency; i++)
                    {
                        Vector2 spawnPos = NPC.Center + Main.rand.NextVector2Circular(1200, 1200);

                        // If the explosion spot is in the safe range, extend it out
                        if (Vector2.Distance(safeZone, spawnPos) < safeRange)
                        {
                            Vector2 directionOut = (spawnPos - safeZone).SafeNormalize(Vector2.Zero);
                            spawnPos = safeZone + directionOut * Main.rand.NextFloat(safeRange, 1200);
                        }

                        Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<MutantBomb>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 3f), 0f, Main.myPlayer, 0, 3);
                    }
                }
            }

            // Spawns the dust around the ritual ring
            if (AttackTimer > 45)
            {
                for (int i = 0; i < 20; i++)
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    Vector2 offset = new(MathF.Sin(angle) * 150f, MathF.Cos(angle) * 150f - 100);
                    Dust dust = Dust.NewDustPerfect(NPC.Center + offset - new Vector2(4, 4), FargoSoulsUtil.AprilFools ? DustID.SolarFlare : DustID.Vortex, null, 100, Color.White, 1.5f);
                    dust.velocity = NPC.velocity;
                    if (Main.rand.NextBool(3))
                        dust.velocity += Vector2.Normalize(offset) * 5f;
                    dust.noGravity = true;
                }
            }
        }
    }
}
