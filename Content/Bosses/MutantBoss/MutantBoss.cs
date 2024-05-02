using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using FargowiltasSouls.Content.Items.Summons;
using FargowiltasSouls.Content.Projectiles.Souls;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public partial class MutantBoss : ModNPC
    {
        #region Enums

        // No stage clarified: Not sure, probably stage 2-3
        // Stage 1: Refactor (replace values with yours)
        // Stage 2: General refactoring - setting clear variable names, commenting, etc
        // Stage 3: Total refactor/final touches (improved)
        public enum BehaviorStates {
            // General attacks (all 3 phases)
            VoidRays,                       // Stage 2 Done
            OkuuSpheres,                    // Stage 2 Done
            BoundaryBulletHell,             // Stage 2 Done

            // Phase 1 + 2 attacks
            SpearTossPredictiveWithDestroyers,            // Stage 2 Done
            SpearDashDirect,                // Pending
            TrueEyeDive,                    // Stage 2 Done
            MutantSword,                    // Pending
            SpawnDestroyers,                // Stage 2 Done

            // Phase 2 exclusive attacks
            SpearTossDirect,                // Stage 2 Done
            MechRayFan,                     // Stage 2 Done
            SpawnFishrons,                  // Stage 2 Done
            Nuke,                           // Stage 2 Done
            SlimeRain,                      // Stage 1 Done
            TwinRangsAndCrystals,           // Stage 1 Done
            EmpressSwordWave,               // Stage 1 Done
            SpearDashPredictive,            // Pending
            PillarDunk,                     // Stage 1 Done
            EoCStarSickles,                 // Stage 1 Done

            // Desperation attacks
            FinalSpark,                     // Stage 1 Done

            // Intermediate states
            ResetCycle, // maybe this isnt needed

            // Phase transitions
            Opening,                        // Not done
            Phase2Transition,               // Not done
            Phase3Transition,               // Not done

            // For the state machine
            RefillAttacks,                  // Not implemented
            Count
        }

        private readonly List<BehaviorStates> P1Attacks = new()
        {
            BehaviorStates.VoidRays,
            BehaviorStates.OkuuSpheres,
            BehaviorStates.BoundaryBulletHell,
            BehaviorStates.SpearTossPredictiveWithDestroyers,
            //BehaviorStates.SpearDashDirect,
            BehaviorStates.TrueEyeDive,
            //BehaviorStates.MutantSword
        };

        private readonly List<BehaviorStates> P2Attacks = new()
        {
            BehaviorStates.VoidRays,
            BehaviorStates.OkuuSpheres,
            BehaviorStates.BoundaryBulletHell,
            BehaviorStates.SpearTossPredictiveWithDestroyers,
            //BehaviorStates.SpearDashDirect,
            BehaviorStates.TrueEyeDive,
            //BehaviorStates.MutantSword,
            BehaviorStates.SpearTossDirect,
            BehaviorStates.SpawnDestroyers,
            BehaviorStates.MechRayFan,
            BehaviorStates.SpawnFishrons,
            BehaviorStates.Nuke,
            BehaviorStates.SlimeRain,
            BehaviorStates.TwinRangsAndCrystals,
            BehaviorStates.EmpressSwordWave,
            //BehaviorStates.SpearDashPredictive,
            BehaviorStates.PillarDunk,
            BehaviorStates.EoCStarSickles
        };

        private readonly List<BehaviorStates> P3Attacks = new()
        {
            BehaviorStates.VoidRays,
            BehaviorStates.OkuuSpheres,
            BehaviorStates.BoundaryBulletHell,
            BehaviorStates.FinalSpark
        };

        #endregion Enums

        #region Fields and Properties

        /// <summary>
        /// Mutant's life ratio as a 0-1 value. Mainly used for phase transition triggers.
        /// </summary>
        public float LifeRatio => MathHelper.Clamp(NPC.life / (float)NPC.lifeMax, 0f, 1f);

        /// <summary>
        /// Mutant's current player target.
        /// </summary>
        public NPCAimedTarget Target => NPC.GetTargetData();

        public Player Player => Main.player[NPC.target];

        public static float HyperMax = 0;

        public int CurrentRitualProjectile;
        public int CurrentAuraProjectile;

        public float AuraScale = 1f;

        public float EndTimeVariance;

        public const string ProjectilePath = "FargowiltasSouls/Content/Bosses/MutantBoss/MutantProjectiles/";
        public bool playerInvulTriggered = false;

        /// <summary>
        /// Ideally, this should be used for internal workings such as storing values across the same attack.
        /// The actual AI values (ai[0] - localAI[3]) are reserved for transmitting information outside of the boss,
        /// such as projectiles tied to it or effect managers.
        /// </summary>
        public float[] MainAI = new float[10];

        private ref float MainAI0 => ref MainAI[0];
        private ref float MainAI1 => ref MainAI[1];
        private ref float MainAI2 => ref MainAI[2];
        private ref float MainAI3 => ref MainAI[3];
        private ref float MainAI4 => ref MainAI[4];
        private ref float MainAI5 => ref MainAI[5];
        private ref float MainAI6 => ref MainAI[6];
        private ref float MainAI7 => ref MainAI[7];
        private ref float MainAI8 => ref MainAI[8];
        private ref float MainAI9 => ref MainAI[9];

        /// <summary>
        /// The current phase.
        /// </summary>
        private ref float CurrentPhase => ref NPC.ai[0];
        private ref float CurrentAttack => ref NPC.ai[1];
        private ref float AttackTimer => ref NPC.ai[2];
        private ref float NPC_AI3 => ref NPC.ai[3];
        private ref float NPC_LAI0 => ref NPC.localAI[0];
        private ref float NPC_LAI1 => ref NPC.localAI[1];
        private ref float NPC_LAI2 => ref NPC.localAI[2];
        private ref float NPC_LAI3 => ref NPC.localAI[3];

        public Vector2 AuraCenter;

        public bool EternityMode => WorldSavingSystem.EternityMode;
        public bool MasochistMode => WorldSavingSystem.MasochistModeReal;
        public bool HostCheck => FargoSoulsUtil.HostCheck;

        private int LastAttackChoice { get; set; }

        public BehaviorStates CurrentState
        {
            get
            {
                if ((StateMachine?.StateStack?.Count ?? 1) <= 0)
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.ResetCycle]);

                return StateMachine?.CurrentState?.Identifier ?? BehaviorStates.SpearTossDirect;
            }
        }

        #endregion Fields and Properties

        #region AI

        

        #endregion AI

        #region Other Methods

        public void Movement(Vector2 target, float speed, bool fastX = true, bool obeySpeedCap = true)
        {
            float turnAroundModifier = 1f;
            float maxSpeed = 24;

            if (WorldSavingSystem.MasochistModeReal)
            {
                speed *= 2;
                turnAroundModifier *= 2f;
                maxSpeed *= 1.5f;
            }

            if (Math.Abs(NPC.Center.X - target.X) > 10)
            {
                if (NPC.Center.X < target.X)
                {
                    NPC.velocity.X += speed;
                    if (NPC.velocity.X < 0)
                        NPC.velocity.X += speed * (fastX ? 2 : 1) * turnAroundModifier;
                }
                else
                {
                    NPC.velocity.X -= speed;
                    if (NPC.velocity.X > 0)
                        NPC.velocity.X -= speed * (fastX ? 2 : 1) * turnAroundModifier;
                }
            }
            
            if (NPC.Center.Y < target.Y)
            {
                NPC.velocity.Y += speed;
                if (NPC.velocity.Y < 0)
                    NPC.velocity.Y += speed * 2 * turnAroundModifier;
            }
            else
            {
                NPC.velocity.Y -= speed;
                if (NPC.velocity.Y > 0)
                    NPC.velocity.Y -= speed * 2 * turnAroundModifier;
            }

            if (obeySpeedCap)
            {
                if (Math.Abs(NPC.velocity.X) > maxSpeed)
                    NPC.velocity.X = maxSpeed * Math.Sign(NPC.velocity.X);
                if (Math.Abs(NPC.velocity.Y) > maxSpeed)
                    NPC.velocity.Y = maxSpeed * Math.Sign(NPC.velocity.Y);
            }
        }

        public void SpawnSphereRing(int max, float speed, int damage, float rotationModifier, float offset = 0)
        {
            if (!HostCheck) return;
            SoundEngine.PlaySound(SoundID.Item84, NPC.Center);

            float rotation = MathHelper.TwoPi / max;
            for (int i = 0; i < max; i++)
            {
                Vector2 velocity = speed * Vector2.UnitY.RotatedBy(rotation * i + offset);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, ModContent.ProjectileType<MutantSphereRing>(), damage, 0f, Main.myPlayer, rotationModifier * NPC.spriteDirection, speed);
            }
        }

        /// <summary>
        /// This function is a more compact and readable way to adjust values for difficulty.
        /// </summary>
        /// <param name="masochistValue"></param>
        /// <param name="eternityValue"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T AdjustValueForDifficulty<T>(T masochistValue, T eternityValue, T defaultValue) {
            if (MasochistMode)
                return masochistValue;
            else if (EternityMode)
                return eternityValue;
            else
                return defaultValue;
        }

        #endregion Other Methods
    }
}
