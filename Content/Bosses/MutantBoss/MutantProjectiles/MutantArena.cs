using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Content.Buffs.Souls;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices.Marshalling;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Chat;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles
{
    public class MutantArena : BaseArena
    {
        public static float ArenaSize => 1200f;

        public override string Texture => FargoSoulsUtil.AprilFools ?
            "FargowiltasSouls/Content/Bosses/MutantBoss/TextureAlts/MutantSphere_April" :
            "Terraria/Images/Projectile_454";

        private const float RealRotation = MathHelper.Pi / 140f;
        private bool MutantDead;

        public MutantArena() : base(RealRotation, ArenaSize, ModContent.NPCType<MutantBoss>(), visualCount: 48) { }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            Main.projFrames[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
        }

        public override bool? CanDamage()
        {
            if (MutantDead)
                return false;

            return base.CanDamage();
        }

        protected override void Movement(NPC npc)
        {
            float targetRotation;
            int mutantCurrentAttack = (int)npc.ai[1];
            
            // Be stationary and denote that fact to the player
            if (mutantCurrentAttack == (int)MutantBoss.BehaviorStates.PillarDunk)
            {
                Projectile.velocity = Vector2.Zero;
                targetRotation = -RealRotation / 2f;
            }
            /*
            else if (mutantCurrentAttack == MutantBoss.BehaviorStates.SANSGOLEM)
            {
                // Snap the arena to the player at the start of the attack
                if (npc.HasValidTarget && npc.ai[2] < 30)
                {
                    Projectile.velocity = (Main.player[npc.target].Center - Projectile.Center) / 10f;
                    targetRotation = RealRotation;
                }
                // Otherwise be stationary and denote that fact to the player
                else
                {
                    Projectile.velocity = Vector2.Zero;
                    targetRotation = -RealRotation / 2;
                }
            }
            */
            else
            {
                Projectile.velocity = npc.Center - Projectile.Center;

                // Slow down the velocity rate, except sped up for certain attacks
                if (mutantCurrentAttack == (int)MutantBoss.BehaviorStates.SlimeRain)
                    Projectile.velocity /= 20f;
                else if (mutantCurrentAttack == (int)MutantBoss.BehaviorStates.SpearDashDirect || mutantCurrentAttack == (int)MutantBoss.BehaviorStates.SpearTossPredictiveWithDestroyers)
                    Projectile.velocity /= 40f;
                else
                    Projectile.velocity /= 60f;

                targetRotation = RealRotation;
            }

            const float increment = RealRotation / 40;

            // Adjust rotation accordingly
            if (rotationPerTick < targetRotation)
            {
                rotationPerTick += increment;
                if (rotationPerTick > targetRotation)
                    rotationPerTick = targetRotation;
            }
            else if (rotationPerTick > targetRotation)
            {
                rotationPerTick -= increment;
                if (rotationPerTick < targetRotation)
                    rotationPerTick = targetRotation;
            }

            // Update whether Mutant is dying or not
            MutantDead = mutantCurrentAttack == (int)MutantBoss.BehaviorStates.Dying;
        }

        public ref float CurrentRotation => ref Projectile.ai[0];
        public ref float MutantIndex => ref Projectile.ai[1];

        public override void AI()
        {
            base.AI();

            // Cycle frames
            if (++Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame > 1)
                    Projectile.frame = 0;
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            base.OnHitPlayer(target, info);

            if (WorldSavingSystem.EternityMode)
            {
                target.FargoSouls().MaxLifeReduction += 100;
                target.AddBuff(ModContent.BuffType<OceanicMaulBuff>(), 5400);
                target.AddBuff(ModContent.BuffType<MutantFangBuff>(), 180);

                if (WorldSavingSystem.MasochistModeReal && Main.npc[EModeGlobalNPC.mutantBoss].ai[1] == (int)MutantBoss.BehaviorStates.FinalSpark)
                {
                    if (!target.HasBuff(ModContent.BuffType<TimeFrozenBuff>()))
                        SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/ZaWarudo"), target.Center);
                    target.AddBuff(ModContent.BuffType<TimeFrozenBuff>(), 300);
                }
            }

            target.AddBuff(ModContent.BuffType<CurseoftheMoonBuff>(), 600);
        }

        // Dimmed when Mutant is dead
        public override Color? GetAlpha(Color lightColor) => Color.White * Projectile.Opacity * (targetPlayer == Main.myPlayer && !MutantDead ? 1f : 0.15f);

        public override bool PreDraw(ref Color lightColor)
        {
            // Base texture
            Texture2D baseTex = ModContent.Request<Texture2D>(Texture).Value;
            int baseFrameHeight = baseTex.Height / Main.projFrames[Projectile.type];
            Rectangle baseRect = new(0, baseFrameHeight * Projectile.frame, baseTex.Width, baseFrameHeight);
            Color baseColor = Projectile.GetAlpha(lightColor);

            // Glow texture
            Texture2D glow = ModContent.Request<Texture2D>("FargowiltasSouls/Content/Bosses/MutantBoss/MutantProjectiles/MutantSphereGlow").Value;
            Rectangle glowRect = new(0, 0, glow.Width, glow.Height);
            Color glowColor = (FargoSoulsUtil.AprilFools ? Color.Red : new Color(196, 247, 255, 0)) * 0.4f;

            // Draw the entire arena
            int numEyes = 32;
            for (int i = 0; i < numEyes; i++)
            {
                Vector2 drawOffset = new Vector2(threshold * Projectile.scale / 2f, 0f).RotatedBy(CurrentRotation + MathHelper.TwoPi / numEyes * i);

                // Draw afterimages
                float trailCacheLength = ProjectileID.Sets.TrailCacheLength[Projectile.type];
                for (int j = 0; j < (int)trailCacheLength; j++)
                {
                    float afterimageRatio = (trailCacheLength - j) / trailCacheLength;
                    Color baseAfterimageColor = baseColor * afterimageRatio;
                    Color glowAfterimageColor = glowColor * afterimageRatio;
                    Vector2 drawPos = Projectile.oldPos[j] + Projectile.Hitbox.Size() / 2f + drawOffset.RotatedBy(rotationPerTick * -j) - Main.screenPosition;

                    Main.spriteBatch.Draw(glow, drawPos, glowRect, glowAfterimageColor, Projectile.rotation, glowRect.Size() / 2f, Projectile.scale * 1.4f, SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(baseTex, drawPos, baseRect, baseAfterimageColor, Projectile.rotation, baseRect.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);
                }

                // Draw the base projectile
                Main.spriteBatch.Draw(glow, Projectile.Center + drawOffset - Main.screenPosition, glowRect, glowColor,
                    Projectile.rotation, glowRect.Size() / 2f, Projectile.scale * 1.4f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(baseTex, Projectile.Center + drawOffset - Main.screenPosition, baseRect, baseColor,
                    Projectile.rotation, baseRect.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);
            }

            return false;
        }
    }
}