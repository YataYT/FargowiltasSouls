using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using UtfUnknown.Core.Models.SingleByte.Finnish;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public class MutantEyeHoming : MutantEye
    {
        public override string Texture => FargoSoulsUtil.AprilFools ?
            "FargowiltasSouls/Content/Bosses/MutantBoss/TextureAlts/MutantEye_April" :
            "Terraria/Images/Projectile_452";

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.timeLeft = 900;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Makes setting AI2 optional
            if (SpeedBonus == 0)
                SpeedBonus = 1;
        }

        /*public ref float AI0 => ref Projectile.ai[0];
        public ref float AI1 => ref Projectile.ai[1];
        public ref float AI2 => ref Projectile.ai[2];
        public ref float LAI0 => ref Projectile.localAI[0];
        public ref float LAI1 => ref Projectile.localAI[1];
        public ref float LAI2 => ref Projectile.localAI[2];*/

        public ref float BackupPlayerTarget => ref Projectile.ai[0];
        public ref float SpeedBonus => ref Projectile.ai[1];
        public ref float HomingTimer => ref Projectile.ai[2];

        public override void AI()
        {
            float maxSpeed = WorldSavingSystem.MasochistModeReal ? 15f : 10f;
            bool stopChasing = false;
            int endHomingTime = -600;

            NPC mutant = FargoSoulsUtil.NPCExists(EModeGlobalNPC.mutantBoss, ModContent.NPCType<MutantBoss>());
            Player player = FargoSoulsUtil.PlayerExists(mutant == null ? Projectile.ai[0] : mutant.target);

            // Stop chasing if any of these are true
            if (mutant is null && !Main.getGoodWorld && !WorldSavingSystem.MasochistModeReal)
            {
                HomingTimer = endHomingTime;
                stopChasing = true;
            }

            // Fly away?
            if (stopChasing || HomingTimer < 0 && player is not null && Projectile.Distance(player.Center) < 240)
            {
                float angle = MathHelper.WrapAngle(Projectile.DirectionFrom(player.Center).ToRotation() - Projectile.velocity.ToRotation());
                Projectile.velocity = Projectile.velocity.RotatedBy(angle * 0.05f);
            } 
            // Home in on player
            else if (HomingTimer < 0 && HomingTimer > endHomingTime && player is not null)
            {
                float homingMaxSpeed = maxSpeed * SpeedBonus;

                if (Projectile.velocity.Length() < homingMaxSpeed)
                    Projectile.velocity *= 1.02f;

                Vector2 target = player.Center;
                float deactivateHomingRange = WorldSavingSystem.MasochistModeReal ? 360 : 480;
                if (Projectile.Distance(target) > deactivateHomingRange)
                {
                    Vector2 distance = target - Projectile.Center;
                    float angle = MathHelper.WrapAngle(distance.ToRotation() - Projectile.velocity.ToRotation());
                    Projectile.velocity = Projectile.velocity.RotatedBy(angle * 0.1f);
                }
                // Stop homing
                else
                {
                    HomingTimer = endHomingTime;
                }
            }

            if (HomingTimer < endHomingTime && !Main.getGoodWorld)
            {
                if (Projectile.velocity.Length() > maxSpeed)
                    Projectile.velocity *= 0.96f;


                // Cut down the lifespan
                if (Projectile.timeLeft > 120)
                    Projectile.timeLeft = 120;
            } 

            HomingTimer--;

            base.AI();
        }
    }
}