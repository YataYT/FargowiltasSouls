using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using Luminance.Common.StateMachines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public partial class MutantBoss : ModNPC
    {
        private PushdownAutomata<EntityAIState<BehaviorStates>, BehaviorStates> stateMachine;

        public PushdownAutomata<EntityAIState<BehaviorStates>, BehaviorStates> StateMachine
        {
            get
            {
                if (stateMachine is null)
                    LoadStates();

                return stateMachine;
            }
            set => stateMachine = value;
        }

        public ref int AttackTimer => ref StateMachine.CurrentState.Time;

        public void LoadStates()
        {
            // Initialize the AI state machine
            StateMachine = new(new(BehaviorStates.Opening));

            // Register all Mutant Boss states in the machine
            for (int i = 0; i < (int)BehaviorStates.Count; i++)
                StateMachine.RegisterState(new((BehaviorStates)i));

            StateMachine.OnStateTransition += OnStateTransition;

            // Autoload the state behaviors
            AutoloadAsBehavior<EntityAIState<BehaviorStates>, BehaviorStates>.FillStateMachineBehaviors<ModNPC>(StateMachine, this);

            LoadTransition_ResetCycle();
            LoadTransition_PhaseTwoTransition();

            #region Transition Registering
            // See Cursed Coffin code for a deeper explanation of how to do this part

            // Opening
            StateMachine.RegisterTransition(BehaviorStates.Opening, null, false, () => AttackTimer == -1);

            // Spear Dash Direct
            StateMachine.RegisterTransition(BehaviorStates.SpearDashDirect, null, false, () => AI3 >= LAI2 && AttackTimer > LAI0, () => {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].type == ModContent.ProjectileType<MutantSpearSpin>() || Main.projectile[i].type == ModContent.ProjectileType<MutantSpearDash>())
                        Main.projectile[i].Kill();
                }
            });

            // Spear Toss Predictive
            StateMachine.RegisterTransition(BehaviorStates.SpearTossPredictive, null, false, () => AttackTimer > LAI1 && AI2 == AI3);

            // Void Rays
            StateMachine.RegisterTransition(BehaviorStates.VoidRays, null, false, () => AttackTimer == 360);

            // Okuu Spheres
            StateMachine.RegisterTransition(BehaviorStates.OkuuSpheres, null, false, () => AttackTimer == 360);

            // Boundary Bullet Hell
            StateMachine.RegisterTransition(BehaviorStates.BoundaryBulletHell, null, false, () => AttackTimer == 360);

            // True Eye Dive
            StateMachine.RegisterTransition(BehaviorStates.TrueEyeDive, null, false, () => AttackTimer == 360);

            // Spawn Destroyers
            StateMachine.RegisterTransition(BehaviorStates.SpawnDestroyers, null, false, () => AttackTimer == 360);

            // Spear Toss Direct
            StateMachine.RegisterTransition(BehaviorStates.SpearTossDirect, null, false, () => AttackTimer == 360);

            // Mech Ray Fan
            StateMachine.RegisterTransition(BehaviorStates.MechRayFan, null, false, () => AttackTimer == 360);

            // Spawn Fishrons
            StateMachine.RegisterTransition(BehaviorStates.SpawnFishrons, null, false, () => AttackTimer == 360);

            // Nuke
            StateMachine.RegisterTransition(BehaviorStates.Nuke, null, false, () => AttackTimer == 360);

            // Slime Rain
            StateMachine.RegisterTransition(BehaviorStates.SlimeRain, null, false, () => AttackTimer == 360);

            // Twinrangs and Crystals
            StateMachine.RegisterTransition(BehaviorStates.TwinRangsAndCrystals, null, false, () => AttackTimer == 360);

            // Empress Sword Wave
            StateMachine.RegisterTransition(BehaviorStates.EmpressSwordWave, null, false, () => AttackTimer == 360);

            // Pillar Dunk
            StateMachine.RegisterTransition(BehaviorStates.PillarDunk, null, false, () => AttackTimer == 360);

            // EoC Star Sickles
            StateMachine.RegisterTransition(BehaviorStates.EoCStarSickles, null, false, () => AttackTimer == 360);

            // Final Spark
            StateMachine.RegisterTransition(BehaviorStates.FinalSpark, null, false, () => AttackTimer == 360);

            // Phase 2 Transition
            StateMachine.RegisterTransition(BehaviorStates.Phase2Transition, null, false, () => AttackTimer == 360);

            #endregion Transition Registering
        }

        private void OnStateTransition(bool stateWasPopped, EntityAIState<BehaviorStates> oldState)
        {
            NPC.netUpdate = true;
            NPC.TargetClosest(false);

            AI1 = 0;
            AI2 = 0;
            AI3 = 0;
            LAI0 = 0;
            LAI1 = 0;
            LAI2 = 0;
            LAI3 = 0;

            if (oldState != null && (P1Attacks.Contains(oldState.Identifier) || P2Attacks.Contains(oldState.Identifier) || P3Attacks.Contains(oldState.Identifier)))
                LastAttackChoice = (int)oldState.Identifier;
        }

        public void LoadTransition_ResetCycle() {
            StateMachine.RegisterTransition(BehaviorStates.RefillAttacks, null, false, () => true, () => {
                NPC.netUpdate = true;

                if (!HostCheck)
                    return;

                StateMachine.StateStack.Clear();

                StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.SpearDashDirect]);
                StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.SpearTossPredictive]);
                return;

                // Get the correct attack list, and remove the last attack used
                List<BehaviorStates> attackList = (CurrentPhase == 0 ? P1Attacks : P2Attacks).Where(attack => attack != (BehaviorStates)LastAttackChoice).ToList();

                // Fill a list of indices
                var indices = new List<int>();
                for (int i = 0; i < attackList.Count; i++)
                    indices.Add(i);

                // Randomly push the attack list using the indices list accessed with a random index
                for (int i = 0; i < attackList.Count; i++) {
                    var currentIndex = indices[Main.rand.Next(0, indices.Count)];
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[attackList[currentIndex]]);
                    indices.Remove(currentIndex);
                }
            });
        }

        public void LoadTransition_PhaseTwoTransition() {
            // Transition hijack
            StateMachine.AddTransitionStateHijack(originalState => {
                if (CurrentPhase != 1 && LifeRatio <= 0.5f) {
                    StateMachine.StateStack.Clear();
                    return BehaviorStates.Phase2Transition;
                }

                return originalState;
            });
        }
    }
}
