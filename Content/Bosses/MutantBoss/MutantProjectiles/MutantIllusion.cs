using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Content.Buffs.Souls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    [AutoloadBossHead]
    public class MutantIllusion : ModNPC
    {
        public override string Texture => $"FargowiltasSouls/Content/Bosses/MutantBoss/{FargoSoulsUtil.TryAprilFoolsTexturePath}MutantBoss{FargoSoulsUtil.TryAprilFoolsTexture}";

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;

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
                ModContent.BuffType<TimeFrozenBuff>()
            });

            this.ExcludeFromBestiary();
        }

        public override void SetDefaults()
        {
            NPC.width = 34;
            NPC.height = 50;
            NPC.damage = 360;
            NPC.defense = 400;
            NPC.lifeMax = 7000000;
            NPC.dontTakeDamage = true;
            NPC.HitSound = SoundID.NPCHit57;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0f;
            NPC.lavaImmune = true;
            NPC.aiStyle = -1;
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.damage = (int)(NPC.damage * 0.5f);
            NPC.lifeMax = (int)(NPC.lifeMax * 0.5f * balance);
        }

        public override bool CanHitPlayer(Player target, ref int CooldownSlot) => false;

        public ref float MutantIndex => ref NPC.ai[0];
        public ref float DirectionX => ref NPC.ai[1];
        public ref float DirectionY => ref NPC.ai[2];
        public ref float ShootDelayTimer => ref NPC.ai[3];
        public ref float Timer => ref NPC.localAI[0];
        public ref float LAI1 => ref NPC.localAI[1];
        public ref float LAI2 => ref NPC.localAI[2];
        public ref float LAI3 => ref NPC.localAI[3];

        public override void AI()
        {
            NPC mutant = FargoSoulsUtil.NPCExists(MutantIndex, ModContent.NPCType<MutantBoss>());

            // Check whether to perish
            if (mutant == null || mutant.ai[1] != (int)MutantBoss.BehaviorStates.PillarDunk || mutant.life <= 1)
            {
                // Perish
                NPC.life = 0;
                NPC.HitEffect();
                NPC.SimpleStrikeNPC(int.MaxValue, 0, noPlayerInteraction: true);
                NPC.active = false;

                // Dust!
                for (int i = 0; i < 40; i++)
                {
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood);
                    Main.dust[d].velocity *= 2.5f;
                    Main.dust[d].scale += 0.5f;
                }
                for (int i = 0; i < 20; i++)
                {
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Vortex, Scale: 2f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].noLight = true;
                    Main.dust[d].velocity *= 9f;
                }

                return;
            }

            // Clone mutant values
            NPC.target = mutant.target;
            NPC.damage = mutant.damage;
            NPC.defDamage = mutant.damage;
            NPC.frame.Y = mutant.frame.Y;

            // Mirror the real Mutant's movements
            if (NPC.HasValidTarget)
            {
                Vector2 target = Main.player[mutant.target].Center;
                Vector2 distance = target - mutant.Center;
                NPC.Center = target;
                NPC.position.X += distance.X * DirectionX;
                NPC.position.Y += distance.Y * DirectionY;
                NPC.direction = NPC.spriteDirection = MathF.Sign(Main.player[NPC.target].Center.X - NPC.position.X);
            }
            // Otherwise hide on real Mutant
            else
                NPC.Center = mutant.Center;

            // Shoot when it's time
            if (--ShootDelayTimer == 0)
            {
                // Determine pillar type
                int projAI0;
                if (DirectionX < 0)
                    projAI0 = 0;
                else if (DirectionY < 0)
                    projAI0 = 1;
                else
                    projAI0 = 2;
                if (FargoSoulsUtil.HostCheck)
                    Projectile.NewProjectile(mutant.GetSource_FromThis(), NPC.Center, Vector2.UnitY * -5, ModContent.ProjectileType<MutantPillar>(), FargoSoulsUtil.ScaledProjectileDamage(mutant.damage, 4f / 3), 0, Main.myPlayer, projAI0, NPC.whoAmI);
            }

            // Makes the mutant faster
            if (Main.getGoodWorld && ++Timer > MutantBoss.HyperMax + 1)
            {
                Timer = 0;
                NPC.AI();
            }

            Timer++;
        }

        public override bool CheckActive() => false;

        public override bool PreKill() => false;
    }
}