using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Linq;
using System.Reflection;
using AncientRealms.Content.Tiles.Misc;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader.IO;
using AncientRealms.Content.SubWorlds.SubSpaceHub;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Capture;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using ReLogic.Content;
using AssGen;
using AncientRealms.Content.SubWorlds.Mushindigo;
using AncientRealms.Core.Systems;

namespace AncientRealms.Content.Biomes
{
    public class MushindigoSpiderNestBiome : ModBiome
    {

        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;   
		private static Vector2 parallaxOrigin;
		private static float vanillaParallax;

		public static bool onScreen;
		public override bool IsBiomeActive(Player player)
        {
            return SubworldLibrary.SubworldSystem.IsActive<Mushindigo>() && player.Center() > Main.rockLayer;
        }
	}
}