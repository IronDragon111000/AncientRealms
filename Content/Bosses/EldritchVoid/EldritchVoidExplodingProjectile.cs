using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Shaders;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.ModLoader.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AncientRealms.Content.Bosses.EldritchVoid;
using AncientRealms.Helpers;
using AncientRealms;

namespace AncientRealms.Content.Bosses.EldritchVoid
{
    public class EldritchVoidExplodingProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.light = 0.5f;
            Projectile.timeLeft = 240;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.95f;
        }

        public override void OnKill(int timeLeft)
        {
            for (int k = 0; k < Main.maxPlayers; k++) //laser collision
			{
				Player Player = Main.player[k];
                if(CheckCircularCollision(Projectile.Center, 126, Player.hitbox))
                {
                    						Player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByProjectile(k, Projectile.whoAmI), Projectile.damage, 0, false, false, -1, false);
                }
            }
        }

        public override bool PreDraw()
        {
            Texture2D texture = Content.Bosses.EldritchVoid.Value;

			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, default, lightColor * Projectile.Opacity, Projectile.rotation + rotationOffset, origin, Projectile.scale, effects, 0);
            return true;
        }
    }
}