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
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles
{
    public class MutantMechDeathray : BaseDeathray, IPixelatedPrimitiveRenderer
    {

        public override string Texture => "FargowiltasSouls/Content/Projectiles/Deathrays/PhantasmalDeathray";
        public MutantMechDeathray() : base(2700, grazeCD: 30) { }

        public override void SetDefaults()
        {
            base.SetDefaults();
            
            CooldownSlot = -1; //iframe interaction with prime lol
        }

        public override void OnSpawn()
        {
            // Setting default value (I don't like BaseDeathray)
            if (MaxTime == 0)
                MaxTime = 270;
        }

        public ref float AngleIncrement => ref Projectile.ai[0];
        public ref float MutantIndex => ref Projectile.ai[1];
        public ref float MaxTime => ref Projectile.ai[2];
        public ref float Timer => ref Projectile.localAI[0];
        public ref float LaserLength => ref Projectile.localAI[1];
        public ref float DisplayMaxTime => ref Projectile.localAI[2];

        public override void AI()
        {
            NPC mutant = FargoSoulsUtil.NPCExists(MutantIndex, ModContent.NPCType<MutantBoss>());

            // Make sure the velocity is valid since it's used for rotating the deathray.
            if (Projectile.velocity.HasNaNs() || Projectile.velocity == Vector2.Zero)
                Projectile.velocity = -Vector2.UnitY;

            // Determine how much to decelerate
            float decelerationValue = WorldSavingSystem.MasochistModeReal ? 0.9716f : 0.9712f;

            if (Timer == 0)
            {
                if (!Main.dedServ)
                    SoundEngine.PlaySound(SoundID.Zombie104 with { Volume = 0.5f }, Projectile.Center);

                DisplayMaxTime = Math.Min(MaxTime, Projectile.timeLeft + 2);
            }

            if (Timer >= MaxTime)
            {
                Projectile.Kill();
                return;
            }

            // Adjust scale
            Projectile.scale = MathF.Sin(Timer * MathHelper.Pi / DisplayMaxTime) * 6f;
            if (Projectile.scale > 1f)
                Projectile.scale = 1f;

            // Update the deathray angle
            float currentAngle = Projectile.velocity.ToRotation();
            if (Timer > 45f && Timer < MaxTime - 120f)
                AngleIncrement *= decelerationValue;
            currentAngle += AngleIncrement;
            Projectile.rotation = currentAngle - MathHelper.PiOver2;
            Projectile.velocity = currentAngle.ToRotationVector2();

            // Dust!!!
            if (Main.rand.NextBool(5))
            {
                Vector2 dustOffset = Projectile.velocity.RotatedBy(MathHelper.PiOver2) * (Main.rand.NextFloat() - 0.5f) * Projectile.width;
                int d = Dust.NewDust(Projectile.Center + Projectile.velocity * (LaserLength - 14f) + dustOffset - Vector2.One * 4f, 8, 8, DustID.CopperCoin, Alpha: 100, Scale: 1.5f);
                Main.dust[d].velocity *= 0.5f;
                Main.dust[d].velocity.Y = -MathF.Abs(Main.dust[d].velocity.Y);
            }

            Timer++;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (WorldSavingSystem.EternityMode)
            {
                target.FargoSouls().MaxLifeReduction += 100;
                target.AddBuff(ModContent.BuffType<OceanicMaulBuff>(), 5400);
                target.AddBuff(ModContent.BuffType<MutantFangBuff>(), 180);
                target.AddBuff(BuffID.Burning, 300);
            }
            target.AddBuff(BuffID.OnFire, 300);
        }

        public override bool PreDraw(ref Color lightColor) => false;

        public float WidthFunction(float trailInterpolant) => Projectile.width * Projectile.scale * 1.3f;

        public static Color ColorFunction(float trailInterpolant) => new(255, 0, 0, 0);

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
            Vector2 initialDrawPoint = Projectile.Center - Projectile.velocity * 50f;
            Vector2[] baseDrawPoints = new Vector2[8];
            for (int i = 0; i < baseDrawPoints.Length; i++)
                baseDrawPoints[i] = Vector2.Lerp(initialDrawPoint, laserEnd, i / (float)(baseDrawPoints.Length - 1f));

            // GameShaders.Misc["FargoswiltasSouls:MutantDeathray"].UseImage1(); cannot be used due to only accepting vanilla paths.
            FargosTextureRegistry.MutantStreak.Value.SetTexture1();
            shader.TrySetParameter("mainColor", new Color(255, 255, 183, 100));
            shader.TrySetParameter("stretchAmount", 1);
            shader.TrySetParameter("scrollSpeed", 3f);
            shader.TrySetParameter("uColorFadeScaler", 1f);
            shader.TrySetParameter("useFadeIn", true);

            PrimitiveRenderer.RenderTrail(baseDrawPoints, new(WidthFunction, ColorFunction, Pixelate: true, Shader: shader), 30);
        }

        public override bool ShouldUpdatePosition() => false;
    }
}
