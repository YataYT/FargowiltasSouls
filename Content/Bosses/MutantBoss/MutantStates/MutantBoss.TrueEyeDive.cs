using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using Luminance.Common.StateMachines;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
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
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.TrueEyeDive)]
        public void TrueEyeDive()
        {
            ref float direction = ref MainAI3;
            ref float numberOfEyesReleased = ref MainAI2;
            ref float endTimer = ref MainAI1;
            ref float maxEyeThreshold = ref MainAI4;
            ref float startDive = ref MainAI5;
            ref float initialDirection = ref MainAI6;

            // Fire an eye every X frames
            float eyeFireRate = 15;

            if (AttackTimer == 1)
                maxEyeThreshold = MasochistMode ? 6 : 3;

            // Prepare first
            if (startDive == 0)
            {
                if (AttackTimer == 1)
                    initialDirection = MathF.Sign(NPC.Center.X - Player.Center.X);

                if (CurrentPhase == 1)
                {
                    Vector2 targetPos = Player.Center;
                    targetPos += new Vector2(700 * initialDirection, -400f);
                    Movement(targetPos, 0.6f);
                    float dist = NPC.Distance(targetPos);
                    bool e = dist < 100;

                    // Start dive
                    if (AttackTimer >= 180 || NPC.Distance(targetPos) < 100)
                    {
                        startDive = 420;

                        NPC.velocity.X = 35f * MathF.Sign(Player.Center.X - NPC.Center.X);
                        if (NPC.velocity.Y < 0)
                            NPC.velocity.Y *= -1;
                        NPC.velocity.Y *= 0.3f;

                        SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                    }
                }
                // Phase 2
                else
                {
                    Vector2 targetPos = Player.Center + new Vector2(400 * MathF.Sign(Player.Center.X - NPC.Center.X), 400);
                    Movement(targetPos, 1.2f);

                    // Start dive
                    if (AttackTimer >= 60)
                    {
                        startDive = 69;

                        NPC.velocity.X = 30f * MathF.Sign(Player.Center.X - NPC.Center.X);
                        if (NPC.velocity.Y < 0)
                            NPC.velocity.Y *= -1;
                        NPC.velocity.Y *= 0.3f;
                    }
                }

                return;
            }

            // Get initial direction
            if (direction == 0)
                direction = Math.Sign(NPC.Center.X - Target.Center.X);
            
            // Move normally after 3 eyes released in maso, otherwise slow down
            if (numberOfEyesReleased > 3)
            {
                Vector2 targetPos = Target.Center;
                targetPos.X += NPC.Center.X < Target.Center.X ? -500 : 500;
                if (NPC.Distance(targetPos) > 50)
                    Movement(targetPos, 0.3f);
            } 
            else
            {
                NPC.velocity *= 0.99f;
            }

            // Fire an eye
            if (AttackTimer % eyeFireRate == 0)
            {
                if (++numberOfEyesReleased <= maxEyeThreshold)
                {
                    if (HostCheck)
                    {
                        int type;
                        float ratio = numberOfEyesReleased / maxEyeThreshold * 3;

                        // Which kind of eye to use
                        if (ratio <= 1f)
                            type = ModContent.ProjectileType<MutantTrueEyeDeathray>();
                        else if (ratio <= 2f)
                            type = ModContent.ProjectileType<MutantTrueEyeBolt>();
                        else
                            type = ModContent.ProjectileType<MutantTrueEyeSphere>();

                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, type, FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 0.8f), 0f, Main.myPlayer, NPC.target, direction);
                    }

                    // Effects
                    SoundEngine.PlaySound(SoundID.Item92, NPC.Center);
                    for (int i = 0; i < 30; i++)
                    {
                        int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.IceTorch, 0f, 0f, 0, default, 3f);
                        Main.dust[d].noGravity = true;
                        Main.dust[d].noLight = true;
                        Main.dust[d].velocity *= 12f;
                    }
                }
            }

            // Add buffer time
            if (numberOfEyesReleased == maxEyeThreshold)
                endTimer = AttackTimer + (eyeFireRate * (MasochistMode ? 3 : 5));
        }
    }
}
