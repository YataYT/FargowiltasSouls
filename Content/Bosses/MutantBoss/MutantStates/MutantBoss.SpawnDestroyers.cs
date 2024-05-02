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
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public partial class MutantBoss : ModNPC
    {
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.SpawnDestroyers)]
        public void SpawnDestroyers() {
            // Normally this is a P2 exclusive attack, but Maso releases destroyers all at once in P1
            if (CurrentPhase == 1 && MasochistMode)
                SpawnDestroyersMasoP1();
            else if (CurrentPhase == 2)
                SpawnDestroyersP2();
            // Cancel the attack
            else
                MainAI7 = -1;
        }

        
    }
}
