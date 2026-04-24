using Terraria;
using SubworldLibrary;
using Terraria.WorldBuilding;
using System.Collections.Generic;
using Terraria.ModLoader;

using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using StructureHelper;
using Terraria.DataStructures;
using AncientRealms.Content;

namespace AncientRealms.SubWorlds.SubSpaceHub
{
    public class SubSpaceHub : Subworld
    {
        public override int Width => 2000;
        public override int Height => 1200;
        public override bool ShouldSave => true;

         public override List<GenPass> Tasks => new() { new PassLegacy("Subworld", SubworldGeneration) };
        private void SubworldGeneration(GenerationProgress progress, GameConfiguration configuration)
        {
            Main.spawnTileX = 929;
            Main.spawnTileY = 333;

            Main.worldSurface = 600.0;
            Main.rockLayer = Main.maxTilesY;
            SubworldSystem.hideUnderworld = true;

             StructureHelper.API.Generator.GenerateStructure("Structures/entrance", new Point16(0,0), ModLoader.GetMod("AncientRealms"));

        }

	// Sets the time to the middle of the day whenever the subworld loads
	public override void OnLoad()
	{
		Main.dayTime = true;
		Main.time = 27000;
	}
    }
}