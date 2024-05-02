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
        private List<string> EdgyText = new List<string>
        {
            "YOU ARE NOTHING COMPARED TO THE ERODED SPIRITS! I BRING FORTH THE END UPON THE FOOLISH, THE UNWORTHY!",
            "YOU WANT TO DEFEAT ME? MAYBE IN TWO ETERNITIES! DIE, FOOLISH TERRARIAN!",
            "THEY SAID THERE WAS 3 END BRINGERS, BUT I AM THE FOURTH, A BREAKER OF REALITY!",
            "HELL DOESN’T ACCEPT SCUM LIKE YOU, SO SUFFER FOREVER IN MY ENDLESS ONSLAUGHT OF INFINITE POWER",
            "THE POTENTIAL OF ETERNITIES STRETCHED TO THE ABSOLUTE MAXIMUM APOTHEOSES!",
            "YOUR UNHOLY SOUL SHALL BE CONSUMED BY DEPTHS LOWER THAN THE DEEPEST REACHES OF HELL!",
            "I CONTROL THE POWER THAT HAS REACHED FROM THE FAR ENDS OF THE UNIVERSE,",
            "UNITING DIMENSIONS, MANIPULATING TERRARIA, SLAYING MASOCHIST, AND JUDGING HEAVENS!",
            "FOR CENTURIES I HAVE TRAINED FOR ONE GOAL ONLY:",
            "PURGE THE WORLD OF THE UNWORTHY, SLAY THE WEAK, AND BRING FORTH TRUE POWER.",
            "IN THE HIGHEST REACHES OF HEAVEN, MY BROTHER RULES OVER THE SKY! SOON,",
            "ALL OF TERRARIA WILL BE PURGED OF THE UNWORTH AND A NEW AGE WILL START!",
            "A NEW AGE OF AWESOME! A GOLDEN AGE WHERE ONLY ABSOLUTE BEINGS EXIST!",
            "DEATH, INFERNO, TIDE; I AM THE OMEGA AND THE ALPAA, THE BEGINNING AND THE END!",
            "ALMIGHTY POWER; REVELATIONS. ABSOLUTE BEING, ABSOLUTE PURITY.",
            "WITHIN THE FOOLISH BANTERINGS OF THE MORTAL WORLD I HAVE ACHIEVED POWER THAT WAS ONCE BANISHED TO THE EDGE OF THE GALAXY!",
            "I BRING FOR CALAMITIES, CATASTROPHES, AND CATACLYSM; ELDRITCH POWERS DERIVED FROM THE ABSOLUTE WORD OF FATE.",
            "FEEL MY UBIQUITOUS WRATH DRIVE YOU INTO THE GROUND AS A WORLD SHAPER DRIVES HIS WORLD INTO REALITY!",
            "THE SHARPSHOOTER’S EYE PALES IN COMPARISON OF MY PERCEPTION OF REALITY! BERSERKERS RAGE NAUGHT BUT A BUNNIES!",
            "OLYMPIANS A MINOR GOD, ARCH WIZARDS A POSER! A MASTERY OF FLIGHT, THE IRON WILL OF A COLOSSUS;",
            "BOTH ELEMENTARY CONCEPTS! A CONJUROR BUT A PEDDLING MAGICIAN, A TRAWLER A SLIVER COMPARED TO MY LIFE MASTERY?",
            "SUPERSONIC SPEED, LIGHTSPEED TIME! GLORIOUS LIGHT SHALL ELIMINATE YOU, YOU FOOLISH BUFFOON!"
        };

        [AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>(BehaviorStates.Opening)]
        public void Opening() {
            ref float startAttacking = ref LAI2;
            ref float currentIndex = ref LAI3;

            Vector2 targetPos = Player.Center;
            float distanceFromPlayer = NPC.Distance(Player.Center);
            if (distanceFromPlayer > 1500)
            {
                targetPos += Vector2.UnitX.RotatedBy(Player.Center.AngleTo(NPC.Center)) * 1500;
                Movement(targetPos, 2f, true, false);
            }
            else
            {
                startAttacking = 666;
                NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 15f;
            }
                

            if (AttackTimer % 5 == 0 && currentIndex < EdgyText.Count)
            {
                Main.NewText(EdgyText[(int)currentIndex], new Color(Main.rand.Next(255), Main.rand.Next(255), Main.rand.Next(255), Main.rand.Next(255)));
                currentIndex++;
            }
        }
    }
}
