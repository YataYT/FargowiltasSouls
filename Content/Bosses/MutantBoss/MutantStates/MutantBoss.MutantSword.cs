using Luminance.Common.StateMachines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public partial class MutantBoss : ModNPC
    {
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.MutantSword)]
        public void MutantSword() {
            ref float ai1 = ref AI1;
            ref float ai2 = ref AI2;
            ref float ai3 = ref AI3;
            ref float lai0 = ref LAI0;
            ref float lai1 = ref LAI1;
            ref float lai2 = ref LAI2;
            ref float lai3 = ref LAI3;

            if (ai2 == 0) {
                return;
            }
        }
    }
}
