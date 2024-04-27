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
        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.Opening)]
        public void Opening() {
            if (AttackTimer % 60 == 0)
                Main.NewText("n-nyaaa~");

            if (AttackTimer > 120)
                AttackTimer = -1;
        }
    }
}
