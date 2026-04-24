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

namespace AncientRealms.Content.Bosses.GateKeeper
{
    public class GateKeeperLaser : ModProjectile
    {
		public GateKeeper parent;
        // These variables control the beam's potential coloration.
		// As a value, hue ranges from 0f to 1f, both of which are pure red. 
		// Saturation ranges from 0f to 1f and controls how greyed out the color is. 0 is fully grayscale, 1 is vibrant, intense color.
		// Lightness ranges from 0f to 1f and controls how dark or light the color is. 0 is pitch black. 1 is pure white.
		private const float BeamColorHue = 0.57f;
		private const float BeamColorSaturation = 0.66f;
		private const float BeamColorLightness = 0.53f;

		// The beam draws two lasers separately: an inner beam and an outer beam. This controls their opacity.
		private const float OuterBeamOpacityMultiplier = 0.9f;
		private const float InnerBeamOpacityMultiplier = 1f;

		public int direction = -1;
		public Vector2 endpoint = Vector2.Zero;

		public float aimOffset = 0;

		public ref float Timer => ref Projectile.ai[0];
		public ref float LaserRotation => ref Projectile.ai[1];

		private float LaserTimer => (Timer - 120) % 400;

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 20;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
        }

		public void FindParent()
		{
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC NPC = Main.npc[i];
				if (NPC.active && NPC.type == NPCType<GateKeeper>())
				{
					parent = NPC.ModNPC as GateKeeper;
					return;
				}
			}

			return;
		}

        public override void AI()
		{
			if (parent is null)
				FindParent();

			if (parent is null)
				return;

			Timer++;
			Projectile.timeLeft = 2;

			Projectile.Center = parent.NPC.Center + new Vector2(4, -4);

			if (Timer < 120 && Main.masterMode)
				Projectile.extraUpdates = 2;
			else
				Projectile.extraUpdates = 0;

			if (Timer < 60)
			{
				for (int k = 0; k < 3; k++)
				{
					Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(300, 300);
					Vector2 vel = pos.DirectionTo(Projectile.Center).RotatedBy(MathHelper.Pi / 2.2f * Main.rand.NextFloatDirection()) * Main.rand.NextFloat(5f);
					
				}

				Projectile.scale = Math.Min(1, Timer / 60f);
			}

			if (Timer > 120)
			{
				if (LaserTimer == 140)
					direction = (Main.player[parent.NPC.target].Center - Projectile.Center).ToRotation() > LaserRotation ? 1 : -1;

				if (LaserTimer == 141)
				{
					
					Projectile.netUpdate = true;
				}

				if (LaserTimer == 30)
					Projectile.netUpdate = true;

				if (LaserTimer > 30 && LaserTimer <= 75)
				{
					LaserRotation = (Main.player[parent.NPC.target].Center - Projectile.Center).ToRotation() + aimOffset;

					Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(300, 300);
					Vector2 vel = pos.DirectionTo(Projectile.Center).RotatedBy(MathHelper.Pi / 2.2f * Main.rand.NextFloatDirection()) * Main.rand.NextFloat(5f);
					
				}

				if (LaserTimer > 150) //laser is actually active
				{
					float laserSpeed = Main.masterMode ? 0.019f : Main.expertMode ? 0.017f : 0.014f;

					for (int k = 0; k < 160; k++) //raycast to find the laser's endpoint
					{
						Vector2 posCheck = Projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation) * k * 8;

						if (!parent.arena.Contains(posCheck.ToPoint()))
						{
							endpoint = posCheck;
							break;
						}
					}

					LaserRotation += laserSpeed * direction;

					for (int k = 0; k < Main.maxPlayers; k++) //laser colission
					{
						Player Player = Main.player[k];

						if (Player.active && !Player.dead && Helpers.CollisionHelper.CheckLinearCollision(Projectile.Center, endpoint, Player.Hitbox, out Vector2 point))
						{
							Player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByNPC(ModContent.NPCType<GateKeeper>()),  Projectile.damage, 0, false, false, -1, false);
							endpoint = point;
							break;
						}
					}
				}
			}

			if (Timer > 500 || parent.Phase == (int)GateKeeper.AIStates.Dying || parent.Phase == (int)GateKeeper.AIStates.Leaving)
			{
				Projectile.scale -= 0.05f;

				if (Projectile.scale <= 0)
					Projectile.active = false;
			}
		}


       

		public override bool PreDraw(ref Color lightColor) {
			if(LaserTimer >= 140)
			{
				Texture2D texture = TextureAssets.Projectile[Type].Value;
				Vector2 centerFloored = Projectile.Center.Floor() + Projectile.velocity * Projectile.scale * 10.5f;
				Vector2 drawScale = new Vector2(Projectile.scale);

				// Reduce the beam length proportional to its square area to reduce block penetration.
				float visualBeamLength = 1000f - 14.5f * Projectile.scale * Projectile.scale;

				DelegateMethods.f_1 = 1f; // f_1 is an unnamed decompiled variable whose function is unknown. Leave it at 1.
				Vector2 startPosition = centerFloored - Main.screenPosition;
				Vector2 endPosition = startPosition + new Vector2(visualBeamLength, 0).RotatedBy(LaserRotation);

				// Draw the outer beam.
				DrawBeam(Main.spriteBatch, texture, startPosition, endPosition, drawScale, GetOuterBeamColor() * OuterBeamOpacityMultiplier * Projectile.Opacity);

				// Draw the inner beam, which is half size.
				drawScale *= 0.7f;
				DrawBeam(Main.spriteBatch, texture, startPosition, endPosition, drawScale, GetInnerBeamColor() * InnerBeamOpacityMultiplier * Projectile.Opacity);
			}
			// Returning false prevents Terraria from trying to draw the Projectile itself.
			return false;
		}

		private void DrawBeam(SpriteBatch spriteBatch, Texture2D texture, Vector2 startPosition, Vector2 endPosition, Vector2 drawScale, Color beamColor) {
			Utils.LaserLineFraming lineFraming = new Utils.LaserLineFraming(DelegateMethods.RainbowLaserDraw);

			// c_1 is an unnamed decompiled variable which is the render color of the beam drawn by DelegateMethods.RainbowLaserDraw.
			DelegateMethods.c_1 = beamColor;
			Utils.DrawLaser(spriteBatch, texture, startPosition, endPosition, drawScale, lineFraming);
		}

		private Color GetOuterBeamColor() {
			// This hue calculation produces a unique color for each beam based on its Beam ID.
			float hue = BeamColorHue;

			// Main.hslToRgb converts Hue, Saturation, Lightness into a Color for general purpose use.
			Color c = Main.hslToRgb(hue, BeamColorSaturation, BeamColorLightness);

			// Manually reduce the opacity of the color so beams can overlap without completely overwriting each other.
			c.A = 64;
			return c;
		}

		// Inner beams are always pure white so that they act as a "blindingly bright" center to each laser.
		private Color GetInnerBeamColor() => Color.White;
    }
}