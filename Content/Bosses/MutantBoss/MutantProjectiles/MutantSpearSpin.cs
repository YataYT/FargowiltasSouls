using FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles;
using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Core.Systems;
using Microsoft.Win32.SafeHandles;
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
    public class MutantSpearSpin : ModProjectile
    {
        public ref float MutantIndex => ref Projectile.ai[0];
        public ref float SpearSpinDuration => ref Projectile.ai[1];
        // Due to lack of AI slots, this variable determines two things. Set to negative to indicate it's predictive. Set the value to how long the homing mutant eye should be alive for.
        public ref float IsPredictiveAndMutantEyeLifetime => ref Projectile.ai[2];
        public ref float MutantEyeSpawningTimer => ref Projectile.localAI[0];
        public ref float Direction => ref Projectile.localAI[1];
        public ref float CurrentPhase => ref Projectile.localAI[2];

        public override string Texture => FargoSoulsUtil.AprilFools ?
            "FargowiltasSouls/Content/Bosses/MutantBoss/TextureAlts/MutantSpear_April" :
            "FargowiltasSouls/Content/Projectiles/BossWeapons/HentaiSpear";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 152;
            Projectile.height = 152;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            CooldownSlot = 1;
            Projectile.FargoSouls().TimeFreezeImmune = true;
            Projectile.FargoSouls().DeletionImmuneRank = 2;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Direction = Main.rand.NextBool() ? 1 : -1;
            Projectile.timeLeft = (int)SpearSpinDuration;
        }

        public override void AI()
        {
            NPC MutantBoss = Main.npc[(int)MutantIndex];
            if (MutantBoss.active && MutantBoss.type == ModContent.NPCType<MutantBoss>())
            {
                // Tie the center and direction to the boss
                Projectile.Center = MutantBoss.Center;
                Projectile.rotation += 0.45f * Direction;

                // Spawn homing mutant eyes
                if (++MutantEyeSpawningTimer > 8)
                {
                    MutantEyeSpawningTimer = 0;
                    if (FargoSoulsUtil.HostCheck && Projectile.Distance(Main.player[MutantBoss.target].Center) > 360)
                    {
                        Vector2 speed = Vector2.UnitY.RotatedByRandom(MathHelper.PiOver2) * Main.rand.NextFloat(6f, 9f);
                        if (MutantBoss.Center.Y < Main.player[MutantBoss.target].Center.Y)
                            speed *= -1f;
                        float speedBonus = CurrentPhase == 1 ? 2f : 1f;
                        float homingDelay = Projectile.timeLeft - (Projectile.timeLeft / 2f);
                        int p = Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.position + Main.rand.NextVector2Square(0f, Projectile.width),
                            speed, ModContent.ProjectileType<MutantEyeHoming>(), Projectile.damage, 0f, Projectile.owner, MutantBoss.target, speedBonus, homingDelay);
                        Main.projectile[p].timeLeft = (int)MathF.Abs(IsPredictiveAndMutantEyeLifetime);
                    }
                }

                // Play a sound occasionally
                if (Projectile.timeLeft % 20 == 0)
                    SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);

                Projectile.alpha = 0;
            }
            else
            {
                Projectile.Kill();
                return;
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), target.Center + Main.rand.NextVector2Circular(100, 100), Vector2.Zero, ModContent.ProjectileType<MutantBombSmall>(), 0, 0f, Projectile.owner);
            if (WorldSavingSystem.EternityMode)
            {
                target.FargoSouls().MaxLifeReduction += 100;
                target.AddBuff(ModContent.BuffType<OceanicMaulBuff>(), 5400);
                target.AddBuff(ModContent.BuffType<MutantFangBuff>(), 180);
            }
            target.AddBuff(ModContent.BuffType<CurseoftheMoonBuff>(), 600);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            int frameHeight = tex.Height / Main.projFrames[Projectile.type];
            Rectangle rect = new(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

            int trailCacheLength = ProjectileID.Sets.TrailCacheLength[Projectile.type];
            for (int i = 0; i < trailCacheLength; i++)
            {
                float afterimageRatio = (trailCacheLength - i) / trailCacheLength;
                Color afterimageColor = Color.White * Projectile.Opacity * afterimageRatio;
                float scale = Projectile.scale * afterimageRatio;
                Main.spriteBatch.Draw(tex, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, rect, afterimageColor, Projectile.oldRot[i], rect.Size() / 2f, scale, SpriteEffects.None, 0f);
            }

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, rect, Color.White * Projectile.Opacity, Projectile.rotation, rect.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);

            Texture2D glow = ModContent.Request<Texture2D>("FargowiltasSouls/Content/Bosses/MutantBoss/MutantProjectiles/MutantSpearAimGlow").Value;
            float modifier = Projectile.timeLeft / SpearSpinDuration;
            Color glowColor = FargoSoulsUtil.AprilFools ? new Color(255, 191, 51, 210) : new(51, 255, 191, 210);
            if (IsPredictiveAndMutantEyeLifetime < 0)   // If below 0, it follows into a predictive attack
                glowColor = FargoSoulsUtil.AprilFools ? new Color(255, 0, 0, 210) : new Color(0, 0, 255, 210);
            glowColor *= 1f - modifier;
            float glowScale = Projectile.scale * 8f * modifier;
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, glow.Bounds, glowColor, 0, glow.Bounds.Size() / 2f, glowScale, SpriteEffects.None, 0);

            return false;
        }
    }
}