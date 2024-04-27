using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss.MutantProjectiles
{
    public abstract class BaseMutantSpearAttack : ModProjectile
    {
        protected NPC MutantBoss;

        protected void TryLifeSteal(Vector2 pos, int player)
        {
            if (WorldSavingSystem.masochistModeReal && MutantBoss is not null)
            {
                int totalHealPerHit = MutantBoss.lifeMax / 100 * 5;

                int max = 20;
                for (int i = 0; i < max; i++)
                {
                    Vector2 vel = Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(-2f, -9f);
                    int healPerOrb = (int)(totalHealPerHit / max * Main.rand.NextFloat(0.95f, 1.05f));

                    if (player == Main.myPlayer && Main.player[player].ownedProjectileCounts[ModContent.ProjectileType<MutantHeal>()] < 10)
                    {
                        Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), pos, vel, ModContent.ProjectileType<MutantHeal>(),
                            healPerOrb, 0f, Main.myPlayer, MutantBoss.whoAmI, vel.Length() / Main.rand.Next(30, 90));

                        SoundEngine.PlaySound(SoundID.Item27, pos);
                    }
                }
            }
        }
    }
}
