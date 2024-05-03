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
            ref float playerCenterX = ref MainAI1;
            ref float playerCenterY = ref MainAI2;
            ref float endTime = ref MainAI3;
            ref float ai4 = ref MainAI4;
            ref float ai5 = ref MainAI5;
            ref float ai6 = ref MainAI6;
            ref float ai7 = ref MainAI7;

            int masoTimeBoost = 30;
            endTime = 450;

            // Initialization
            if (AttackTimer == 1)
            {
                float eyeAI1 = 0;

                // Start attack faster
                if (MasochistMode)
                {
                    AttackTimer = masoTimeBoost;
                    eyeAI1 = masoTimeBoost;
                }

                // Spawn eye
                if (HostCheck)
                {
                    int p = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, -Vector2.UnitY, ModContent.ProjectileType<MutantEyeOfCthulhu>(),
                        FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.target, eyeAI1);
                    if (MasochistMode && p != Main.maxProjectiles)
                        Main.projectile[p].timeLeft -= masoTimeBoost;
                }
            }

            // Track the player until EoC starts attacking. This is to base his movement on where you were such that it keeps the arena in place.
            if (AttackTimer < 120)
            {
                playerCenterX = Player.Center.X;
                playerCenterY = Player.Center.Y;
            }

            // Movement
            Vector2 targetPos = new(playerCenterX, playerCenterY);
            targetPos += NPC.DirectionFrom(targetPos).RotatedBy(MathHelper.ToRadians(-5)) * 450f;   // This keeps him rotating around you
            if (NPC.Distance(targetPos) > 50)
                Movement(targetPos, 0.25f);
        }
    }
}
