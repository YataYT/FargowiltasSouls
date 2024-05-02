using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public class MutantDestroyerHead : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.AprilFools ?
            "FargowiltasSouls/Content/Bosses/MutantBoss/TextureAlts/MutantDestroyerHead_April" :
            "FargowiltasSouls/Assets/ExtraTextures/Resprites/NPC_134";

        public override void SetDefaults()
        {
            Projectile.width = 42;
            Projectile.height = 42;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 900;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.netImportant = true;
            CooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
            writer.Write(Projectile.localAI[1]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
            Projectile.localAI[1] = reader.ReadSingle();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Appearance == 0 ? ModContent.Request<Texture2D>(Texture).Value : ModContent.Request<Texture2D>("FargowiltasSouls/Assets/ExtraTextures/Resprites/NPC_13").Value;
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() / 2f, Projectile.scale, spriteEffects, 0);

            return false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Makes setting AI1 optional, since it must be a value above 0
            if (SpeedMultiplier == 0)
                SpeedMultiplier = 1f;
        }

        public ref float PlayerTarget => ref Projectile.ai[0];
        public ref float SpeedMultiplier => ref Projectile.ai[1];
        public ref float Appearance => ref Projectile.ai[2];
        public ref float LAI0 => ref Projectile.localAI[0];
        public ref float Timer => ref Projectile.localAI[1];
        public ref float LAI2 => ref Projectile.localAI[2];

        public override void AI()
        {
            //keep the head looking right
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.spriteDirection = Projectile.velocity.X > 0f ? 1 : -1;

            int homingDelay = 60;
            float desiredFlySpeedInPixelsPerFrame = 10 * SpeedMultiplier;
            float amountOfFramesToLerpBy = 25f / SpeedMultiplier; // minimum of 1, please keep in full numbers even though it's a float!

            // Start homing onto the player
            if (Timer > 60)
            {
                Player target = Main.player[(int)PlayerTarget];

                // Accelerate faster if the player is already far away
                if (Projectile.Distance(target.Center) > 700)
                {
                    desiredFlySpeedInPixelsPerFrame *= 2;
                    amountOfFramesToLerpBy /= 2;
                }

                Vector2 desiredVelocity = Projectile.SafeDirectionTo(target.Center) * desiredFlySpeedInPixelsPerFrame;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 1f / amountOfFramesToLerpBy);
            }

            float idleAccel = 0.05f;
            foreach (Projectile proj in Main.projectile.Where(proj => proj.active && proj.type == Projectile.type && proj.whoAmI != Projectile.whoAmI
            && proj.Distance(Projectile.Center) < Projectile.width))
            {
                Projectile.velocity += new Vector2(Projectile.position.X < proj.position.X ? -1 : 1, Projectile.position.Y < proj.position.Y ? -1 : 1) * idleAccel;
                proj.velocity += new Vector2(proj.position.X < Projectile.position.X ? -1 : 1, proj.position.Y < Projectile.position.Y ? -1 : 1) * idleAccel;
            }

            Timer++;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(Appearance == 0 ? ModContent.BuffType<LightningRodBuff>() : BuffID.Weak, Main.rand.Next(300, 1200));
            if (WorldSavingSystem.EternityMode)
                target.AddBuff(ModContent.BuffType<MutantFangBuff>(), 180);
        }

        public override void OnKill(int timeLeft)
        {
            // Dust!!
            for (int i = 0; i < 40; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Main.rand.NextBool() ? DustID.PurpleTorch : DustID.RedTorch,
                    -Projectile.velocity.X * 0.2f, -Projectile.velocity.Y * 0.2f, 100, Scale: Main.rand.NextFloat(1f, 2f));
                Main.dust[d].noGravity = Main.rand.NextBool();
                Main.dust[d].velocity *= 2f;
            }

            SoundEngine.PlaySound(SoundID.NPCDeath14, Projectile.Center);
        }
    }
}