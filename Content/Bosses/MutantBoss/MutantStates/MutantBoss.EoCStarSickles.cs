using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using FargowiltasSouls.Core.Systems;
using Luminance.Common.StateMachines;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public partial class MutantBoss : ModNPC
    {
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.EoCStarSickles)]
        public void EoCStarSickles() {
            ref float ai1 = ref MainAI1;
            ref float ai2 = ref MainAI2;
            ref float ai3 = ref MainAI3;
            ref float lai0 = ref MainAI4;
            ref float lai1 = ref MainAI5;
            ref float lai2 = ref MainAI6;
            ref float lai3 = ref MainAI7;

            if (ai1++ == 0) {
                float eyeAI = 0;

                if (MasochistMode) //begin attack much faster
                {
                    eyeAI = 30;
                    ai1 = 30;
                }

                if (HostCheck) {
                    int p = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, -Vector2.UnitY, ModContent.ProjectileType<MutantEyeOfCthulhu>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.target, eyeAI);
                    if (WorldSavingSystem.MasochistModeReal && p != Main.maxProjectiles)
                        Main.projectile[p].timeLeft -= 30;
                }
            }

            if (ai1 < 120) //stop tracking when eoc begins attacking, this locks arena in place
            {
                ai2 = Player.Center.X;
                ai3 = Player.Center.Y;
            }

            Vector2 targetPos = new(ai2, ai3);
            targetPos += NPC.DirectionFrom(targetPos).RotatedBy(MathHelper.ToRadians(-5)) * 450f;
            if (NPC.Distance(targetPos) > 50)
                Movement(targetPos, 0.25f);
        }
    }
}
