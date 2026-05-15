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
using AncientRealms.Content.Bosses.SpiderBoss;
using AncientRealms.Helpers;
using AncientRealms;

namespace AncientRealms.Content.Bosses.SpiderBoss
{
    public class SpiderBossAcidBigProjectile : ModProjectile
    {
        public int phase = 0;
        private int spilts = 15;
            public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 7;
		}
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.penetrate = -1; // will be overiden by boss
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600; // will be overiden by boss
            Projectile.aiStyle = -1;
        }
        
        public override void AI()
        {
            if(phase >= 6)
            {
                Projectile.hostile = true;
                Projectile.velocity += new Vector2(0, 0.5f); // give projectile gravity

            }
            Projectile.frame = phase;
        }

        public override void OnKill(int timeLeft)
        {
            for(int i = 0; i < spilts; i++)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, 
                    Projectile.velocity.RotatedBy((MathHelper.TwoPi / 1.75f) - (i * MathHelper.TwoPi / (1.75 * spilts))), 
                    ModContent.ProjectileType<SpiderBossAcidSmallProjectile>(), 15, 1, 0, 0, 0);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if(phase >= 6)
            {
                Texture2D texture = Request<Texture2D>(Texture).Value;
                texture.Frame(1,7,0,6);
			    Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition - new Vector2(Projectile.width / 2, Projectile.height / 2), default, lightColor, Projectile.rotation, new Vector2(Projectile.width / 2, Projectile.height / 2), Projectile.scale, SpriteEffects.None, 0);
                return false;
            }
            return true;
        }
    }
}