using FargowiltasSouls.Assets.ExtraTextures;
using FargowiltasSouls.Content.Projectiles.Deathrays;
using FargowiltasSouls.Core.Systems;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles
{
    public class MutantDeathrayAim : BaseDeathray, IPixelatedPrimitiveRenderer
    {
        public override string Texture => "FargowiltasSouls/Content/Projectiles/Deathrays/PhantasmalDeathrayML";
        public MutantDeathrayAim() : base(69) { }

        public override bool? CanDamage()
        {
            return false;
        }

        public ref float MutantIndex => ref Projectile.ai[0];
        public ref float TrackingStrength => ref Projectile.ai[1];
        public ref float Duration => ref Projectile.ai[2];
        public ref float Timer => ref Projectile.localAI[0];
        public ref float LAI1 => ref Projectile.localAI[1];
        public ref float LAI2 => ref Projectile.localAI[2];

        public override void AI()
        {
            if (Timer >= Duration || WorldSavingSystem.MasochistModeReal)
            {
                Projectile.Kill();
                return;
            }

            NPC npc = FargoSoulsUtil.NPCExists(MutantIndex, ModContent.NPCType<MutantBoss>());
            if (npc != null && !npc.dontTakeDamage)
                Projectile.Center = npc.Center;

            Projectile.scale = MathF.Sin(Timer * MathHelper.Pi / Duration) * 0.2f;
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.One);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, npc.SafeDirectionTo(Main.player[npc.target].Center + Main.player[npc.target].velocity * TrackingStrength), 0.2f);
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            Timer++;
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

            // This allows the drawing to be pushed back, which is needed due to the shader fading in at the start to avoid sharp lines.
            Vector2 initialDrawPoint = Projectile.Center - Projectile.velocity * 150f;
            Vector2[] baseDrawPoints = new Vector2[8];
            for (int i = 0; i < baseDrawPoints.Length; i++)
                baseDrawPoints[i] = Vector2.Lerp(initialDrawPoint, laserEnd, i / (float)(baseDrawPoints.Length - 1f));

            // Set shader parameters. This one takes a fademap and a color.
            FargosTextureRegistry.MutantStreak.Value.SetTexture1();
            shader.TrySetParameter("mainColor", FargoSoulsUtil.AprilFools ? new Color(255, 255, 183, 100) : new Color(183, 252, 253, 100));
            shader.TrySetParameter("stretchAmount", 3);
            shader.TrySetParameter("scrollSpeed", 2f);
            shader.TrySetParameter("uColorFadeScaler", 1f);
            shader.TrySetParameter("useFadeIn", true);

            PrimitiveRenderer.RenderTrail(baseDrawPoints, new(WidthFunction, ColorFunction, Pixelate: true, Shader: shader), 20);
        }
    }
}