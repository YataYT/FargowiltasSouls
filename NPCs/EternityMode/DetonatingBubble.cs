using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using FargowiltasSouls.Buffs.Masomode;
using Terraria.GameContent;

namespace FargowiltasSouls.NPCs.EternityMode
{
    public class DetonatingBubble : ModNPC
    {
        public override string Texture => "Terraria/Images/NPC_371";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Detonating Bubble");
            Main.npcFrameCount[NPC.type] = 2;
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "爆炸泡泡");
        }

        public override void SetDefaults()
        {
            NPC.width = 36;
            NPC.height = 36;
            NPC.damage = 100;
            NPC.lifeMax = 1;
            NPC.HitSound = SoundID.NPCHit3;
            NPC.DeathSound = SoundID.NPCDeath3;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0f;
            NPC.alpha = 255;
            NPC.lavaImmune = true;
            NPC.buffImmune[BuffID.OnFire] = true;
            NPC.buffImmune[BuffID.Suffocation] = true;
            NPC.aiStyle = -1;
            NPC.chaseable = false;
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            NPC.damage = (int)(NPC.damage * 0.75);
            NPC.lifeMax = 1;
        }

        public override void AI()
        {
            if (NPC.buffTime[0] != 0)
            {
                NPC.buffImmune[NPC.buffType[0]] = true;
                NPC.DelBuff(0);
            }

            if (NPC.alpha > 50)
                NPC.alpha -= 30;
            else
                NPC.alpha = 50;

            NPC.velocity *= 1.03f;

            NPC.ai[0]++;
            if (NPC.ai[0] >= 240f)
            {
                NPC.life = 0;
                NPC.checkDead();
                NPC.active = false;
            }
        }

        public override bool CheckDead()
        {
            NPC.GetGlobalNPC<FargoSoulsGlobalNPC>().Needled = false;
            return true;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Wet, 420);
            if (FargoSoulsWorld.MasochistModeReal)
                target.AddBuff(ModContent.BuffType<SqueakyToy>(), 120);
            target.AddBuff(ModContent.BuffType<OceanicMaul>(), 20 * 60);
            target.GetModPlayer<FargoSoulsPlayer>().MaxLifeReduction += FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.fishBossEX, NPCID.DukeFishron) ? 100 : 25;
        }

        public override void FindFrame(int frameHeight)
        {
            if (TextureAssets.Npc[NPC.type].IsLoaded)
                NPC.frame.Y = TextureAssets.Npc[NPC.type].Value.Height / 2;
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return false;
        }
    }
}