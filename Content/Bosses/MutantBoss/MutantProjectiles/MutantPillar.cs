using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.Systems;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public class MutantPillar : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.AprilFools ?
            "FargowiltasSouls/Content/Bosses/MutantBoss/TextureAlts/MutantPillar_April" :
            "FargowiltasSouls/Content/Projectiles/Masomode/CelestialPillar";


        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.aiStyle = -1;
            Projectile.alpha = 255;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            CooldownSlot = 1;
            Projectile.FargoSouls().TimeFreezeImmune = true;
            Projectile.FargoSouls().DeletionImmuneRank = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Target);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Target = reader.ReadInt32();
        }

        public override bool? CanDamage() => Projectile.alpha == 0;

        public override void OnSpawn(IEntitySource source)
        {
            // Determine the dust type of pillar to emit
            int type = AI0 switch
            {
                0f => 242,
                1f => 127,
                2f => 229,
                _ => 135,
            };

            // Create specific pillar-based dust effects
            for (int index = 0; index < 50; ++index)
            {
                Dust dust = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, type)];
                dust.velocity *= 10f;
                dust.fadeIn = 1f;
                dust.scale = 1 + Main.rand.NextFloat() + Main.rand.Next(4) * 0.3f;
                if (!Main.rand.NextBool(3))
                {
                    dust.noGravity = true;
                    dust.velocity *= 3f;
                    dust.scale *= 2f;
                }
            }
        }

        private int Target = -1;
        public ref float AI0 => ref Projectile.ai[0];
        public ref float MutantIndex => ref Projectile.ai[1];
        public ref float AI2 => ref Projectile.ai[2];
        public ref float LAI0 => ref Projectile.localAI[0];
        public ref float LAI1 => ref Projectile.localAI[1];
        public ref float LAI2 => ref Projectile.localAI[2];

        public override void AI()
        {
            // Fade in slowly
            if (Projectile.alpha > 0)
            {
                Projectile.velocity.Y += 5f / 120f;
                Projectile.rotation += Projectile.velocity.Length() / 20f * 2f;
                LAI1 += Projectile.velocity.Y;
                Projectile.alpha -= 2;

                //
                if (Projectile.alpha <= 0)
                {
                    Projectile.alpha = 0;

                    // Start the launch sequence
                    if (Target != -1)
                    {
                        SoundEngine.PlaySound(SoundID.Item89, Projectile.Center);

                        float speed = 32f;
                        Projectile.velocity = Main.player[Target].Center - Projectile.Center;
                        float distance = Projectile.velocity.Length();
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.One) * speed;
                        Projectile.timeLeft = (int)(distance / speed);
                        return;
                    }
                    // If there's no target, disappear
                    else
                        Projectile.Kill();
                }
                // Keep setting the target up until it's about to launch itself
                else
                {
                    NPC npc = Main.npc[(int)MutantIndex];
                    Target = npc.target;
                    Projectile.Center = npc.Center;
                    Projectile.position.Y += LAI1;
                }

                // Sometime during the spin, start turning towards the player
                if (Target >= 0 && Main.player[Target].active && !Main.player[Target].dead)
                {
                    if (Projectile.alpha < 100)
                        Projectile.rotation = Projectile.rotation.AngleLerp((Main.player[Target].Center - Projectile.Center).ToRotation(), (255 - Projectile.alpha) / 255f * 0.08f);
                }
                // Otherwise try to find a player and set the target accordingly
                else
                {
                    int possibleTarget = Player.FindClosest(Projectile.Center, 0, 0);
                    if (possibleTarget != -1)
                        Target = possibleTarget;
                }
            }
            // Otherwise aim towards the launch direction
            else
                Projectile.rotation = Projectile.velocity.ToRotation();

            // Set the frame to the correct pillar
            Projectile.frame = (int)AI0;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // Hit em so hard they fall off their ride
            if (target.mount.Active)
                target.mount.Dismount(target);

            // And knock them back forcefully
            target.velocity.X = 15f * MathF.Sign(-Projectile.velocity.X);
            target.velocity.Y = -10f;

            // Debuffs
            target.AddBuff(ModContent.BuffType<StunnedBuff>(), 60);
            target.AddBuff(ModContent.BuffType<CurseoftheMoonBuff>(), 600);
            if (WorldSavingSystem.EternityMode)
            {
                target.AddBuff(ModContent.BuffType<MarkedforDeathBuff>(), 240);
                target.AddBuff(ModContent.BuffType<MutantFangBuff>(), 180);
            }

            // Eternity mode pillar debuffs
            switch (AI0)
            {
                case 0: target.AddBuff(ModContent.BuffType<ReverseManaFlowBuff>(), 360); break; // Nebula
                case 1: target.AddBuff(ModContent.BuffType<AtrophiedBuff>(), 360); break; // Solar
                case 2: target.AddBuff(ModContent.BuffType<JammedBuff>(), 360); break; // Vortex
                default: target.AddBuff(ModContent.BuffType<AntisocialBuff>(), 360); break; // Stardust
            }

            // Die
            Projectile.timeLeft = 0;
        }

        public override void OnKill(int timeLeft)
        {
            // Shaek screne
            if (Main.LocalPlayer.active && !Main.dedServ)
                ScreenShakeSystem.StartShake(10, shakeStrengthDissipationIncrement: 10f / 30);

            SoundEngine.PlaySound(SoundID.Item92, Projectile.Center);

            // Determine dust type
            var type = AI0 switch
            {
                0 => 242,
                1 => 127,
                2 => 229,
                _ => 135,
            };

            // Emit pillar dust
            for (int index = 0; index < 80; ++index)
            {
                Dust dust = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, type)];
                dust.velocity *= 10f;
                dust.fadeIn = 1f;
                dust.scale = 1 + Main.rand.NextFloat() + Main.rand.Next(4) * 0.3f;
                if (!Main.rand.NextBool(3))
                {
                    dust.noGravity = true;
                    dust.velocity *= 3f;
                    dust.scale *= 2f;
                }
            }

            // Explode into fragments
            if (FargoSoulsUtil.HostCheck)
            {
                int fragmentDuration = 240;
                const int max = 24;
                const float rotationInterval = MathHelper.TwoPi / max;
                float speed = WorldSavingSystem.MasochistModeReal ? 5.5f : 5f;

                // Find out how long the fragments should last for
                if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.mutantBoss, ModContent.NPCType<MutantBoss>())
                && Main.npc[EModeGlobalNPC.mutantBoss].ai[1] == (int)MutantBoss.BehaviorStates.PillarDunk)
                    fragmentDuration = (int)Main.npc[EModeGlobalNPC.mutantBoss].localAI[0];

                for (int j = 0; j < 4; j++)
                {
                    Vector2 vel = new Vector2(0f, speed * (j + 0.5f)).RotatedBy(Projectile.rotation);
                    for (int i = 0; i < max; i++)
                    {
                        int p = Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center, vel.RotatedBy(rotationInterval * i),
                            ModContent.ProjectileType<MutantFragment>(), Projectile.damage / 2, 0f, Main.myPlayer, AI0);
                        if (p != Main.maxProjectiles)
                            Main.projectile[p].timeLeft = fragmentDuration;
                    }
                }
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 255 - Projectile.alpha);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            int frameHeight = tex.Height / Main.projFrames[Projectile.type];
            Rectangle rect = new(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

            float trailCacheLength = ProjectileID.Sets.TrailCacheLength[Projectile.type];
            for (int i = 0; i < (int)trailCacheLength; i++)
            {
                float afterimageRatio = (trailCacheLength - i) / trailCacheLength;
                Color afterimageColor = Projectile.GetAlpha(lightColor) * afterimageRatio;

                Main.spriteBatch.Draw(tex, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, rect, afterimageColor, Projectile.oldRot[i], rect.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, rect, Projectile.GetAlpha(lightColor), Projectile.rotation, rect.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
    }
}