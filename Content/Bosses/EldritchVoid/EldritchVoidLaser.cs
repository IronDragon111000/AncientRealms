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

using Terraria.ModLoader.Core;
using static Terraria.ModLoader.Core.TmodFile;
namespace AncientRealms.Content.Bosses.EldritchVoid
{
    public class EldritchVoidLaser : ModProjectile
    {
		public override string Texture => "AncientRealms/Content/Bosses/EldritchVoid/EldritchVoidLaser"; // Use texture of item as projectile texture

        private float laserLength = 2000f;
        private Vector2 endPoint = Vector2.Zero;
        public ref float timer => ref Projectile.ai[0];
        // This property encloses the internal AI variable Projectile.ai[1].
		private float sourceIndex {
			get => Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}
		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 2;
		}
        public override void SetDefaults()
        {
            Projectile.width = 72;
            Projectile.height = 14;
            Projectile.penetrate = -1;
			Projectile.alpha = 0; // Laser is drawn manually in PreDraw, so don't make it transparent
			// The beam itself still stops on tiles, but its invisible "source" Projectile ignores them.
			// This prevents the beams from vanishing if the player shoves the Prism into a wall.
			Projectile.tileCollide = false;
        }

        public override void AI()
        {
            // If something has gone wrong with either the beam or the host Prism, destroy the beam.
			EldritchVoidLaserSource source = Main.projectile[(int)sourceIndex].ModProjectile as EldritchVoidLaserSource;
			if (Projectile.type != ModContent.ProjectileType<EldritchVoidLaser>() || !source.Projectile.active || source.Projectile.type != ModContent.ProjectileType<EldritchVoidLaserSource>()) {
				Projectile.Kill();
				return;
			}
            timer++;

            Projectile.damage = source.Projectile.damage;
            Projectile.Center = source.Projectile.Center;
            endPoint = Projectile.Center + (Vector2.Normalize(source.Projectile.velocity) * laserLength);

            if(timer > source.telegraphLength) // Laser is active
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
			EldritchVoidLaserSource source = Main.projectile[(int)sourceIndex].ModProjectile as EldritchVoidLaserSource;
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			int frameHeight = texture.Height / Main.projFrames[Type];
			int spriteSheetOffset;

			Vector2 drawScale = new Vector2(Projectile.scale);

			DelegateMethods.f_1 = 1f; // f_1 is an unnamed decompiled variable whose function is unknown. Leave it at 1.
			Vector2 startPosition = Projectile.Center - Main.screenPosition;
			Vector2 endPosition = endPoint - Main.screenPosition;

			if(timer < source.telegraphLength) // tell
            {
				spriteSheetOffset = frameHeight * 0;
				DrawBeam(Main.spriteBatch, texture, startPosition, endPosition, drawScale, Color.Purple * 0.05f, spriteSheetOffset);
							
            } else // Laser is active
            {
				spriteSheetOffset = frameHeight * 1;
				// Draw the inner beam, which is half size.
				DrawBeam(Main.spriteBatch, texture, startPosition, endPosition, drawScale * 0.7f, Color.White * Projectile.Opacity, spriteSheetOffset);

				// Draw the outer beam.
				DrawBeam(Main.spriteBatch, texture, startPosition, endPosition, drawScale, Color.Purple * Projectile.Opacity, spriteSheetOffset);
            }
			
			return false;
		}

		private void DrawBeam(SpriteBatch spriteBatch, Texture2D texture, Vector2 startPosition, Vector2 endPosition, Vector2 drawScale, Color beamColor, int frameOffset) {
			Utils.LaserLineFraming lineFraming = (int stage, Vector2 currentPosition, float distanceLeft, Rectangle lastFrame, out float distCovered, out Rectangle frame, out Vector2 origin, out Color color) =>
			{
				distCovered = drawScale.X;
				int y = frameOffset;
				frame = new Rectangle(0, y + frameOffset, texture.Width, texture.Height + frameOffset);
				origin = new Vector2(texture.Width / 2f, frameOffset);
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