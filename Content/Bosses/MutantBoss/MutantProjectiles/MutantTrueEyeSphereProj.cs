using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles
{
    public class MutantTrueEyeSphereProj : BaseMutantSphere
    {
        public override float ScaleMultiplier => 1.2f;

        public override string Texture => FargoSoulsUtil.AprilFools ?
            "FargowiltasSouls/Content/Bosses/MutantBoss/TextureAlts/MutantTrueEyeSphere_April" :
            "Terraria/Images/Projectile_454";

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.timeLeft = 360;
        }

        public ref float TrueEyeIdentity => ref Projectile.ai[0];

        public override void AI()
        {
            int byIdentity = FargoSoulsUtil.GetProjectileByIdentity(Projectile.owner, (int)TrueEyeIdentity, ModContent.ProjectileType<MutantTrueEyeSphere>());

            if (byIdentity != -1)
            {
                // Timed to shoot right as the True Eye rams
                if (Projectile.timeLeft > 295)
                {
                    // Stop following true eye if true eye has lost the target and isn't preparing to charge
                    if (Main.projectile[byIdentity].ai[1] == 0f) 
                    {
                        Projectile.ai[0] = -1f;
                        Projectile.velocity = Vector2.Zero;
                        Projectile.netUpdate = true;
                    }
                    else
                        Projectile.velocity = Main.projectile[byIdentity].velocity;
                }
            }

            // Fade in
            float fadeInTime = 40f;
            Projectile.Opacity = Utilities.InverseLerp(0f, fadeInTime, Timer);
            Projectile.scale = Utilities.InverseLerp(0f, fadeInTime, Timer);

            // Update frame
            if (++Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame > 1)
                    Projectile.frame = 0;
            }

            Timer++;
        }
    }
}