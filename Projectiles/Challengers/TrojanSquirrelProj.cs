using FargowiltasSouls.Buffs.Masomode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Projectiles.Challengers
{
    public class TrojanSquirrelProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Top Hat Squirrel");
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Projectile.velocity.Y += Projectile.ai[0];

            if (--Projectile.ai[1] <= 0 && !Projectile.tileCollide)
            {
                if (!Collision.SolidCollision(Projectile.Center, 0, 0))
                    Projectile.tileCollide = true;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Projectile.localAI[0] == 0)
                Projectile.localAI[0] = Main.rand.NextBool() ? 1 : -1;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;

            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (FargoSoulsWorld.EternityMode)
                target.AddBuff(ModContent.BuffType<Guilty>(), 120);
        }

        public override void Kill(int timeLeft)
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);

            for (int k = 0; k < 20; k++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0f, -1f);

            if (!Main.dedServ)
            {
                int g = Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, -0.1f * Projectile.oldVelocity.RotatedByRandom(MathHelper.PiOver4), ModContent.Find<ModGore>(Mod.Name, $"TrojanSquirrelGore2").Type, Projectile.scale);
                Main.gore[g].rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new Rectangle(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;

            SpriteEffects effects = Projectile.localAI[0] > 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, effects, 0);
            return false;
        }
    }
}