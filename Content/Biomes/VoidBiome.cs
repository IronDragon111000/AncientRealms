using System;
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

namespace AncientRealms.Content.Biomes
{
    public class VoidBiome : ModBiome
    {
        public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<VoidBiomeBackgroundStyle>();
        public override bool IsBiomeActive(Player player)
        {
            return SubworldLibrary.SubworldSystem.IsActive<SubSpaceHub>();
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