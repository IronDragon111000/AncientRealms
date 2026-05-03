using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Linq;
using System.Reflection;
using AncientRealms.Content.Tiles.Misc;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ModLoader;
using AncientRealms.Content.SubWorlds.SubSpaceHub;
using Terraria;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Capture;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using ReLogic.Content;

namespace AncientRealms.Content.Biomes
{
    public class VoidBiome : ModBiome
    {
		public static bool onScreen;
        //public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<VoidBiomeBackgroundStyle>();
		
		private static Vector2 parallaxOrigin;
		private static float vanillaParallax;
		/*internal static Asset<Texture2D>[] textures =
  		{
            Assets.Backgrounds.VoidBackGround0,
    		Assets.Backgrounds.VoidBackGround1,
            Assets.Backgrounds.VoidBackGround2,
            Assets.Backgrounds.VoidBackGround3,
            Assets.Backgrounds.VoidBackGround4
  		};*/
        public override bool IsBiomeActive(Player player)
        {
            return SubworldLibrary.SubworldSystem.IsActive<SubSpaceHub>();
        }

        public override void Load()
        {
            On_Main.DrawBackgroundBlackFill += DrawVoidBackground;
			On_Main.DrawBlack += ForceDrawBlack;
			IL_Main.DrawBlack += ChangeBlackThreshold;
        }


		/// <summary>
		/// This method forces DrawBlack to be called while in the biome to ensure correct rendering
		/// of the area with the passive light, similar to hell
		/// </summary>
		/// <param name="orig"></param>
		/// <param name="self"></param>
		/// <param name="force"></param>
		private void ForceDrawBlack(On_Main.orig_DrawBlack orig, Main self, bool force)
		{
			if (onScreen)
				orig(self, true);
			else
				orig(self, force);
		}

		/// <summary>
		/// This IL edit changes the threshold for DrawBlack to render, this is needed to ensure
		/// that black squares dont appear in thin air.
		/// </summary>
		/// <param name="il"></param>
		private void ChangeBlackThreshold(ILContext il)
		{
			var c = new ILCursor(il);
			c.TryGotoNext(n => n.MatchLdloc(8), n => n.MatchStloc(12)); //beginning of the loop, local 11 is a looping variable
			c.Index++; //this is kinda goofy since I dont think you could actually ever write c# to compile to the resulting IL from emitting here.
			c.Emit(OpCodes.Ldloc, 3); //pass the original value so we can set that instead if we dont want to change the threshold
			c.EmitDelegate<Func<float, float>>(NewThreshold); //check if were in the biome to set, else set the original value
			c.Emit(OpCodes.Stloc, 3); //num2 in vanilla, controls minimum threshold to turn a tile black
		}

		/// <summary>
		/// This is called by the ChangeBlackThreshold IL edit to get the appropriate threshold
		/// </summary>
		/// <param name="orig">The original threshold value, to return if not in the biome</param>
		/// <returns>The threshold to use</returns>
		private float NewThreshold(float orig)
		{
			if (onScreen)
				return 0.01f;
			else
				return orig;
		}

		/// <summary>
		/// This detour acts as the main function responsible for drawing the background in the desert
		/// </summary>
		/// <param name="orig"></param>
		/// <param name="self"></param>
		private void DrawVoidBackground(On_Main.orig_DrawBackgroundBlackFill orig, Main self)
		{
			orig(self);

			// If we're in an invalid state to draw, such as on the menu, are a dedserv, or are not in the biome, dont!
			if (Main.gameMenu || Main.dedServ || !onScreen)
				return;

			parallaxOrigin = Main.screenPosition + Main.ScreenSize.ToVector2() / 2f;
			vanillaParallax = 1 - (Main.caveParallax - 0.8f) / 0.2f;

			Vector2 basepoint = Vector2.Zero;

			float x = basepoint.X + GetParallaxOffset(basepoint.X, 0.6f) - Main.screenPosition.X;
			float y = basepoint.Y + GetParallaxOffsetY(basepoint.Y, 0.2f) - Main.screenPosition.Y;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);

			//DrawLayer(basepoint, textures[4].Value, 5, Vector2.UnitY * 40, default, false);
			//DrawLayer(basepoint, textures[3].Value, 4, Vector2.UnitY * 150, default, false);
			//DrawLayer(basepoint, textures[2].Value, 3, Vector2.UnitY * 160, default, false);
			//DrawLayer(basepoint, textures[1].Value, 2, Vector2.UnitY * 355, default, false);
			//DrawLayer(basepoint, textures[0].Value, 1, Vector2.UnitY * 380, default, false);

			float progress = (float)Math.Sin(Main.GameUpdateCount / 50f);
			var color = new Color(255, 255, 100, 0);
			float colorAdd = 0f;

			if (!Main.dayTime)
				colorAdd = Math.Min(2, (float)Math.Sin(Main.time / Main.nightLength) * 5.0f);

			
		}

		/// <summary>
		/// Helper method to check if a tiling background tile can be drawn
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="size"></param>
		/// <param name="biome"></param>
		/// <param name="dontCheckScreen"></param>
		/// <returns></returns>
		private bool CheckBackground(Vector2 pos, Vector2 size, Rectangle biome, bool dontCheckScreen = false)
		{
			if (dontCheckScreen)
			{
				if (!Main.BackgroundEnabled)
					return true;
				else if (!biome.Contains(((pos + Main.screenPosition) / 16).ToPoint()) || !biome.Contains(((pos + size + Main.screenPosition) / 16).ToPoint()))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Draws a single layer of the vitric desert background
		/// </summary>
		/// <param name="basepoint">The base pos to calculate parallax from</param>
		/// <param name="texture">The texture for this layer</param>
		/// <param name="parallax">The parallax offset to calculate parallax</param>
		/// <param name="off">Absolute offset to add to this layer</param>
		/// <param name="color">The color to draw this layer in</param>
		/// <param name="flip">If this layer is flipped or not</param>
		private static void DrawLayer(Vector2 basepoint, Texture2D texture, float parallax, Vector2 off = default, Color color = default, bool flip = false)
		{
			if (color == default)
			{
				color = Color.White;

				byte a = color.A;

				color *= 0.8f + (Main.dayTime ? (float)Math.Sin(Main.time / Main.dayLength * 3.14f) * 0.35f : -(float)Math.Sin(Main.time / Main.nightLength * 3.14f) * 0.35f);
				color.A = a;
			}

			for (int k = 0; k <= 5; k++)
			{
				float x = basepoint.X + off.X + k * 739 * 4 + GetParallaxOffset(basepoint.X, parallax * 0.1f) - (int)Main.screenPosition.X;
				float y = basepoint.Y + off.Y - (int)Main.screenPosition.Y + GetParallaxOffsetY(basepoint.Y + Main.maxTilesY * 8, parallax * 0.04f);

				if (x > -texture.Width && x < Main.screenWidth + 30)
					Main.spriteBatch.Draw(texture, new Vector2(x, y), null, color, 0f, Vector2.Zero, 1f, flip ? SpriteEffects.FlipVertically : 0, 0);
			}
		}

		private static int GetParallaxOffset(float startpoint, float factor)
		{
			return (int)((parallaxOrigin.X - startpoint) * factor * vanillaParallax);
		}

		private static int GetParallaxOffsetY(float startpoint, float factor)
		{
			return (int)((parallaxOrigin.Y - startpoint) * factor);
		}
    }

    /*public class VoidBiomeSystem : ModSystem
    {
        
    } */

    public class VoidBiomeBackgroundStyle : ModSurfaceBackgroundStyle
    {
        public override void ModifyFarFades(float[] fades, float transitionSpeed) {
			for (int i = 0; i < fades.Length; i++) {
				if (i == Slot) {
					fades[i] += transitionSpeed;
					if (fades[i] > 1f) {
						fades[i] = 1f;
					}
				}
				else {
					fades[i] -= transitionSpeed;
					if (fades[i] < 0f) {
						fades[i] = 0f;
					}
				}
			}
		}

		public override int ChooseFarTexture() {
			return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Backgrounds/VoidBackGround4");
		}
        private static int SurfaceFrameCounter;
		private static int SurfaceFrame;
		public override int ChooseMiddleTexture() {
			if (++SurfaceFrameCounter > 12) {
				SurfaceFrame = (SurfaceFrame + 1) % 3;
				SurfaceFrameCounter = 0;
			}
			switch (SurfaceFrame) {
				case 0:
					return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Backgrounds/VoidBackGround3");
				case 1:
					return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Backgrounds/VoidBackGround2");
				case 2:
					return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Backgrounds/VoidBackGround1");
				default:
					return -1;
			}
		}

		public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b) {
			return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Backgrounds/VoidBackGround0");
		}
    }
}