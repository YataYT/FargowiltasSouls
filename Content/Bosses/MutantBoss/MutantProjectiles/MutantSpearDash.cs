using Fargowiltas.NPCs;
using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Content.Items;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Achievements;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles
{
    public class MutantSpearDash : BaseMutantSpearAttack
    {
        public override string Texture => FargoSoulsUtil.AprilFools ?
            "FargowiltasSouls/Content/Bosses/MutantBoss/MutantSpear_April" :
            "FargowiltasSouls/Content/Projectiles/BossWeapons/HentaiSpear";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
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

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Intersects(targetHitbox))
                return true;

            float length = 200;
            float dummy = 0f;
            Vector2 offset = length / 2 * Projectile.scale * (Projectile.rotation - MathHelper.ToRadians(135f)).ToRotationVector2();
            Vector2 end = Projectile.Center - offset;
            Vector2 tip = Projectile.Center + offset;

            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), end, tip, 8f * Projectile.scale, ref dummy))
                return true;

            return false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (source is EntitySource_Parent parent && parent.Entity is NPC sourceNPC)
                MutantBoss = sourceNPC;

            SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write7BitEncodedInt(MutantBoss is NPC ? MutantBoss.whoAmI : -1);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            MutantBoss = FargoSoulsUtil.NPCExists(reader.Read7BitEncodedInt());
        }

        public ref float AI0 => ref Projectile.ai[0];
        public ref float Variant => ref Projectile.ai[1];
        public ref float AI2 => ref Projectile.ai[2];
        public ref float Timer => ref Projectile.localAI[0];
        public ref float LAI1 => ref Projectile.localAI[1];
        public ref float LAI2 => ref Projectile.localAI[2];

        public override void AI()
        {
            if (MutantBoss.active && MutantBoss.type == ModContent.NPCType<MutantBoss>())
            {
                Projectile.velocity = MutantBoss.velocity;
                Projectile.rotation = MutantBoss.velocity.ToRotation() + MathHelper.ToRadians(135f);
                Projectile.Center = MutantBoss.Center + MutantBoss.velocity;

                // If it's the final dash of a predictive spear dash or it's masomode
                if (Variant <= -1)
                {
                    if (Variant == -2 && Timer % 2 == 0)
                    {
                        for (int i = -1; i <= 1; i += 2)
                        {
                            if (FargoSoulsUtil.HostCheck)
                            {
                                int p = Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center, 16f * Vector2.Normalize(MutantBoss.velocity).RotatedBy(MathHelper.PiOver2 * i),
                                ModContent.ProjectileType<MutantSphereSmall>(), Projectile.damage, 0f, Projectile.owner, -1);
                                if (p != Main.maxProjectiles)
                                    Main.projectile[p].timeLeft = 15;
                            }
                        }
                    }
                    // If it's not the special variant and masomode is on, always use this dash
                    else if (WorldSavingSystem.MasochistModeReal && Timer % 3 == 0)
                    {
                        for (int i = -1; i <= 1; i += 2)
                        {
                            if (FargoSoulsUtil.HostCheck)
                            {
                                int p = Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center, 16f / 2f * Vector2.Normalize(MutantBoss.velocity).RotatedBy(MathHelper.PiOver2 * i),
                                ModContent.ProjectileType<MutantSphereSmall>(), Projectile.damage, 0f, Projectile.owner, -1);
                                if (p != Main.maxProjectiles)
                                    Main.projectile[p].timeLeft = 15;
                            }
                        }
                    }
                }
                else
                {
                    if (Timer % 3 == 0)
                    {
                        if (FargoSoulsUtil.HostCheck)
                            Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<MutantSphereSmall>(), Projectile.damage, 0f, Projectile.owner, MutantBoss.target);
                    }
                }
            }
            else
                Projectile.Kill();

            Timer++;
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

            TryLifeSteal(target.Center, target.whoAmI);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.zenithWorld)
                TryLifeSteal(target.Center, Main.myPlayer);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = ModContent.Request<Texture2D>("FargowiltasSouls/Content/Bosses/MutantBoss/MutantProjectiles/MutantEye_Glow").Value;
            int frameHeight = glow.Height / Main.projFrames[Projectile.type];
            Rectangle rect = new(0, frameHeight * Projectile.frame, glow.Width, frameHeight);
            Color glowColorStart = (FargoSoulsUtil.AprilFools ? new Color(255, 191, 51, 0) : new Color(51, 255, 191, 0)) * 0.4f;
            Color glowColorEnd = (FargoSoulsUtil.AprilFools ? new Color(255, 242, 194, 0) : new Color(194, 255, 242, 0)) * 0.3f;
            Color glowColor = Color.Lerp(glowColorStart, glowColorEnd, 0.5f + MathF.Sin(Timer / 7f) / 2f);
            Vector2 drawCenter = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 28f;

            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 20;
                Vector2 drawCenter2 = drawCenter + vel.RotatedBy(MathHelper.Pi / 5 - i * MathHelper.Pi / 5);
                drawCenter2 -= vel;
                float scale = Projectile.scale + MathF.Sin(Timer / 7f) / 7f;
                Main.spriteBatch.Draw(glow, drawCenter2 - Main.screenPosition, rect, glowColor, Projectile.rotation - MathHelper.PiOver4,
                    rect.Size() / 2f, scale * 1.25f, SpriteEffects.None, 0);
            }

            int trailCacheLength = ProjectileID.Sets.TrailCacheLength[Projectile.type];
            for (int i = 0; i < trailCacheLength; i++)
            {
                float afterimageRatio = (trailCacheLength - i) / trailCacheLength;
                Color afterimageColor = glowColor * afterimageRatio;
                float scale = Projectile.scale * afterimageRatio;
                scale += MathF.Sin(Timer / 7f) / 7f;
                Vector2 afterimagePos = Projectile.oldPos[i] - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 14f;
                Main.spriteBatch.Draw(glow, afterimagePos + Projectile.Size / 2f - Main.screenPosition, rect, afterimageColor, Projectile.oldRot[i] - MathHelper.PiOver4,
                    rect.Size() / 2f, scale * 1.25f, SpriteEffects.None, 0);
            }

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
