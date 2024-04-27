using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using FargowiltasSouls.Content.Projectiles;
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
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Humanizer.On;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public partial class MutantBoss : ModNPC
    {
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.BoundaryBulletHell)]
        public void BoundaryBulletHell() {
            ref float currentDirection = ref AI0;
            ref float currentRotation = ref AI2;

            // Shoots faster in P2 with less eyes. Shoots slower in P3 with more eyes.
            float attackDelay = (CurrentPhase == 1 ? 2 : 3);

            NPC.velocity = Vector2.Zero;

            // Initialize direction
            if (currentDirection == 0) {
                currentDirection = MathF.Sign(NPC.Center.X - Player.Center.X);

                // Create glow ring effect in P2
                if (HostCheck && CurrentPhase == 1)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<GlowRing>(), 0, 0f, Main.myPlayer, NPC.whoAmI, -2);

                // idk if this is needed
                if (MasochistMode && CurrentPhase == 1)
                    currentRotation = Main.rand.NextFloat(MathHelper.Pi);
            }

            // In p2, only start shooting after a delay. In p3, start instantly
            bool phaseTwoDelay = CurrentPhase == 1 ? AttackTimer > 60 : true;

            // Shoot mutant eyes
            if (AttackTimer % attackDelay == 0 && phaseTwoDelay) {
                SoundEngine.PlaySound(SoundID.Item12, NPC.Center);

                // Custom rotation for each phase
                if (CurrentPhase == 1)
                    currentRotation += MasochistMode ? MathHelper.Pi / 8 / 480 * (AttackTimer - 300) * currentDirection : MathHelper.Pi / 77f;
                else if (CurrentPhase == 2)
                    currentRotation += MathHelper.Pi / 8 / 480 * AttackTimer * currentDirection;
                else
                    currentRotation += MathHelper.Pi / 5 / 420 * AttackTimer * currentDirection * (MasochistMode ? 2 : 1);
                currentRotation = MathHelper.WrapAngle(currentRotation);    // Keep the rotation wrapped

                if (HostCheck) {
                    int max = CalculateEyeAmount();
                    float eyeSpeed = CurrentPhase == 0 ? -7f : -6f;

                    for (int i = 0; i < max; i++)
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(0f, eyeSpeed).RotatedBy(currentRotation + MathHelper.TwoPi / max * i),
                           ModContent.ProjectileType<MutantEye>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer);
                }
            }

            // Only in P3
            if (CurrentPhase == 2) {
                for (int i = 0; i < 5; i++) {
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, FargoSoulsUtil.AprilFools ? DustID.SolarFlare : DustID.Vortex, 0f, 0f, 0, default, 1.5f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].noLight = true;
                    Main.dust[d].velocity *= 4f;
                }
            }
        }

        // Since the values for each phase are very different, calculating the number of eyes shot per "cycle" gets their own function
        private int CalculateEyeAmount() {
            if (CurrentPhase == 0) {                // 4/5 eyes
                return MasochistMode ? 5 : 4;
            } else if (CurrentPhase == 1) {         // 4/5/6 eyes
                int numEyes = 4;

                if (EternityMode)
                    numEyes++;
                if (MasochistMode)
                    numEyes++;

                return numEyes;
            } else {                                // 8/10 eyes
                return MasochistMode ? 10 : 8;
            }
        }
    }
}
