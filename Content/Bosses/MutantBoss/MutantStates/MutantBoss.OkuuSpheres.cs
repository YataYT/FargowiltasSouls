using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Core.Systems;
using Luminance.Common.StateMachines;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.OkuuSpheres)]
        public void OkuuSpheres()
        {
            if (CurrentPhase == 1)
                OkuuSpheresP1();
            else
                OkuuSpheresP2AndP3();
        }

        public void OkuuSpheresP1()
        {
            ref float sphereRingsShot = ref MainAI2;
            ref float endTime = ref MainAI7;

            float masoSpeedBuff = MasochistMode ? 3 : 1;
            int max = MasochistMode ? 9 : 6;
            float speed = MasochistMode ? 10 : 9;
            int sign = MasochistMode ? (sphereRingsShot % 2 == 0 ? 1 : -1) : 1;
            int baseFireRate = 90;
            int baseNumRings = 4;
            endTime = baseFireRate * baseNumRings + 1;
            if (MasochistMode)  // Wait a bit longer in Masomode
                endTime = baseFireRate * baseNumRings * 1.5f + 1;

            // Don't move only if in Maso
            if (MasochistMode)
                NPC.velocity = Vector2.Zero;
            else
                NPC.velocity = NPC.SafeDirectionTo(Player.Center) * 2f;

            // Spawn sphere rings
            if (AttackTimer % (int)(baseFireRate / masoSpeedBuff) == 1)
            {
                sphereRingsShot++;

                if (sphereRingsShot <= baseNumRings * masoSpeedBuff)
                {
                    SpawnSphereRing(max, speed, (int)(0.8 * FargoSoulsUtil.ScaledProjectileDamage(NPC.damage)), 1f * sign);
                    SpawnSphereRing(max, speed, (int)(0.8 * FargoSoulsUtil.ScaledProjectileDamage(NPC.damage)), -0.5f * sign);
                }
            }
        }

        public void OkuuSpheresP2AndP3()
        {
            ref float endTime = ref MainAI7;
            ref float attackDirection = ref MainAI2;
            ref float currentRotation = ref MainAI3;

            // Experimental
            float rotationSpeed = 1f;

            // Variables
            int pauseAtStart = 180;
            int pauseBeforeFiring = 60 + pauseAtStart;
            int shootTime = 360 + pauseBeforeFiring;
            int pauseAtEnd = 60;
            int fireRate = 10;
            int totalAttackTime = shootTime + pauseAtEnd;
            float ringRotation = MathHelper.ToRadians(60) * (currentRotation - 45) / 240;
            int ringMax = MasochistMode ? 10 : 9;
            float ringSpeed = MasochistMode ? 11f : 10f;
            float ringRotationModifier = 1f;

            endTime = totalAttackTime;

            // Change some values in the desperation phase version of the attack
            if (CurrentPhase == 3)
            {
                pauseAtEnd += 60;
                ringMax = MasochistMode ? 11 : 10;
                ringSpeed = MasochistMode ? 11 : 10;
                ringRotationModifier = 0.75f;
                pauseAtStart = 0;
            }

            // Initialize variables
            if (AttackTimer == 1 + pauseAtStart)
            {
                attackDirection = Main.rand.NextBool() ? 1 : -1;
                currentRotation = Main.rand.NextFloat(MathHelper.TwoPi);

                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                if (HostCheck && CurrentPhase == 2)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<GlowRing>(), 0, 0f, Main.myPlayer, NPC.whoAmI, -2);
            }

            // Hover somewhere from the player for the first few seconds before doing anything else (only in Phase 2)
            if (CurrentPhase == 2 && AttackTimer < pauseAtStart)
            {
                Vector2 targetPos = Player.Center + Player.SafeDirectionTo(NPC.Center) * 450f;
                Movement(targetPos, 0.8f);

                // When the NPC is close enough, proceed right away
                if (NPC.Distance(targetPos) < 50)
                    AttackTimer = pauseAtStart;

                return;
            }

            // Fire rings of spheres
            if (AttackTimer % fireRate == 0 && AttackTimer > pauseBeforeFiring && AttackTimer < shootTime)
            {
                SpawnSphereRing(ringMax, ringSpeed, FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), -ringRotationModifier, ringRotation);
                SpawnSphereRing(ringMax, ringSpeed, FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), ringRotationModifier, ringRotation);
            }

            // Increase the rotation
            currentRotation += attackDirection * rotationSpeed;

            // Freeze in place
            NPC.velocity = Vector2.Zero;

            // Dust effect
            for (int i = 0; i < 5; i++)
            {
                int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, FargoSoulsUtil.AprilFools ? DustID.SolarFlare : DustID.Vortex, 0f, 0f, 0, default, 1.5f);
                Main.dust[d].noGravity = true;
                Main.dust[d].noLight = true;
                Main.dust[d].velocity *= 4f;
            }
        }
    }
}
