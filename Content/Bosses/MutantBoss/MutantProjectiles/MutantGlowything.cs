using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public class MutantGlowything : ModProjectile
    {
        public override string Texture => "FargowiltasSouls/Content/Bosses/MutantBoss/MutantProjectiles/MutantGlowything";

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1;
            Projectile.scale = 0.5f;
            Projectile.alpha = 0;
            CooldownSlot = 1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SpawnPointX = Projectile.Center.X;
            SpawnPointY = Projectile.Center.Y;
        }

        public ref float PointDirection => ref Projectile.ai[0];
        public ref float MutantIndex => ref Projectile.ai[1];
        public ref float AI2 => ref Projectile.ai[2];
        public ref float LAI0 => ref Projectile.localAI[0];
        public ref float SpawnPointX => ref Projectile.localAI[1];
        public ref float SpawnPointY => ref Projectile.localAI[2];

        public override void AI()
        {
            Vector2 spawnPoint = new(SpawnPointX, SpawnPointY);

            // Adjust rotation and center
            Projectile.rotation = PointDirection;
            Projectile.Center = spawnPoint + Vector2.UnitX.RotatedBy(PointDirection) * 96 * Projectile.scale;

            // Grow over time
            if (Projectile.scale < 4f)
                Projectile.scale += 0.2f;

            // When full size, start fading away
            else
            {
                Projectile.scale = 4f;
                Projectile.alpha += 10;
            }

            // Die if fully faded away
            if (Projectile.alpha > 255)
                Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle rect = new(0, 0, glow.Width, glow.Height);

            Main.EntitySpriteDraw(glow, Projectile.Center + Projectile.Size / 2f - Main.screenPosition, rect, Projectile.GetAlpha(Color.Red),
                Projectile.rotation, rect.Size() / 2f, Projectile.scale * 2, SpriteEffects.None, 0);

            return false;
        }
    }
}