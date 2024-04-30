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
            if (CurrentPhase == 0)
                OkuuSpheresP1();
            else
                OkuuSpheresP2AndP3();
        }

        public void OkuuSpheresP1()
        {
            ref float shootTimer = ref AI1;
            ref float sphereRingsShot = ref AI2;
            ref float endAttack = ref LAI3;

            float masoSpeedBuff = MasochistMode ? 3 : 1;
            int max = MasochistMode ? 9 : 6;
            float speed = MasochistMode ? 10 : 9;
            int sign = MasochistMode ? (sphereRingsShot % 2 == 0 ? 1 : -1) : 1;

            // Don't move only if in Maso
            if (MasochistMode)
                NPC.velocity = Vector2.Zero;
            else
                NPC.velocity = NPC.SafeDirectionTo(Player.Center) * 2f;

            // Spawn sphere rings
            if (--shootTimer < 0)
            {
                sphereRingsShot++;
                shootTimer = 90 / masoSpeedBuff;

                if (sphereRingsShot < 4 * masoSpeedBuff)
                {
                    SpawnSphereRing(max, speed, (int)(0.8 * FargoSoulsUtil.ScaledProjectileDamage(NPC.damage)), 1f * sign);
                    SpawnSphereRing(max, speed, (int)(0.8 * FargoSoulsUtil.ScaledProjectileDamage(NPC.damage)), -0.5f * sign);
                }
            }

            // End the attack when enough rings are fired, except with some extra end time in masomode (weak)
            if (sphereRingsShot > 4 * masoSpeedBuff)
            {
                if (!MasochistMode || sphereRingsShot > 6 * masoSpeedBuff)
                    endAttack++;
            }
        }

        public void OkuuSpheresP2AndP3()
        {
            ref float shootTimer = ref AI1;
            ref float direction = ref AI2;
            ref float overallAttackTimer = ref AI3;
            ref float endTime = ref LAI1;
            ref float endAttack = ref LAI3;

            // Don't move
            NPC.velocity = Vector2.Zero;

            bool isPhaseTwo = CurrentPhase == 1;    // Phase 3 if false
            endTime = 360;

            // Phase-specific changes
            if (!isPhaseTwo)
                endTime += 120 + (MasochistMode ? 360 : 0);

            // Shoot every 10 frames after 60 seconds
            if (++shootTimer > 10 && overallAttackTimer > 60 && overallAttackTimer < endTime)
            {
                shootTimer = 0;
                float rotation = MathHelper.ToRadians(60) * (overallAttackTimer - 45) / 240 * direction;
                int max = (MasochistMode ? 10 : 9) + (isPhaseTwo ? 0 : 1);
                float speed = MasochistMode ? 11f : 10f;
                float rotationModifier = isPhaseTwo ? 1f : 0.75f;
                SpawnSphereRing(max, speed, FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), -rotationModifier, rotation);
                SpawnSphereRing(max, speed, FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), rotationModifier, rotation);
            }

            // Initialized at the start of an attack
            if (direction == 0 && isPhaseTwo)
            {
                direction = Main.rand.NextBool() ? -1 : 1;
                overallAttackTimer = Main.rand.NextFloat(MathHelper.TwoPi);
                
                // Telegraph that the attack is about to happen
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                if (HostCheck)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<GlowRing>(), 0, 0f, Main.myPlayer, NPC.whoAmI, -2);
            }

            // Effects
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
