using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using Luminance.Common.StateMachines;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public partial class MutantBoss : ModNPC
    {
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.Nuke)]
        public void Nuke() {
            ref float ai1 = ref AI1;    // This is only used for preparing nuke

            // Movement
            Vector2 movementTarget = NPC.Bottom.Y < Player.Top.Y
                ? Player.Center + 300f * Vector2.UnitX * Math.Sign(NPC.Center.X - Player.Center.X)  // explain
                : NPC.Center + 30 * NPC.DirectionFrom(Player.Center).RotatedBy(MathHelper.ToRadians(60) * Math.Sign(Player.Center.X - NPC.Center.X));   // explain
            Movement(movementTarget, 0.1f);

            // Move faster in maso
            int maxSpeed = MasochistMode ? 3 : 2;
            if (NPC.velocity.Length() > maxSpeed)
                NPC.velocity = Vector2.Normalize(NPC.velocity) * maxSpeed;

            // Explode the entire screen except for the safe zone
            if (AttackTimer > (MasochistMode ? 120 : 180)) {
                //if (!Main.dedServ && Main.LocalPlayer.active)
                    //Main.LocalPlayer.FargoSouls().Screenshake = 2;  // screnshaek

                // Spawn bombs all over except inside the safe area
                if (HostCheck) {
                    Vector2 safeZone = NPC.Center;
                    safeZone.Y -= 100;
                    float safeRange = 150 + 200;
                    for (int i = 0; i < 3; i++) {
                        Vector2 spawnPos = NPC.Center + Main.rand.NextVector2Circular(1200, 1200);
                        if (Vector2.Distance(safeZone, spawnPos) < safeRange) {
                            Vector2 directionOut = spawnPos - safeZone;
                            directionOut.Normalize();
                            spawnPos = safeZone + directionOut * Main.rand.NextFloat(safeRange, 1200);
                        }
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<MutantBomb>(),
                            FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 3f), 0f, Main.myPlayer);
                    }
                }
            }


            // Explosion effect
            if (AttackTimer > 45) {
                for (int i = 0; i < 20; i++) {
                    Vector2 offset = new();
                    offset.Y -= 100;
                    double angle = Main.rand.NextDouble() * 2d * Math.PI;
                    offset.X += (float)(Math.Sin(angle) * 150);
                    offset.Y += (float)(Math.Cos(angle) * 150);
                    Dust dust = Main.dust[Dust.NewDust(NPC.Center + offset - new Vector2(4, 4), 0, 0, FargoSoulsUtil.AprilFools ? DustID.SolarFlare : DustID.Vortex, 0, 0, 100, Color.White, 1.5f)];
                    dust.velocity = NPC.velocity;
                    if (Main.rand.NextBool(3))
                        dust.velocity += Vector2.Normalize(offset) * 5f;
                    dust.noGravity = true;
                }
            }
        }
    }
}
