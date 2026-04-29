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
using AncientRealms.Content.SubWorlds.SubSpaceHub;
using AncientRealms.Content.SubWorlds.SubSpaceHub.Structures;

namespace AncientRealms.SubWorlds.SubSpaceHub
{
    public class SubSpaceHub : Subworld
    {
        public class SubSpaceHubGenPass : ExampleGenPass
        {

            public SubSpaceHubGenPass() : base("Terrain", 1f) { }
            protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
            {
                progress.Message = "Generating terrain"; // Sets the text displayed for this pass
                Main.worldSurface = Main.maxTilesY - 42; // Hides the underground layer just out of bounds
                Main.rockLayer = Main.maxTilesY; // Hides the cavern layer way out of bounds
                for (int i = 0; i < Main.maxTilesX; i++)
                {
                    for (int j = 0; j < Main.maxTilesY; j++)
                    {
                        progress.Set((j + i * Main.maxTilesY) / (float)(Main.maxTilesX * Main.maxTilesY)); // Controls the progress bar, should only be set between 0f and 1f
                        Tile tile = Main.tile[i, j];
                        tile.HasTile = true;
                        tile.TileType = TileID.Dirt;
                    }
                }
            }
        }
        public override int Width => 2000;
        public override int Height => 1200;
        public override bool ShouldSave => true;

        public override List<GenPass> Tasks => new List<GenPass>()
        {
            new SubSpaceHubGenPass()
        };

	// Sets the time to the middle of the day whenever the subworld loads
	public override void OnLoad()
	{
		Main.dayTime = true;
		Main.time = 27000;
	}
    }
}