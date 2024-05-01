using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Content.Buffs.Souls;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.Systems;
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
    public class MutantSphereRing : BaseMutantSphere
    {
        protected virtual bool DieOutsideArena => MutantPhase != 0;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
        }

        public override float ScaleMultiplier => 1.25f;

        public override bool CanHitPlayer(Player target)
        {
            return target.hurtCooldowns[1] == 0 || WorldSavingSystem.MasochistModeReal;
        }

        public override void OnSpawn(IEntitySource source)
        {
            RitualID = -1;
            OriginalSpeed = Projectile.velocity.Length();
        }

        public ref float RotationModifier => ref Projectile.ai[0];
        public ref float Speed => ref Projectile.ai[1];
        public ref float MutantPhase => ref Projectile.ai[2];
        public ref float OriginalSpeed => ref Projectile.localAI[1];
        public ref float RitualID => ref Projectile.localAI[2];

        public override void AI()
        {
            // Cycle frames
            if (++Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame > 1)
                    Projectile.frame = 0;
            }

            Timer++;

            // Update the velocity to move in a ring-like pattern
            Projectile.velocity = OriginalSpeed * Projectile.velocity.SafeNormalize(Vector2.One).RotatedBy(Speed / (MathHelper.TwoPi * RotationModifier * Timer));

            // Fade in
            float fadeInTime = 10f;
            Projectile.Opacity = Utilities.InverseLerp(0f, fadeInTime, Timer);
            Projectile.scale = Utilities.InverseLerp(0f, fadeInTime, Timer);
            //Projectile.Opacity = Projectile.scale = 1f;
            /*
            if (DieOutsideArena)
            {
                if (RitualID == -1)
                {
                    // Give up if cannot find the ritual projectile
                    RitualID = -2;
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<MutantRitual>())
                        {
                            RitualID = i;
                            break;
                        }
                    }
                }

                Projectile ritual = FargoSoulsUtil.ProjectileExists(RitualID, ModContent.ProjectileType<MutantRitual>());
                if (ritual != null && Projectile.Distance(ritual.Center) > 1200f)
                    Projectile.Kill();
            }*/

            // If in masomode and desperation phase, the player will get frozen on hit
            TryTimeStop();
        }

        private void TryTimeStop()
        {
            if (Main.LocalPlayer.active && !Main.LocalPlayer.dead && !Main.LocalPlayer.ghost
                && FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.mutantBoss, ModContent.NPCType<MutantBoss>()))
            {
                // Only triggered in Maso + Desperation Phase
                if (WorldSavingSystem.MasochistModeReal && MutantPhase == 2 && Projectile.Colliding(Projectile.Hitbox, Main.LocalPlayer.FargoSouls().GetPrecisionHurtbox()))
                {
                    if (!Main.LocalPlayer.HasBuff(ModContent.BuffType<TimeFrozenBuff>()))
                        SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/ZaWarudo"), Main.LocalPlayer.Center);

                    Main.LocalPlayer.AddBuff(ModContent.BuffType<TimeFrozenBuff>(), 300);
                }
            }
        }
    }
}