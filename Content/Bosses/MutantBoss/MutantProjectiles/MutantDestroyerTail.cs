using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public class MutantDestroyerTail : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.AprilFools ?
            "FargowiltasSouls/Content/Bosses/MutantBoss/TextureAlts/MutantDestroyerTail_April" :
            "FargowiltasSouls/Assets/ExtraTextures/Resprites/NPC_136";

        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 900;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.netImportant = true;
            Projectile.hide = true;
            CooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
            writer.Write(Projectile.localAI[1]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
            Projectile.localAI[1] = reader.ReadSingle();
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Appearance == 0 ? ModContent.Request<Texture2D>(Texture).Value : ModContent.Request<Texture2D>("FargowiltasSouls/Assets/ExtraTextures/Resprites/NPC_15").Value;
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() / 2f, Projectile.scale, spriteEffects, 0);

            return false;
        }

        public ref float AI0 => ref Projectile.ai[0];
        public ref float AI1 => ref Projectile.ai[1];
        public ref float Appearance => ref Projectile.ai[2];
        public ref float WormIndex => ref Projectile.localAI[0];
        public ref float LAI1 => ref Projectile.localAI[1];
        public ref float Timer => ref Projectile.localAI[2];

        public override void AI()
        {
            // Update every once in a while
            if ((int)Main.time % 120 == 0)
                Projectile.netUpdate = true;

            for (int i = 0; i < 2; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100, Scale: 2f);
                Main.dust[d].noGravity = true;
            }

            // Identify the segment ahead and extract some info. If there's no segment, perish
            int byIdentity = FargoSoulsUtil.GetProjectileByIdentity(Projectile.owner, (int)Projectile.ai[0], ModContent.ProjectileType<MutantDestroyerBody>());
            Vector2 nextDestroyerSegmentPos;
            float nextDestroyerSegmentRot;
            if (byIdentity >= 0 && Main.projectile[byIdentity].active)
            {
                nextDestroyerSegmentPos = Main.projectile[byIdentity].Center;
                nextDestroyerSegmentRot = Main.projectile[byIdentity].rotation;
                Main.projectile[byIdentity].localAI[0] = WormIndex + 1f;
                Projectile.timeLeft = Main.projectile[byIdentity].timeLeft;
            }
            else
            {
                Projectile.Kill();
                return;
            }

            // Fade in
            Projectile.alpha = (int)MathHelper.Lerp(255f, 0f, Timer / 12f);

            // Adjust the tail position to be right before the other destroyer
            Projectile.velocity = Vector2.Zero;
            float angle = MathHelper.WrapAngle(nextDestroyerSegmentRot - Projectile.rotation);
            Vector2 tailPos = (nextDestroyerSegmentPos - Projectile.Center).RotatedBy(angle * 0.1f);
            Projectile.rotation = tailPos.ToRotation() + MathHelper.PiOver2;
            Projectile.position = Projectile.Center;
            Projectile.width = Projectile.height = (int)(30 * Projectile.scale);
            Projectile.Center = Projectile.position;
            Projectile.spriteDirection = tailPos.X > 0f ? 1 : -1;
            if (nextDestroyerSegmentPos != Vector2.Zero)
                Projectile.Center = nextDestroyerSegmentPos - tailPos.SafeNormalize(Vector2.UnitX) * 36;

            Timer++;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 40; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch,
                    -Projectile.velocity.X * 0.2f, -Projectile.velocity.Y * 0.2f, 100, Scale: 2f);
                Main.dust[d].noGravity = Main.rand.NextBool();
                Main.dust[d].velocity *= 2f;
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(Appearance == 0 ? ModContent.BuffType<LightningRodBuff>() : BuffID.Weak, Main.rand.Next(300, 1200));
            if (WorldSavingSystem.EternityMode)
                target.AddBuff(ModContent.BuffType<MutantFangBuff>(), 180);
        }
    }
}