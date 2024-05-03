using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public class MutantGuardian : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.AprilFools ?
            "FargowiltasSouls/Content/Bosses/MutantBoss/TextureAlts/MutantGuardian_April" :
            "FargowiltasSouls/Assets/ExtraTextures/Resprites/NPC_127";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = Main.npcFrameCount[NPCID.SkeletronPrime];
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 70;
            Projectile.height = 70;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            CooldownSlot = 1;

            Projectile.timeLeft = 240;
            Projectile.hide = true;
            Projectile.light = 0.5f;

            Projectile.FargoSouls().DeletionImmuneRank = 1;
        }

        public override bool CanHitPlayer(Player target)
        {
            return target.hurtCooldowns[1] == 0;
        }

        public override void OnSpawn()
        {
            Projectile.rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);
            Projectile.hide = false;

            for (int i = 0; i < 5; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Alpha: 100, Scale: 2f);
                Main.dust[d].noGravity = true;
            }
        }

        public ref float AI0 => ref Projectile.ai[0];
        public ref float AI1 => ref Projectile.ai[1];
        public ref float AI2 => ref Projectile.ai[2];
        public ref float LAI0 => ref Projectile.localAI[0];
        public ref float LAI1 => ref Projectile.localAI[1];
        public ref float LAI2 => ref Projectile.localAI[2];

        public override void AI()
        {
            Projectile.frame = 2;
            Projectile.direction = Projectile.velocity.X < 0 ? -1 : 1;
            Projectile.rotation += Projectile.direction * 0.3f;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // Lots of debuffs
            if (WorldSavingSystem.EternityMode)
            {
                target.FargoSouls().MaxLifeReduction += 100;
                target.AddBuff(ModContent.BuffType<OceanicMaulBuff>(), 5400);
                if (WorldSavingSystem.MasochistModeReal)
                    target.AddBuff(ModContent.BuffType<GodEaterBuff>(), 420);
                target.AddBuff(ModContent.BuffType<FlamesoftheUniverseBuff>(), 420);
                target.AddBuff(ModContent.BuffType<MarkedforDeathBuff>(), 420);
                target.AddBuff(ModContent.BuffType<MutantFangBuff>(), 180);
            }
            target.AddBuff(ModContent.BuffType<DefenselessBuff>(), 480);
        }

        public override void OnKill(int timeLeft)
        {
            // Dust!!!
            for (int i = 0; i < 5; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0, 0, 100, default, 2f);
                Main.dust[dust].noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Utilities.GetAfterimagesCentered(Projectile, smth);

            return false;
        }
    }
}

