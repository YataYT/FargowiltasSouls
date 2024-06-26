﻿using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
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
            StateMachine.RegisterTransition(BehaviorStates.Opening, null, false, () => MainAI6 != 0);

            // Spear Dash Direct
            StateMachine.RegisterTransition(BehaviorStates.SpearDashDirect, null, false, () => MainAI3 >= MainAI6 && AttackTimer > MainAI4, () => {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].type == ModContent.ProjectileType<MutantSpearSpin>() || Main.projectile[i].type == ModContent.ProjectileType<MutantSpearDash>())
                        Main.projectile[i].Kill();

                    // This causes all the eyes to fly away
                    if (Main.projectile[i].type == ModContent.ProjectileType<MutantEyeHoming>())
                        Main.projectile[i].ai[2] = 69;
                }
            });

            // Spear Toss Predictive
            StateMachine.RegisterTransition(BehaviorStates.SpearTossPredictiveWithDestroyers, null, false, () => AttackTimer > MainAI5 && MainAI2 == MainAI3);

            // Void Rays
            StateMachine.RegisterTransition(BehaviorStates.VoidRays, null, false, () => MainAI3 >= MainAI0 && AttackTimer > 3);

            // Okuu Spheres
            StateMachine.RegisterTransition(BehaviorStates.OkuuSpheres, null, false, () => AttackTimer > MainAI7);

            // Boundary Bullet Hell
            StateMachine.RegisterTransition(BehaviorStates.BoundaryBulletHell, null, false, () => AttackTimer > MainAI0);

            // True Eye Dive
            StateMachine.RegisterTransition(BehaviorStates.TrueEyeDive, null, false, () => MainAI1 != 0 && AttackTimer > MainAI1);

            // Spawn Destroyers
            StateMachine.RegisterTransition(BehaviorStates.SpawnDestroyers, null, false, () => MainAI7 == -1);

            // Spear Toss Direct
            StateMachine.RegisterTransition(BehaviorStates.SpearTossDirect, null, false, () => AttackTimer == 360);

            // Mutant Sword
            StateMachine.RegisterTransition(BehaviorStates.MutantSword, null, false, () => MainAI1 != 0 && MainAI6 >= MainAI1);

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

            MainAI1 = 0;
            MainAI2 = 0;
            MainAI3 = 0;
            MainAI4 = 0;
            MainAI5 = 0;
            MainAI6 = 0;
            MainAI7 = 0;

            if (oldState != null && (P1Attacks.Contains(oldState.Identifier) || P2Attacks.Contains(oldState.Identifier) || P3Attacks.Contains(oldState.Identifier)))
                LastAttackChoice = (int)oldState.Identifier;
        }

        public void LoadTransition_ResetCycle() {
            StateMachine.RegisterTransition(BehaviorStates.RefillAttacks, null, false, () => true, () => {
                NPC.netUpdate = true;

                if (!HostCheck)
                    return;

                StateMachine.StateStack.Clear();

                StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.MutantSword]);
                StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.BoundaryBulletHell]);
                StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.VoidRays]);
                StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.SpearDashDirect]);
                StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.TrueEyeDive]);
                StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.OkuuSpheres]);
                StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.SpearTossPredictiveWithDestroyers]);
                return;

                // Get the correct attack list, and remove the last attack used
                List<BehaviorStates> attackList = (CurrentPhase == 1 ? P1Attacks : P2Attacks).Where(attack => attack != (BehaviorStates)LastAttackChoice).ToList();

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
                if (CurrentPhase != 2 && LifeRatio <= 0.5f) {
                    StateMachine.StateStack.Clear();
                    return BehaviorStates.Phase2Transition;
                }

                return originalState;
            });
        }
    }
}
