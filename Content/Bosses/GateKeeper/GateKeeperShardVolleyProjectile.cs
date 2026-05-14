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
using AncientRealms.Content.Bosses.GateKeeper;
using AncientRealms.Helpers;
using AncientRealms;
using AssGen;

using Terraria.ModLoader.Core;
using static Terraria.ModLoader.Core.TmodFile;
namespace AncientRealms.Content.Bosses.GateKeeper
{
    public class GateKeeperShardVolleyProjectile : ModProjectile
    {
        private float laserLength = 1000f;
        private Vector2 endPoint = Vector2.Zero;
		public bool Tell = true;
        internal ref float timer => ref Projectile.ai[1];
		public GateKeeper source;
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            timer--;
            if(timer <= 0)
                Tell = false;
            if(Tell)
            {
                Projectile.hostile = false;
                Projectile.Center = source.NPC.Center + Projectile.velocity;
                endPoint = Projectile.Center + Vector2.Normalize(Projectile.velocity) * laserLength;
            }else
            {
                Projectile.hostile = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if(Tell)
            {
                //var texture = Assets.Bosses.GateKeeper.GateKeeperShardVolleyProjectileTell.Value;
                var texture = TextureAssets.Projectile[Type].Value;

                Vector2 drawScale = new Vector2(Projectile.scale);

                DelegateMethods.f_1 = 1f; // f_1 is an unnamed decompiled variable whose function is unknown. Leave it at 1.
                Vector2 startPosition = Projectile.Center - Main.screenPosition;
                Vector2 endPosition = endPoint - Main.screenPosition;
                DrawBeam(Main.spriteBatch, texture, startPosition, endPosition, drawScale, Color.SkyBlue * 0.02f);
                return false;
            }
            return true;
        }
        private void DrawBeam(SpriteBatch spriteBatch, Texture2D texture, Vector2 startPosition, Vector2 endPosition, Vector2 drawScale, Color beamColor) {
			Utils.LaserLineFraming lineFraming = (int stage, Vector2 currentPosition, float distanceLeft, Rectangle lastFrame, out float distCovered, out Rectangle frame, out Vector2 origin, out Color color) =>
			{
				distCovered = drawScale.X;
				frame = new Rectangle(0, 0, texture.Width, texture.Height);
				origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
				color = beamColor;
			};

			// c_1 is an unnamed decompiled variable which is the render color of the beam drawn by DelegateMethods.RainbowLaserDraw.
			DelegateMethods.c_1 = beamColor;
			
			Rectangle frameRect = new Rectangle(0, 0, texture.Width, texture.Height / Main.projFrames[Type]);
			Texture2D frameTexture = new Texture2D(Main.graphics.GraphicsDevice, frameRect.Width, frameRect.Height);
			Color[] data = new Color[frameRect.Width * frameRect.Height];
			texture.GetData(0, frameRect, data, 0, data.Length);
			frameTexture.SetData(data);
			
			Utils.DrawLaser(spriteBatch, frameTexture, startPosition, endPosition, drawScale, lineFraming);
		}
    }
}