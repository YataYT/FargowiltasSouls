using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles
{
    public class MutantBombSmall : MutantBomb
    {
        public override string Texture => $"Terraria/Images/Projectile_{(FargoSoulsUtil.AprilFools ? "687" : "645")}";

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 275;
            Projectile.height = 275;
            Projectile.scale = 0.75f;
            Projectile.FargoSouls().TimeFreezeImmune = false;
        }

        public override bool? CanDamage()
        {
            if (Projectile.frame > 2 && Projectile.frame <= 4)
            {
                Projectile.FargoSouls().GrazeCD = 1;
                return false;
            }
            return true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (ExplosionDuration == 0)
                ExplosionDuration = 21;

            if (ExplosionScale == 0)
                ExplosionScale = 0.75f;

            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            int frameHeight = tex.Height / Main.projFrames[Projectile.type];
            Rectangle rect = new(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, rect, Projectile.GetAlpha(lightColor) with { A = 210 },
                Projectile.rotation, rect.Size() / 2f, Projectile.scale * 3f, SpriteEffects.None, 0f);

            return false;
        }
    }
}