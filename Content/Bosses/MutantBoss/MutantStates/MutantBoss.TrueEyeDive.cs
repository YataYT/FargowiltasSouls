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
            ref float direction = ref AI3;
            ref float numberOfEyesReleased = ref AI2;

            // Fire an eye every X frames
            float eyeFireRate = 15;

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
                int maxEyeThreshold = MasochistMode ? 6 : 3;
                if (++numberOfEyesReleased <= maxEyeThreshold)
                {
                    if (HostCheck)
                    {
                        int type;
                        float ratio = numberOfEyesReleased / maxEyeThreshold * 3;

                        // Which kind of eye to use
                        if (ratio <= 1f)
                            type = ModContent.ProjectileType<MutantTrueEyeL>();
                        else if (ratio <= 2f)
                            type = ModContent.ProjectileType<MutantTrueEyeS>();
                        else
                            type = ModContent.ProjectileType<MutantTrueEyeR>();

                        int p = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, type, FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 0.8f), 0f, Main.myPlayer, NPC.target);

                        // Inform them which side the attack began on
                        if (p != Main.maxProjectiles)
                        {
                            Main.projectile[p].localAI[1] = direction;
                            Main.projectile[p].netUpdate = true;
                        }
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
        }
    }
}
