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

            // The moment Mutant enters Phase 2, begin the transition
            StateMachine.ApplyToAllStatesExcept((state) =>
            {
                StateMachine.RegisterTransition(state, BehaviorStates.Phase2Transition, false, () => CurrentPhase < 2 && LifeRatio <= 0.5f);
            }, BehaviorStates.Phase2Transition);

            // The moment Mutant enters Phase 3, begin the transition
            StateMachine.ApplyToAllStatesExcept((state) =>
            {
                StateMachine.RegisterTransition(state, BehaviorStates.Phase3Transition, false, () => CurrentPhase < 3 && NPC.life <= 1);
            }, BehaviorStates.Phase3Transition);

            // Spear Toss Predictive
            StateMachine.RegisterTransition(BehaviorStates.SpearTossPredictiveWithDestroyers, null, false, () => AttackTimer > MainAI5 && MainAI2 == MainAI3);

            // Void Rays
            StateMachine.RegisterTransition(BehaviorStates.VoidRays, null, false, () => MainAI3 >= MainAI0 && AttackTimer > 3);

            // Spear Dash Predictive
            StateMachine.RegisterTransition(BehaviorStates.SpearDashPredictive, null, false, () => MainAI6 != 0);

            // Okuu Spheres
            StateMachine.RegisterTransition(BehaviorStates.OkuuSpheres, null, false, () => AttackTimer > MainAI7);

            // Boundary Bullet Hell
            StateMachine.RegisterTransition(BehaviorStates.BoundaryBulletHell, null, false, () => AttackTimer > MainAI0);

            // True Eye Dive
            StateMachine.RegisterTransition(BehaviorStates.TrueEyeDive, null, false, () => MainAI1 != 0 && AttackTimer > MainAI1);

            // Spawn Destroyers
            StateMachine.RegisterTransition(BehaviorStates.SpawnDestroyers, null, false, () => MainAI7 == -1);

            // Spear Toss Direct
            StateMachine.RegisterTransition(BehaviorStates.SpearTossDirect, null, false, () => MainAI5 != 0 && MainAI2 > MainAI5);

            // Mutant Sword
            StateMachine.RegisterTransition(BehaviorStates.MutantSword, null, false, () => MainAI1 != 0 && MainAI6 >= MainAI1);

            // Mech Ray Fan
            StateMachine.RegisterTransition(BehaviorStates.MechRayFan, null, false, () => MainAI0 != 0 && AttackTimer > MainAI0);

            // Spawn Fishrons
            StateMachine.RegisterTransition(BehaviorStates.SpawnFishrons, null, false, () => MainAI1 != 0 && AttackTimer > MainAI1);

            // Nuke
            StateMachine.RegisterTransition(BehaviorStates.Nuke, null, false, () => MainAI0 != 0 && AttackTimer > MainAI0);

            // Slime Rain
            StateMachine.RegisterTransition(BehaviorStates.SlimeRain, null, false, () => MainAI0 != 0 && AttackTimer > MainAI0);

            // Twinrangs and Crystals
            StateMachine.RegisterTransition(BehaviorStates.TwinRangsAndCrystals, null, false, () => MainAI3 != 0 && AttackTimer >= MainAI3);

            // Empress Sword Wave
            StateMachine.RegisterTransition(BehaviorStates.EmpressSwordWave, null, false, () => MainAI0 != 0 && AttackTimer > MainAI0);

            // Pillar Dunk
            StateMachine.RegisterTransition(BehaviorStates.PillarDunk, null, false, () => MainAI7 != 0 && AttackTimer > MainAI7);

            // EoC Star Sickles
            StateMachine.RegisterTransition(BehaviorStates.EoCStarSickles, null, false, () => MainAI3 != 0 && AttackTimer > MainAI3);

            // Final Spark
            StateMachine.RegisterTransition(BehaviorStates.FinalSpark, null, false, () => AttackTimer == 360);

            // Phase 2 Transition
            StateMachine.RegisterTransition(BehaviorStates.Phase2Transition, null, false, () => MainAI3 != 0 && AttackTimer >= MainAI3, () =>
            {
                NPC.dontTakeDamage = false;
                StateMachine.StateStack.Clear();
            });

            // Phase 3 Transition
            StateMachine.RegisterTransition(BehaviorStates.Phase3Transition, null, false, () => MainAI7 != 0, () => NPC.dontTakeDamage = false);

            #endregion Transition Registering
        }

        private void OnStateTransition(bool stateWasPopped, EntityAIState<BehaviorStates> oldState)
        {
            NPC.netUpdate = true;
            NPC.TargetClosest(false);

            MainAI0 = 0;
            MainAI1 = 0;
            MainAI2 = 0;
            MainAI3 = 0;
            MainAI4 = 0;
            MainAI5 = 0;
            MainAI6 = 0;
            MainAI7 = 0;
            MainAI8 = 0;
            MainAI9 = 0;

            NPC_LAI0 = 0;
            NPC_LAI1 = 0;
            NPC_LAI2 = 0;
            NPC_LAI3 = 0;

            if (oldState != null && (P1Attacks.Contains(oldState.Identifier) || P2Attacks.Contains(oldState.Identifier) || P3Attacks.Contains(oldState.Identifier)))
                LastAttackChoice = (int)oldState.Identifier;
        }

        public void LoadTransition_ResetCycle() {
            StateMachine.RegisterTransition(BehaviorStates.RefillAttacks, null, false, () => true, () => {
                NPC.netUpdate = true;

                if (!HostCheck)
                    return;

                StateMachine.StateStack.Clear();
                /*
                
                StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.MutantSword]);
                StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.BoundaryBulletHell]);
                StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.VoidRays]);
                StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.SpearDashDirect]);
                StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.TrueEyeDive]);
                StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.OkuuSpheres]);
                StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.SpearTossPredictiveWithDestroyers]);
                

                //StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.SpearDashDirect]);

                if (CurrentPhase == 1)
                {
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.TrueEyeDive]);
                }
                
                if (CurrentPhase == 2)
                {
                //StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.TwinRangsAndCrystals]);
                //StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.SpearTossDirect]);
                //StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.SpearDashPredictive]);
                //StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.EmpressSwordWave]);
                //StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.EoCStarSickles]);
                //StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.Nuke]);
                StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.SpawnFishrons]);
                StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.SlimeRain]);
                //StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.PillarDunk]);
                }

                if (CurrentPhase == 3)
                {
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.FinalSpark]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.BoundaryBulletHell]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.OkuuSpheres]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.VoidRays]);
                }
                */

                // Get the correct attack list, and remove the last attack used
                List<BehaviorStates> attackList = (CurrentPhase == 1 ? P1Attacks : P2Attacks).Where(attack => attack != (BehaviorStates)LastAttackChoice).ToList();
                if (CurrentPhase == 1)
                {
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.MutantSword]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.BoundaryBulletHell]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.VoidRays]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.SpearDashDirect]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.TrueEyeDive]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.OkuuSpheres]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.SpearTossPredictiveWithDestroyers]);
                } 
                else if (CurrentPhase == 2)
                {
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.EoCStarSickles]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.MutantSword]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.SpearTossDirect]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.BoundaryBulletHell]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.MechRayFan]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.PillarDunk]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.VoidRays]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.SpawnFishrons]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.SpearDashDirect]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.Nuke]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.TrueEyeDive]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.SpearDashPredictive]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.SlimeRain]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.OkuuSpheres]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.TwinRangsAndCrystals]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.SpearTossPredictiveWithDestroyers]);
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.EmpressSwordWave]);

                    return;

                    // Fill a list of indices
                    var indices = new List<int>();
                    for (int i = 0; i < attackList.Count; i++)
                        indices.Add(i);

                    // Randomly push the attack list using the indices list accessed with a random index
                    for (int i = 0; i < attackList.Count; i++)
                    {
                        var currentIndex = indices[Main.rand.Next(0, indices.Count)];
                        StateMachine.StateStack.Push(StateMachine.StateRegistry[attackList[currentIndex]]);
                        indices.Remove(currentIndex);
                    }
                }
            });
        }
    }
}
