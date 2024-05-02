using FargowiltasSouls.Assets.ExtraTextures;
using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Content.Projectiles.Deathrays;
using FargowiltasSouls.Core.Systems;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public class MutantDeathray : MutantSpecialDeathray, IPixelatedPrimitiveRenderer
    {
        public MutantDeathray() : base(60) { }

        public override bool? CanDamage()
        {
            return Projectile.scale >= 1;
        }

        public override bool CanHitPlayer(Player target)
        {
            return target.hurtCooldowns[1] == 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Zombie104, Projectile.Center);
            Projectile.frame = Main.rand.Next(16);

            // Default value
            if (Duration == 0)
                Duration = 60;
        }

        public ref float Duration => ref Projectile.ai[0];
        public ref float AI1 => ref Projectile.ai[1];
        public ref float AI2 => ref Projectile.ai[2];
        public ref float Timer => ref Projectile.localAI[0];
        public ref float LaserLength => ref Projectile.localAI[1];
        public ref float LAI2 => ref Projectile.localAI[2];

        public override void AI()
        {
            base.AI();

            if (Projectile.velocity.HasNaNs() || Projectile.velocity == Vector2.Zero)
                Projectile.velocity = -Vector2.UnitY;

            // Kill the projectile when its exceeded its lifetime
            if (Timer >= Duration)
            {
                Projectile.Kill();
                return;
            }

            // Oscillate the scale a bit
            Projectile.scale = MathF.Sin(Timer * MathHelper.Pi / Duration) * 4f;
            if (Projectile.scale > 1)
                Projectile.scale = 1;

            // Values
            float maxLaserLength = 1500;
            LaserLength = MathHelper.Lerp(LaserLength, maxLaserLength, 0.1f);
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            Timer++;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (WorldSavingSystem.EternityMode)
            {
                target.FargoSouls().MaxLifeReduction += 100;
                target.AddBuff(ModContent.BuffType<OceanicMaulBuff>(), 5400);
                target.AddBuff(ModContent.BuffType<MutantFangBuff>(), 180);
            }
            target.AddBuff(ModContent.BuffType<CurseoftheMoonBuff>(), 600);
        }

        public override bool PreDraw(ref Color lightColor) => false;

        public float WidthFunction(float trailInterpolant) => Projectile.width * Projectile.scale * 1.3f;

        public static Color ColorFunction(float trailInterpolant)
        {
            Color color = FargoSoulsUtil.AprilFools ? Color.Red : Color.Cyan;
            color.A = 100;
            return color;
        }
        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            if (Projectile.hide)
                return;

            ManagedShader shader = ShaderManager.GetShader("FargowiltasSouls.GenericDeathray");

            // Get the laser end position.
            Vector2 laserEnd = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitY) * drawDistance * 1.1f;

            // Create 8 points that span across the draw distance from the projectile center.

            // This allows the drawing to be pushed back, which is needed due to the shader fading in at the start to avoid
            // sharp lines.
            Vector2 initialDrawPoint = Projectile.Center - Projectile.velocity * 150f;
            Vector2[] baseDrawPoints = new Vector2[8];
            for (int i = 0; i < baseDrawPoints.Length; i++)
                baseDrawPoints[i] = Vector2.Lerp(initialDrawPoint, laserEnd, i / (float)(baseDrawPoints.Length - 1f));

            // GameShaders.Misc["FargoswiltasSouls:MutantDeathray"].UseImage1(); cannot be used due to only accepting vanilla paths.
            FargosTextureRegistry.MutantStreak.Value.SetTexture1();
            // The laser should fade to this in the middle.
            shader.TrySetParameter("mainColor", FargoSoulsUtil.AprilFools ? new Color(255, 255, 183, 100) : new Color(183, 252, 253, 100));
            shader.TrySetParameter("stretchAmount", 3);
            shader.TrySetParameter("scrollSpeed", 2f);
            shader.TrySetParameter("uColorFadeScaler", 1f);
            shader.TrySetParameter("useFadeIn", true);

            PrimitiveRenderer.RenderTrail(baseDrawPoints, new(WidthFunction, ColorFunction, Pixelate: true, Shader: shader), 25);
        }
    }
}