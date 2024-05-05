using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public class MutantFragment : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.AprilFools ?
            "FargowiltasSouls/Content/Bosses/MutantBoss/TextureAlts/MutantFragment_April" :
            "FargowiltasSouls/Content/Projectiles/Masomode/CelestialFragment";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = -1;
            Projectile.scale = 1.25f;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            CooldownSlot = 1;
        }

        public ref float AI0 => ref Projectile.ai[0];
        public ref float AI1 => ref Projectile.ai[1];
        public ref float AI2 => ref Projectile.ai[2];
        public ref float LAI0 => ref Projectile.localAI[0];
        public ref float LAI1 => ref Projectile.localAI[1];
        public ref float RitualID => ref Projectile.localAI[2];

        public override void AI()
        {
            // Slow down
            Projectile.velocity *= 0.985f;
            Projectile.rotation += Projectile.velocity.X / 30f;
            Projectile.frame = (int)AI0;

            if (Main.rand.NextBool(15))
            {
                var type = (int)AI0 switch
                {
                    0 => 242,
                    1 => 127,
                    2 => 229,
                    _ => 135,
                };
                Dust dust = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, type)];
                dust.velocity *= 4f;
                dust.fadeIn = 1f;
                dust.scale = 1f + Main.rand.NextFloat() + Main.rand.Next(4) * 0.3f;
                dust.noGravity = true;
            }

            // Identify the ritual client-side
            if (RitualID == -1)
            {
                // Give up after the first try
                RitualID = -2;

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<MutantArena>())
                    {
                        RitualID = i;
                        break;
                    }
                }
            }

            // Die when outside the ritual
            Projectile ritual = FargoSoulsUtil.ProjectileExists(RitualID, ModContent.ProjectileType<MutantArena>());
            if (ritual != null && Projectile.Distance(ritual.Center) > 1200f)
                Projectile.timeLeft = 0;
        }

        public override void OnKill(int timeLeft)
        {
            // Identify the dust type
            var type = (int)AI0 switch
            {
                0 => 242,
                1 => 127,
                2 => 229,
                _ => 135,
            };

            // Dust!!
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, type, 0f, 0f, 0, new Color(), 1f)];
                dust.velocity *= 6f;
                dust.fadeIn = 1f;
                dust.scale = 1f + Main.rand.NextFloat() + Main.rand.Next(4) * 0.3f;
                dust.noGravity = true;
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<HexedBuff>(), 120);
            target.AddBuff(ModContent.BuffType<CurseoftheMoonBuff>(), 360);
            if (WorldSavingSystem.EternityMode)
                target.AddBuff(ModContent.BuffType<MutantFangBuff>(), 180);

            // Pillar-specific debuffs
            switch ((int)AI0)
            {
                case 0: target.AddBuff(ModContent.BuffType<ReverseManaFlowBuff>(), 180); break; // Nebula
                case 1: target.AddBuff(ModContent.BuffType<AtrophiedBuff>(), 180); break; // Solar
                case 2: target.AddBuff(ModContent.BuffType<JammedBuff>(), 180); break; // Vortex
                default: target.AddBuff(ModContent.BuffType<AntisocialBuff>(), 180); break; // Stardust
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            int frameHeight = tex.Height / Main.projFrames[Projectile.type];
            Rectangle rect = new(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, rect, Projectile.GetAlpha(lightColor),
                Projectile.rotation, rect.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
    }
}