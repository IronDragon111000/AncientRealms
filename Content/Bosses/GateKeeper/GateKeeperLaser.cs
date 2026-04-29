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

using Terraria.ModLoader.Core;
using static Terraria.ModLoader.Core.TmodFile;
namespace AncientRealms.Content.Bosses.GateKeeper
{
    public class GateKeeperLaser : ModProjectile
    {
        private float laserLength = 2000f;
        private Vector2 endPoint = Vector2.Zero;
		public bool Tell = true;
		public GateKeeper source;

		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 2;
		}
        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 16;
            Projectile.penetrate = -1;
			Projectile.tileCollide = false;
        }

        public override void AI()
        {
            // If something has gone wrong with either the beam or the host Prism, destroy the beam.
			if (Projectile.type != ModContent.ProjectileType<GateKeeperLaser>() || !source.NPC.active || source.NPC.type != ModContent.NPCType<GateKeeper>()) {
				Projectile.Kill();
				return;
			}
            Projectile.Center = source.NPC.Center;
            endPoint = Projectile.Center + (Vector2.Normalize(Projectile.velocity) * laserLength);

            if(!Tell) // Laser is active
            {
                for (int k = 0; k < Main.maxPlayers; k++) //laser collision
				{
					Player Player = Main.player[k];
					if (Player.active && !Player.dead && Helpers.CollisionHelper.CheckLinearCollision(Projectile.Center, endPoint, Projectile.width/2, Player.Hitbox, out Vector2 point))
					{
						Player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByProjectile(k, Projectile.whoAmI), Projectile.damage, 0, false, false, -1, false);
					}
				}
            }

        }
        public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			int frameHeight = texture.Height / Main.projFrames[Type];
			int spriteSheetOffset;

			Vector2 drawScale = new Vector2(Projectile.scale);

			DelegateMethods.f_1 = 1f; // f_1 is an unnamed decompiled variable whose function is unknown. Leave it at 1.
			Vector2 startPosition = Projectile.Center - Main.screenPosition;
			Vector2 endPosition = endPoint - Main.screenPosition;

			if(Tell) 
            {
				spriteSheetOffset = frameHeight * 0;
				DrawBeam(Main.spriteBatch, texture, startPosition, endPosition, drawScale, Color.Purple * 0.02f, spriteSheetOffset);
							
            } else // Laser is active
            {
				spriteSheetOffset = frameHeight * 1;
				// Draw the outer beam.
				DrawBeam(Main.spriteBatch, texture, startPosition, endPosition, drawScale, Color.SkyBlue, spriteSheetOffset);
				// Draw the inner beam, which is half size.
				DrawBeam(Main.spriteBatch, texture, startPosition, endPosition, drawScale * 0.7f, Color.White * 0.5f, spriteSheetOffset);
            }
			
			return false;
		}

		private void DrawBeam(SpriteBatch spriteBatch, Texture2D texture, Vector2 startPosition, Vector2 endPosition, Vector2 drawScale, Color beamColor, int frameOffset) {
			Utils.LaserLineFraming lineFraming = (int stage, Vector2 currentPosition, float distanceLeft, Rectangle lastFrame, out float distCovered, out Rectangle frame, out Vector2 origin, out Color color) =>
			{
				distCovered = drawScale.X;
				int y = frameOffset;
				frame = new Rectangle(0, y + frameOffset, texture.Width, texture.Height + frameOffset);
				origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
				color = beamColor;
			};

			// c_1 is an unnamed decompiled variable which is the render color of the beam drawn by DelegateMethods.RainbowLaserDraw.
			DelegateMethods.c_1 = beamColor;
			
			// Create a texture that represents just the current frame
			Rectangle frameRect = new Rectangle(0, frameOffset, texture.Width, texture.Height / Main.projFrames[Type]);
			Texture2D frameTexture = new Texture2D(Main.graphics.GraphicsDevice, frameRect.Width, frameRect.Height);
			Color[] data = new Color[frameRect.Width * frameRect.Height];
			texture.GetData(0, frameRect, data, 0, data.Length);
			frameTexture.SetData(data);
			
			Utils.DrawLaser(spriteBatch, frameTexture, startPosition, endPosition, drawScale, lineFraming);
		}
    }
}