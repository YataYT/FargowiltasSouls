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
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.Phase3Transition)]
        public void Phase3Transition() {
            ref float ai1 = ref MainAI1;
            ref float ai2 = ref MainAI2;
            ref float ai3 = ref MainAI3;
            ref float lai0 = ref MainAI4;
            ref float lai1 = ref MainAI5;
            ref float lai2 = ref MainAI6;
            ref float lai3 = ref MainAI7;
        }
    }
}
