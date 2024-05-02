using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using FargowiltasSouls.Content.Projectiles.Masomode;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Luminance.Common.StateMachines;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public partial class MutantBoss : ModNPC
    {
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.PillarDunk)]
        public void PillarDunk() {
            ref float ai1 = ref MainAI1;
            ref float ai2 = ref MainAI2;
            ref float ai3 = ref MainAI3;
            ref float lai0 = ref MainAI4;
            ref float lai1 = ref MainAI5;
            ref float lai2 = ref MainAI6;
            ref float lai3 = ref MainAI7;

            int pillarAttackDelay = 60;

            if (Main.getGoodWorld)
                this.Player.confused = true;

            if (ai2 == 0 && ai3 == 0) //target one corner of arena
            {
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                if (HostCheck) //spawn cultists
                {
                    void Clone(float ai1, float ai2, float ai3) => FargoSoulsUtil.NewNPCEasy(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<MutantIllusion>(), NPC.whoAmI, NPC.whoAmI, ai1, ai2, ai3);
                    Clone(-1, 1, pillarAttackDelay * 4);
                    Clone(1, -1, pillarAttackDelay * 2);
                    Clone(1, 1, pillarAttackDelay * 3);
                    if (MasochistMode)
                        Clone(1, 1, pillarAttackDelay * 6);

                    Projectile.NewProjectile(NPC.GetSource_FromThis(), Player.Center, new Vector2(0, -4), ModContent.ProjectileType<BrainofConfusion>(), 0, 0, Main.myPlayer);
                }

                NPC.netUpdate = true;
                ai2 = NPC.Center.X;
                ai3 = NPC.Center.Y;
                for (int i = 0; i < Main.maxProjectiles; i++) {
                    if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<MutantRitual>() && Main.projectile[i].ai[1] == NPC.whoAmI) {
                        ai2 = Main.projectile[i].Center.X;
                        ai3 = Main.projectile[i].Center.Y;
                        break;
                    }
                }

                Vector2 offset = 1000f * Vector2.UnitX.RotatedBy(MathHelper.ToRadians(45));
                if (Main.rand.NextBool()) //always go to a side player isn't in but pick a way to do it randomly
                {
                    if (Player.Center.X > ai2)
                        offset.X *= -1;
                    if (Main.rand.NextBool())
                        offset.Y *= -1;
                } else {
                    if (Main.rand.NextBool())
                        offset.X *= -1;
                    if (Player.Center.Y > ai3)
                        offset.Y *= -1;
                }

                lai1 = ai2; //for illusions
                lai2 = ai3;

                ai2 = offset.Length();
                ai3 = offset.ToRotation();
            }

            Vector2 targetPos = Player.Center;
            targetPos.X += NPC.Center.X < Player.Center.X ? -700 : 700;
            targetPos.Y += ai1 < 240 ? 400 : 150;
            if (NPC.Distance(targetPos) > 50)
                Movement(targetPos, 1f);

            int endTime = 240 + pillarAttackDelay * 4 + 60;
            if (MasochistMode)
                endTime += pillarAttackDelay * 2;

            lai0 = endTime - ai1; //for pillars to know remaining duration
            lai0 += 60f + 60f * (1f - ai1 / endTime); //staggered despawn


            if (ai1 == pillarAttackDelay) {
                if (HostCheck) {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitY * -5,
                        ModContent.ProjectileType<MutantPillar>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 3f), 0, Main.myPlayer, 3, NPC.whoAmI);
                }
            } else if (MasochistMode && ai1 == pillarAttackDelay * 5) {
                if (HostCheck) {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitY * -5,
                        ModContent.ProjectileType<MutantPillar>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 3f), 0, Main.myPlayer, 1, NPC.whoAmI);
                }
            }
        }
    }
}
