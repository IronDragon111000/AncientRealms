using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Microsoft.Xna.Framework.Graphics.Texture2D;
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
        public ref float damage => ref Projectile.ai[0];
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
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
            Projectile.velocity *= 0.99f;
        }

        public override void OnKill(int timeLeft)
        {
            for (int k = 0; k < Main.maxPlayers; k++) //laser collision
			{
				Player Player = Main.player[k];
                if(CollisionHelper.CheckCircularCollision(Projectile.Center, 200, Player.Hitbox))
                {
                    Player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByProjectile(k, Projectile.whoAmI), (int)damage, 0, false, false, -1, false);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D telegraphTexture = Request<Texture2D>(Texture + "Tell").Value;
			Main.spriteBatch.Draw(telegraphTexture, Projectile.Center - Main.screenPosition - new Vector2(telegraphTexture.Width / 2, telegraphTexture.Height / 2), default, Color.DarkMagenta * 0.25f);
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition - new Vector2(texture.Width / 2, texture.Height / 2), default, Color.White);
            return false;
        }
    }
}