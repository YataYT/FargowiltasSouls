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

namespace FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles
{
    public class MutantDestroyerBody : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.AprilFools ?
            "FargowiltasSouls/Content/Bosses/MutantBoss/TextureAlts/MutantDestroyerBody_April" :
            "FargowiltasSouls/Assets/ExtraTextures/Resprites/NPC_135";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("The Destroyer");
            Main.projFrames[Projectile.type] = 2;
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

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Appearance == 0 ? ModContent.Request<Texture2D>(Texture).Value : ModContent.Request<Texture2D>("FargowiltasSouls/Assets/ExtraTextures/Resprites/NPC_14").Value;
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() / 2f, Projectile.scale, spriteEffects, 0);

            return false;
        }

        public ref float PreviousWormIdentity => ref Projectile.ai[0];
        public ref float AI1 => ref Projectile.ai[1];
        public ref float Appearance => ref Projectile.ai[2];
        public ref float WormIndex => ref Projectile.localAI[0];
        public ref float idk => ref Projectile.localAI[1];
        public ref float Timer => ref Projectile.localAI[2];

        public override void AI()
        {
            if (Timer % 120 == 0)
                Projectile.netUpdate = true;

            // Identify the segment ahead and extract some info. If there's no segment, perish
            int identity = FargoSoulsUtil.GetProjectileByIdentity(Projectile.owner, PreviousWormIdentity, Projectile.type, ModContent.ProjectileType<MutantDestroyerHead>());
            Vector2 nextSegmentPos;
            float nextSegmentRot;
            if (identity >= 0 && Main.projectile[identity].active)
            {
                nextSegmentPos = Main.projectile[identity].Center;
                nextSegmentRot = Main.projectile[identity].rotation;
                Main.projectile[identity].localAI[0] = WormIndex + 1;
                Projectile.timeLeft = Main.projectile[identity].timeLeft;

                if (Main.projectile[identity].type != ModContent.ProjectileType<MutantDestroyerHead>())
                    Main.projectile[identity].localAI[1] = Projectile.identity;
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
            float angle = MathHelper.WrapAngle(nextSegmentRot - Projectile.rotation);
            Vector2 tailPos = (nextSegmentPos - Projectile.Center).RotatedBy(angle * 0.1f);
            Projectile.rotation = tailPos.ToRotation() + MathHelper.PiOver2;
            Projectile.position = Projectile.Center;
            Projectile.width = Projectile.height = (int)(30 * Projectile.scale);
            Projectile.Center = Projectile.position;
            Projectile.spriteDirection = tailPos.X > 0f ? 1 : -1;
            if (nextSegmentPos != Vector2.Zero)
                Projectile.Center = nextSegmentPos - tailPos.SafeNormalize(Vector2.UnitX) * 36;

            Timer++;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 40; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Main.rand.NextBool() ? DustID.PurpleTorch : DustID.RedTorch,
                    -Projectile.velocity.X * 0.2f, -Projectile.velocity.Y * 0.2f, 100, Scale: Main.rand.NextFloat(1f, 2f));
                Main.dust[d].noGravity = Main.rand.NextBool();
                Main.dust[d].velocity *= 2f;
            }

            if (!Main.dedServ)
            {
                // 156 is Destroyer gore, 25 is EoW gore
                int g = Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity / 2,
                    Projectile.ai[2] == 0 ? 156 : 25, Projectile.scale);
                Main.gore[g].timeLeft = 20;
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