using Fargowiltas.NPCs;
using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Content.Buffs.Souls;
using FargowiltasSouls.Content.Items.BossBags;
using FargowiltasSouls.Content.Items.Materials;
using FargowiltasSouls.Content.Items.Pets;
using FargowiltasSouls.Content.Items.Placables.Relics;
using FargowiltasSouls.Content.Items.Placables.Trophies;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.ItemDropRules.Conditions;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public partial class MutantBoss : ModNPC
    {
        public override string Texture => $"FargowiltasSouls/Content/Bosses/MutantBoss/{FargoSoulsUtil.TryAprilFoolsTexturePath}MutantBoss{FargoSoulsUtil.TryAprilFoolsTexture}";

        public override void SetStaticDefaults() {
            // DisplayName.SetDefault("Mutant");

            Main.npcFrameCount[NPC.type] = 4;
            NPCID.Sets.NoMultiplayerSmoothingByType[NPC.type] = true;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(NPC.type);
            NPCID.Sets.MustAlwaysDraw[Type] = true;

            NPC.AddDebuffImmunities(new List<int>
            {
                BuffID.Confused,
                BuffID.Chilled,
                BuffID.OnFire,
                BuffID.Suffocation,
                ModContent.BuffType<LethargicBuff>(),
                ModContent.BuffType<ClippedWingsBuff>(),
                ModContent.BuffType<MutantNibbleBuff>(),
                ModContent.BuffType<OceanicMaulBuff>(),
                ModContent.BuffType<LightningRodBuff>(),
                ModContent.BuffType<SadismBuff>(),
                ModContent.BuffType<GodEaterBuff>(),
                ModContent.BuffType<TimeFrozenBuff>(),
                ModContent.BuffType<LeadPoisonBuff>(),

            });

        }

        public override void SetDefaults() {
            NPC.width = 120;//34;
            NPC.height = 120;//50;
            NPC.damage = 444;
            NPC.defense = 255;
            NPC.value = Item.buyPrice(7);
            NPC.lifeMax = Main.expertMode ? 7700000 : 3500000;
            NPC.HitSound = SoundID.NPCHit57;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.npcSlots = 50f;
            NPC.knockBackResist = 0f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.aiStyle = -1;
            NPC.netAlways = true;
            NPC.timeLeft = NPC.activeTime * 30;
            if (WorldSavingSystem.AngryMutant) {
                NPC.damage *= 17;
                NPC.defense *= 10;
            }

            if (ModLoader.TryGetMod("FargowiltasMusic", out Mod musicMod)) {
                Music = MusicLoader.GetMusicSlot(musicMod,
                    WorldSavingSystem.MasochistModeReal ? "Assets/Music/rePrologue" : "Assets/Music/SteelRed");
            } else {
                Music = MusicID.OtherworldlyTowers;
            }
            SceneEffectPriority = SceneEffectPriority.BossHigh;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Sky,
                new FlavorTextBestiaryInfoElement($"Mods.FargowiltasSouls.Bestiary.{Name}")
            });
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) {
            NPC.damage = (int)Math.Round(NPC.damage * 0.5);
            NPC.lifeMax = (int)Math.Round(NPC.lifeMax * 0.5 * balance);
        }

        public override bool CanHitPlayer(Player target, ref int CooldownSlot) {
            CooldownSlot = 1;
            if (!WorldSavingSystem.MasochistModeReal)
                return false;
            return NPC.Distance(FargoSoulsUtil.ClosestPointInHitbox(target, NPC.Center)) < Player.defaultHeight;
        }

        public override bool CanHitNPC(NPC target) {
            if (target.type == ModContent.NPCType<Deviantt>() || target.type == ModContent.NPCType<Abominationn>() || target.type == ModContent.NPCType<Mutant>())
                return false;
            return base.CanHitNPC(target);
        }

        public override void SendExtraAI(BinaryWriter writer) {
            writer.Write(NPC.localAI[0]);
            writer.Write(NPC.localAI[1]);
            writer.Write(NPC.localAI[2]);
        }

        public override void ReceiveExtraAI(BinaryReader reader) {
            NPC.localAI[0] = reader.ReadSingle();
            NPC.localAI[1] = reader.ReadSingle();
            NPC.localAI[2] = reader.ReadSingle();
        }

        public override void OnSpawn(IEntitySource source) {
            // Find Mutant NPC, silently kill the imposter, and move to his place like a sussy imposter
            if (ModContent.TryFind("Fargowiltas", "Mutant", out ModNPC modNPC)) {
                int n = NPC.FindFirstNPC(modNPC.Type);
                if (n != -1 && n != Main.maxNPCs) {
                    NPC.Bottom = Main.npc[n].Bottom;

                    Main.npc[n].life = 0;
                    Main.npc[n].active = false;
                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n);
                }
            }

            AuraCenter = NPC.Center;
        }

        public override void AI() {
            // Try to find a new target if the current one is invalid
            bool invalidTarget = Target.Invalid;
            if (invalidTarget)
                NPC.TargetClosest(false);

            EModeGlobalNPC.mutantBoss = NPC.whoAmI;

            // Try to find a new target if the current one is very far away.
            if (!NPC.WithinRange(Target.Center, 4600f))
                NPC.TargetClosest(false);

            // No players, so gtfo
            if (Target.Invalid)
                NPC.active = false;

            // Phase 2+: Disable weather effects and set time to midnight for empress visuals
            if (EternityMode && CurrentPhase > 0) {
                Main.dayTime = false;
                Main.time = 16200; // midnight

                Main.raining = false;
                Main.rainTime = 0;
                Main.maxRaining = 0;

                Main.bloodMoon = false;
            }

            // Refil the state machine if it's empty
            if ((StateMachine?.StateStack?.Count ?? 1) <= 0)
                StateMachine.StateStack.Push(StateMachine.StateRegistry[BehaviorStates.RefillAttacks]);

            // Update the state machine.
            StateMachine.PerformBehaviors();
            StateMachine.PerformStateTransitionCheck();

            // Ensure that there is a valid state timer to get.
            if (StateMachine.StateStack.Count > 0)
                AttackTimer++;

            // DEBUG AREA
            CurrentPhase = 0;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
            if (WorldSavingSystem.EternityMode) {
                target.FargoSouls().MaxLifeReduction += 100;
                target.AddBuff(ModContent.BuffType<OceanicMaulBuff>(), 5400);
                target.AddBuff(ModContent.BuffType<MutantFangBuff>(), 180);
            }
            target.AddBuff(ModContent.BuffType<CurseoftheMoonBuff>(), 600);
        }

        public override void HitEffect(NPC.HitInfo hit) {
            for (int i = 0; i < 3; i++) {
                int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, FargoSoulsUtil.AprilFools ? DustID.SolarFlare : DustID.Vortex, 0f, 0f, 0, default, 1f);
                Main.dust[d].noGravity = true;
                Main.dust[d].noLight = true;
                Main.dust[d].velocity *= 3f;
            }
        }

        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers) {
            if (WorldSavingSystem.AngryMutant)
                modifiers.FinalDamage *= 0.07f;
        }

        public override bool CheckDead() {
            return true;
        }

        public override void OnKill() {
            base.OnKill();

            if (!playerInvulTriggered && WorldSavingSystem.EternityMode) {
                Item.NewItem(NPC.GetSource_Loot(), NPC.Hitbox, ModContent.ItemType<PhantasmalEnergy>());
            }

            if (WorldSavingSystem.EternityMode) {
                if (Main.LocalPlayer.active) {
                    if (!Main.LocalPlayer.FargoSouls().Toggler.CanPlayMaso && Main.netMode != NetmodeID.Server)
                        Main.NewText(Language.GetTextValue($"Mods.{Mod.Name}.Message.MasochistModeUnlocked"), new Color(51, 255, 191, 0));
                    Main.LocalPlayer.FargoSouls().Toggler.CanPlayMaso = true;
                }
                WorldSavingSystem.CanPlayMaso = true;
            }

            WorldSavingSystem.SkipMutantP1 = 0;

            NPC.SetEventFlagCleared(ref WorldSavingSystem.downedMutant, -1);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot) {
            base.ModifyNPCLoot(npcLoot);

            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<MutantBag>()));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MutantTrophy>(), 10));

            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<MutantRelic>()));
            npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<SpawnSack>(), 4));

            LeadingConditionRule emodeRule = new(new EModeDropCondition());
            emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<Items.Accessories.Masomode.MutantEye>()));
            npcLoot.Add(emodeRule);
        }

        public override void BossLoot(ref string name, ref int potionType) {
            potionType = ItemID.SuperHealingPotion;
        }

        public override void FindFrame(int frameHeight) {
            if (++NPC.frameCounter > 4) {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= Main.npcFrameCount[NPC.type] * frameHeight)
                    NPC.frame.Y = 0;
            }
        }

        public override void BossHeadSpriteEffects(ref SpriteEffects spriteEffects) {
            //spriteEffects = NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Npc[NPC.type].Value;
            Vector2 position = NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY);
            Rectangle rectangle = NPC.frame;
            Vector2 origin2 = rectangle.Size() / 2f;

            SpriteEffects effects = NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(texture2D13, position, new Rectangle?(rectangle), NPC.GetAlpha(drawColor), NPC.rotation, origin2, NPC.scale, effects, 0);

            Vector2 auraPosition = AuraCenter - screenPos + new Vector2(0f, NPC.gfxOffY);

            return false;
        }
    }
}
