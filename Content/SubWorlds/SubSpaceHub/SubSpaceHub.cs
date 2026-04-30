using Terraria;
using SubworldLibrary;
using Terraria.WorldBuilding;
using System.Collections.Generic;
using Terraria.ModLoader;
using System.Reflection;

using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.DataStructures;
using AncientRealms.Common.Systems;
using StructureHelper.Models;

namespace AncientRealms.Content.SubWorlds.SubSpaceHub
{
    public class SubSpaceHub : Subworld
    {
        public class SubSpaceHubGenPass : GenPass
        {

            public SubSpaceHubGenPass() : base("Terrain", 1f) {}
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

                //Set defualt spawn position
                //Main.spawnTileX

                //Generate Structure
                StructureData Enterance = StructureHelper.API.Generator.GetStructureData("Structures/enterance", AncientRealms.Instance);
                StructureData GateKeeperArena = StructureHelper.API.Generator.GetStructureData("Structures/GateKeeperArena", AncientRealms.Instance);
                Point16 GateKeeperArenaSpawn = new Point16(Main.spawnTileX -10 + Enterance.width,Main.spawnTileY -13 - GateKeeperArena.height + 1 + Enterance.height);
                
                StructureHelper.API.Generator.GenerateFromData(Enterance, new Point16(Main.spawnTileX -10, Main.spawnTileY -13));
                StructureHelper.API.Generator.GenerateFromData(GateKeeperArena, GateKeeperArenaSpawn);
                
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

        public override void CopyMainWorldData()
        {
            SubworldSystem.CopyWorldData(nameof(BossDownedSystem.downedEldritchVoid), BossDownedSystem.downedEldritchVoid);
            SubworldSystem.CopyWorldData(nameof(BossDownedSystem.downedGateKeeper), BossDownedSystem.downedGateKeeper);

        }

        public override void ReadCopiedMainWorldData()
        {
            BossDownedSystem.downedGateKeeper = SubworldSystem.ReadCopiedWorldData<bool>(nameof(BossDownedSystem.downedGateKeeper));
            BossDownedSystem.downedEldritchVoid = SubworldSystem.ReadCopiedWorldData<bool>(nameof(BossDownedSystem.downedEldritchVoid));

            base.ReadCopiedMainWorldData();
        }
    }
}