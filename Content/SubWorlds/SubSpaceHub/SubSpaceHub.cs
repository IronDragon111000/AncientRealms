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
            public List<Point16> LargeIslandLocations = new List<Point16>();
            public List<Point16> MediumIslandLocations = new List<Point16>();
            public List<Point16> SmallIslandLocations = new List<Point16>();
            public SubSpaceHubGenPass() : base("Terrain", 1f) {}
            protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
            {
                progress.Message = "Generating terrain"; // Sets the text displayed for this pass
                Main.worldSurface = Main.maxTilesY - 42; // Hides the underground layer just out of bounds
                Main.rockLayer = Main.maxTilesY; // Hides the cavern layer way out of bounds
                

                
                GenerateFloatingIslands(progress, configuration);

                //Generate Structure
                StructureData Enterance = StructureHelper.API.Generator.GetStructureData("Structures/enterance", AncientRealms.Instance);
                StructureData GateKeeperArena = StructureHelper.API.Generator.GetStructureData("Structures/GateKeeperArena", AncientRealms.Instance);
                Point16 GateKeeperArenaSpawn = new Point16(Main.spawnTileX -10 + Enterance.width,Main.spawnTileY -13 - GateKeeperArena.height + 1 + Enterance.height);
                
                StructureHelper.API.Generator.GenerateFromData(Enterance, new Point16(Main.spawnTileX -10, Main.spawnTileY -13));
                StructureHelper.API.Generator.GenerateFromData(GateKeeperArena, GateKeeperArenaSpawn);
                
            }
            private void GenerateFloatingIslands(GenerationProgress progress, GameConfiguration configuration)
            {
                Vector2 spawnPoint = new Vector2(Main.spawnTileX , Main.spawnTileY);
                for(int i = 80; i < Main.maxTilesX - 80; i++)
                {
                    for(int j = 50; j < Main.maxTilesY -50; j++)
                    {
                        Vector2 cords = new Vector2(i, j);
                        //check if near spawn if so dont place an island
                        if(Vector2.Distance(spawnPoint, cords) > 200)
                        {
                            int rand = WorldGen.genRand.Next(0, 1000);
                            if(rand > 980)
                            {
                                LargeIslandLocations.Add(new Point16(i, j));
                                GenerateIsland(i, j, 2);
                            } else if(rand > 940)
                            {
                                MediumIslandLocations.Add(new Point16(i, j));
                                GenerateIsland(i, j, 1);
                            } else if (rand > 900)
                            {
                                SmallIslandLocations.Add(new Point16(i, j));
                                GenerateIsland(i, j, 0);
                            }
                        }
                    }
                }
            }
            public void GenerateIsland(int x, int y , int size)
            {
                // large sized
                int wid = WorldGen.genRand.Next(50, 74); 
                int maxDepth = 28;
                int minDepth = 18;
                int maxDepthChange = 3;
                int depth = 3;
                if(size == 1) // mid sized
                {
                    wid = WorldGen.genRand.Next(30,40);
                    maxDepth = 20;
                    minDepth = 10;
                    maxDepthChange = 2;
                } else if(size == 0) // small sized
                {
                    wid = WorldGen.genRand.Next(8,16);
                    maxDepth = 6;
                    minDepth = 2;
                    maxDepthChange = 1;
                }
                for (int i = x - (int)(wid / 2f); i < x + wid / 2f; ++i)
                {
                    for(int j = y; j < y + depth; j++)
                    {
                        if(i >= 0 && i < Main.maxTilesX && j >= 0 && j < Main.maxTilesY) // prevent out of bounds
                        {   
                            Tile tile = Main.tile[i, j];
                            tile.HasTile = true;
                            tile.TileType = TileID.Dirt;
                        }
                    }
                    depth += WorldGen.genRand.Next(-maxDepthChange, maxDepthChange);
                    if(depth > maxDepth)
                        depth = maxDepth;
                    if(depth < minDepth)
                        depth = minDepth;
                }   
            }
        }
        //Same size as a small world (ToDo: make it so that it increases with main world size)
        public override int Width => 4200;
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