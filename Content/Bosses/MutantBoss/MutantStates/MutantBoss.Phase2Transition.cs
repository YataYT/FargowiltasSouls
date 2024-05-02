using Luminance.Common.StateMachines;
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
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.Phase2Transition)]
        public void Phase2Transition() {
            CurrentPhase = 2;
            if (AttackTimer % 60 == 0)
                Main.NewText("uwu >~<");
        }
    }
}
