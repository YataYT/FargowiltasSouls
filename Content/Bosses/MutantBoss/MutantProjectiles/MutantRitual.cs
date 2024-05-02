﻿using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Content.Buffs.Souls;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles
{
    public class MutantRitual : BaseArena
    {
        public override string Texture => FargoSoulsUtil.AprilFools ?
            "FargowiltasSouls/Content/Bosses/MutantBoss/TextureAlts/MutantSphere_April" :
            "Terraria/Images/Projectile_454";

        private const float realRotation = MathHelper.Pi / 140f;
        private bool MutantDead;

        public MutantRitual() : base(realRotation, 1200f, ModContent.NPCType<MutantBoss>(), visualCount: 48) { }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            // DisplayName.SetDefault("Mutant Seal");
            Main.projFrames[Projectile.type] = 2;
        }

        public override bool? CanDamage()
        {
            if (MutantDead)
                return false;
            return base.CanDamage();
        }

        protected override void Movement(NPC npc)
        {
            //int[] unmovingArenaStates =
            //{
            //    19, //pillars
            //    13, 14, 15, //predictive dash
            //    21, 22, 23 //direct dash
            //};

            float targetRotation;
            //if (unmovingArenaStates.Contains((int)npc.ai[0])) //be stationary
            if (npc.ai[0] == 19) //pillars
            {
                Projectile.velocity = Vector2.Zero;

                targetRotation = -realRotation / 2; //denote arena isn't moving
            }
            else if (npc.ai[0] == 49) //golem
            {
                if (npc.HasValidTarget && npc.ai[1] < 30) //snap it to player at start
                {
                    Projectile.velocity = (Main.player[npc.target].Center - Projectile.Center) / 10f;

                    targetRotation = realRotation;
                }
                else
                {
                    Projectile.velocity = Vector2.Zero;

                    targetRotation = -realRotation / 2; //denote arena isn't moving
                }
            }
            else
            {
                Projectile.velocity = npc.Center - Projectile.Center;
                if (npc.ai[0] == 36)
                    Projectile.velocity /= 20f; //much faster for slime rain
                else if (npc.ai[0] == 22 || npc.ai[0] == 23 || npc.ai[0] == 25)
                    Projectile.velocity /= 40f; //move faster for direct dash, predictive throw
                else
                    Projectile.velocity /= 60f;

                targetRotation = realRotation;
            }

            const float increment = realRotation / 40;
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

            MutantDead = npc.ai[0] <= -6;
        }

        public override void AI()
        {
            base.AI();
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame > 1)
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

                if (WorldSavingSystem.MasochistModeReal && Main.npc[EModeGlobalNPC.mutantBoss].ai[0] == -5)
                {
                    if (!target.HasBuff(ModContent.BuffType<TimeFrozenBuff>()))
                        SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/ZaWarudo"), target.Center);
                    target.AddBuff(ModContent.BuffType<TimeFrozenBuff>(), 300);
                }
            }
            target.AddBuff(ModContent.BuffType<CurseoftheMoonBuff>(), 600);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * Projectile.Opacity * (targetPlayer == Main.myPlayer && !MutantDead ? 1f : 0.15f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;

            Color color26 = Projectile.GetAlpha(lightColor);
            Texture2D glow = ModContent.Request<Texture2D>("FargowiltasSouls/Content/Bosses/MutantBoss/MutantProjectiles/MutantSphereGlow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            int rect1 = glow.Height;
            int rect2 = 0;
            Rectangle glowrectangle = new(0, rect2, glow.Width, rect1);
            Vector2 gloworigin2 = glowrectangle.Size() / 2f;
            Color glowcolor = Color.Lerp(FargoSoulsUtil.AprilFools ? Color.Red : new Color(196, 247, 255, 0), Color.Transparent, 0.8f);

            for (int x = 0; x < 32; x++)
            {
                Vector2 drawOffset = new Vector2(threshold * Projectile.scale / 2f, 0f).RotatedBy(Projectile.ai[0]);
                drawOffset = drawOffset.RotatedBy(2f * MathHelper.Pi / 32f * x);
                const int max = 4;
                for (int i = 0; i < max; i++)
                {
                    Color color27 = color26;
                    color27 *= (float)(max - i) / max;
                    Vector2 value4 = Projectile.oldPos[i] + Projectile.Hitbox.Size() / 2 + drawOffset.RotatedBy(rotationPerTick * -i);
                    float num165 = Projectile.rotation;
                    Main.EntitySpriteDraw(texture2D13, value4 - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color27, num165, origin2, Projectile.scale, SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(glow, value4 - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(glowrectangle), glowcolor * ((float)(max - i) / max),
                        Projectile.rotation, gloworigin2, Projectile.scale * 1.4f, SpriteEffects.None, 0);
                }
                Main.EntitySpriteDraw(texture2D13, Projectile.Center + drawOffset - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color26, Projectile.rotation, origin2, Projectile.scale, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(glow, Projectile.Center + drawOffset - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(glowrectangle), glowcolor,
                    Projectile.rotation, gloworigin2, Projectile.scale * 1.3f, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}