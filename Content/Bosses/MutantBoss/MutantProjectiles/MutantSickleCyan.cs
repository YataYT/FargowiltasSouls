using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles
{
    public class MutantSickleCyan : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.AprilFools ? "FargowiltasSouls/Content/Bosses/MutantBoss/AltTextures/MutantScythe1_April"
            : "FargowiltasSouls/Content/Bosses/MutantBoss/MutantProjectiles/MutantScythe1";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Mutant Sickle");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.alpha = 0;
            Projectile.hostile = true;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            CooldownSlot = 1;

            Projectile.hide = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write7BitEncodedInt(Projectile.timeLeft);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.timeLeft = reader.Read7BitEncodedInt();
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            targetHitbox.Y = targetHitbox.Center.Y;
            targetHitbox.Height = Math.Min(targetHitbox.Width, targetHitbox.Height);
            targetHitbox.Y -= targetHitbox.Height / 2;

            return base.Colliding(projHitbox, targetHitbox);
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public ref float AccelerationAmount => ref Projectile.ai[0];
        public ref float TargetAngle => ref Projectile.ai[1];
        public ref float AI2 => ref Projectile.ai[2];
        public ref float LAI0 => ref Projectile.localAI[0];
        public ref float LAI1 => ref Projectile.localAI[1];
        public ref float LAI2 => ref Projectile.localAI[2];

        public override void AI()
        {
            float modifier = MathHelper.Clamp((180f - Projectile.timeLeft + 90) / 180f, 0f, 1f);
            Projectile.rotation += 0.1f + 0.7f * modifier;

            if (Projectile.timeLeft < 180)
            {
                if (Projectile.velocity == Vector2.Zero)
                    Projectile.velocity = Projectile.ai[1].ToRotationVector2();

                Projectile.velocity *= 1f + AccelerationAmount;
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * Projectile.Opacity;

        public override bool PreDraw(ref Color lightColor)
        {
            Utilities.DrawAfterimagesCentered(Projectile, 2, lightColor);

            return false;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (WorldSavingSystem.EternityMode)
                target.AddBuff(ModContent.BuffType<MutantFangBuff>(), 180);
            target.AddBuff(BuffID.Bleeding, 600);
        }
    }
}