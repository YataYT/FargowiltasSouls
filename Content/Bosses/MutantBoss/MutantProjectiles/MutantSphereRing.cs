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
    public class MutantSphereRing : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.AprilFools ?
            "FargowiltasSouls/Content/Bosses/MutantBoss/TextureAlts/MutantSphere_April" :
            "Terraria/Images/Projectile_454";

        protected virtual bool DieOutsideArena => true;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 480;
            Projectile.alpha = 0;
            CooldownSlot = 1;
        }

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
        public ref float Timer => ref Projectile.localAI[0];
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

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.mutantBoss, ModContent.NPCType<MutantBoss>()))
            {
                if (WorldSavingSystem.EternityMode)
                {
                    target.FargoSouls().MaxLifeReduction += 100;
                    target.AddBuff(ModContent.BuffType<OceanicMaulBuff>(), 5400);
                    target.AddBuff(ModContent.BuffType<MutantFangBuff>(), 180);
                }
            }
            target.AddBuff(ModContent.BuffType<CurseoftheMoonBuff>(), 360);
        }

        public override void OnKill(int timeleft)
        {
            // Play the death sound randomly, taking into account how many spheres are alive to control volume levels
            if (Main.rand.NextBool(Main.player[Projectile.owner].ownedProjectileCounts[Projectile.type] / 10 + 1))
                SoundEngine.PlaySound(SoundID.NPCDeath6, Projectile.Center);

            for (int i = 0; i < 2; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Alpha: 100, Scale: 1.5f);

            for (int i = 0; i < 4; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Vortex, Alpha: Main.rand.Next(100), Scale: Main.rand.NextFloat(1.5f, 2.5f));
                Main.dust[d].noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = ModContent.Request<Texture2D>("FargowiltasSouls/Content/Bosses/MutantBoss/MutantProjectiles/MutantSphereGlow").Value;
            Color glowColor = (FargoSoulsUtil.AprilFools ? Color.Red : new Color(196, 247, 255, 0)) * 0.9f * Projectile.Opacity;
            Rectangle rect = new(0, 0, glow.Width, glow.Height);

            int trailCacheLength = ProjectileID.Sets.TrailCacheLength[Projectile.type];
            for (int i = 0; i < trailCacheLength; i++)
            {
                float afterimageRatio = (trailCacheLength - i) / trailCacheLength;
                Color afterimageColor = glowColor * afterimageRatio;
                float scale = Projectile.scale * afterimageRatio;
                Vector2 afterimagePos = Projectile.oldPos[i] - Vector2.Normalize(Projectile.velocity) * i * trailCacheLength + Projectile.Size / 2f;
                Main.spriteBatch.Draw(glow, afterimagePos - Main.screenPosition, rect, afterimageColor, Projectile.rotation, rect.Size() / 2f, scale * 1.5f, SpriteEffects.None, 0);
            }

            Main.spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition, rect, Color.White * 0.85f, Projectile.rotation,
                rect.Size() / 2f, Projectile.scale * 1.5f, SpriteEffects.None, 0);

            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int frameHeight = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
            Rectangle rect = new(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, rect, Color.White * Projectile.Opacity,
                Projectile.rotation, rect.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
        }
    }
}