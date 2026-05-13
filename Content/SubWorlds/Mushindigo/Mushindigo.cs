using Terraria;
using SubworldLibrary;
using Terraria.WorldBuilding;
using System.Collections.Generic;
using Terraria.ModLoader;
using System.Reflection;
using System.Numerics;
using System;
using static FastNoiseLite;

using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.DataStructures;
using AncientRealms.Core.Systems;
using StructureHelper.Models;

namespace AncientRealms.Content.SubWorlds.Mushindigo
{
    public class Mushindigo : Subworld
    {
        public class MushindigoBaseGenPass : GenPass
        {
            public MushindigoBaseGenPass() : base("Terrain", 1f) {}
            protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
            {
                progress.Message = "Generating terrain"; // Sets the text displayed for this pass
                Main.worldSurface = Main.maxTilesY / 5; 
                Main.rockLayer = Main.worldSurface + Main.maxTilesY / 15; 
            
                for(int i = 0; i < Main.maxTilesX; i++)
                {
                    for(int j = (int)Main.rockLayer; j < Main.UnderworldLayer; j++)
                    {
                        Tile tile = Main.tile[i, j];
                        tile.HasTile = true;
                        tile.TileType = TileID.Stone;
                    }
                }
                GenerateSurfaceLayer(progress, configuration);
                //GenerateOres(progress, configuration);
                //Set biome locations
            }

            public void GenerateSurfaceLayer(GenerationProgress progress, GameConfiguration configuration)
            {
                FastNoiseLite noise = new FastNoiseLite(Main.ActiveWorldFileData.Seed);
                noise.SetNoiseType(NoiseType.OpenSimplex2);
                noise.SetFrequency(0.006f);

                 for(int i = 0; i < Main.maxTilesX ; i++)
                {
                    int surfaceHeight = (int)(noise.GetNoise(i, 0) * 20 + Main.worldSurface + -20);
                    for(int j = surfaceHeight; j < Main.rockLayer; j++)
                    {
                        Tile tile = Main.tile[i, j];
                        tile.HasTile = true;
                        tile.TileType = TileID.Mud;
                    } 
                }
            }
        }

        public class MushindigoOceanGenpass : GenPass
        {
            public MushindigoOceanGenpass() : base("Terrain", 1f) {}
            protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
            {
                progress.Message = "Making Oceans"; // Sets the text displayed for this pass
                int xCord = 300;
                Point16 ShoreLeft = new Point16(xCord , Helpers.GenerationHelper.GetHighestBlockY(xCord));
                Point16 ShoreRight = new Point16(Main.maxTilesX - xCord, Helpers.GenerationHelper.GetHighestBlockY(Main.maxTilesX - xCord));

                GenerateSandyShore(ShoreLeft, progress,  configuration);
                GenerateRockyCoast(ShoreRight, progress,  configuration);
            }

            private void GenerateSandyShore(Point16 Shore, GenerationProgress progress, GameConfiguration configuration)
            {
                bool leftSide = true;
                if(Shore.X > Main.maxTilesX/2)
                    leftSide = false;
                    
                //clear any terrain above the waterline  
                if(leftSide)
                {  
                    for(int i = 0; i < Shore.X; i++)
                    {
                        for(int j = 0; j < Shore.Y; j++)
                        {
                            Tile tile = Main.tile[i, j];
                            tile.HasTile = false;
                        }
                    }
                }else{
                    for(int i = Shore.X; i < Main.maxTilesX; i++)
                    {
                        for(int j = 0; j < Shore.Y; j++)
                        {
                            Tile tile = Main.tile[i, j];
                            tile.HasTile = false;
                        }
                    }
                }
                //then generate a sandy beach using a logarithmic curve
                int x = Shore.X;
                int maxDepth = Main.maxTilesY / 10;
                float A = (maxDepth - 2)/ 2;
                while((x >= 0 && leftSide) || (x < Main.maxTilesX && !leftSide))
                {
                    int floor = Shore.Y + (int)(maxDepth/(1 + (A * Math.Pow(Math.E, -0.05f * Math.Abs(x - Shore.X)))));
                    for(int y = Shore.Y; y < floor && y < Main.maxTilesY; y++)
                    {
                        Tile tile = Main.tile[x, y];
                        if(floor - y < 25)
                        {
                            tile.HasTile = true;
                            tile.TileType = TileID.Sand;    
                        }else if(y - Shore.Y > 5)
                        {
                            tile.HasTile = false;
                            tile.LiquidType = LiquidID.Water;
                            tile.LiquidAmount = 255;
                        }
                        else
                        {    
                            tile.HasTile = false;
                        }
                    }
                    if(leftSide){x--;}else{x++;}
                }   
            }
            private void GenerateRockyCoast(Point16 Shore, GenerationProgress progress, GameConfiguration configuration)
            {
                bool leftSide = true;
                if(Shore.X > Main.maxTilesX/2)
                    leftSide = false;
                List<Point16> PillarLocations = new List<Point16>();
                int maxDepth = 6 * (Main.maxTilesY / 10);
                float A = (maxDepth - Shore.Y) / Shore.Y;
                int x = Shore.X;
                
                //clear any terrarian above the waterline
                if(leftSide)
                {  
                    for(int i = 0; i < Shore.X; i++)
                    {
                        for(int j = 0; j < Shore.Y; j++)
                        {
                            Tile tile = Main.tile[i, j];
                            tile.HasTile = false;
                        }
                    }
                }else{
                    for(int i = Shore.X; i < Main.maxTilesX; i++)
                    {
                        for(int j = 0; j < Shore.Y; j++)
                        {
                            Tile tile = Main.tile[i, j];
                            tile.HasTile = false;
                        }
                    }
                }
                //then generate the coast using a logistic curve, with random stone pillars for variety
                while((x >= 0 && leftSide) || (x < Main.maxTilesX && !leftSide))
                {
                    int floor = (int)(maxDepth/(1 + (A * Math.Pow(Math.E, -0.025f * Math.Abs(x - Shore.X)))));
                    for(int y = Shore.Y; y < floor && y < Main.maxTilesY; y++)
                    {
                        Tile tile = Main.tile[x, y];
                        if(floor - y < 25)
                        {
                            tile.HasTile = true;
                            tile.TileType = TileID.Sand;    
                        }else if(y - Shore.Y > 5)
                        {
                            tile.HasTile = false;
                            tile.LiquidType = LiquidID.Water;
                            tile.LiquidAmount = 255;
                        }
                        else
                        {    
                            tile.HasTile = false;
                        }
                        int rand = WorldGen.genRand.Next(0, 50000);
                        if(rand > 49990)
                        {
                            PillarLocations.Add(new Point16(x, y));
                        }
                    }
                    foreach(Point16 location in PillarLocations)
                    {
                        GenerateStonePillar(location, WorldGen.genRand.Next(12, 18), WorldGen.genRand.Next(30, 45));
                    }
                    if(leftSide){x--;}else{x++;}
                } 
            }
            private void GenerateStonePillar(Point16 center, int Width, int Height)
            {
                for(int y = center.Y - Height/2; y < center.Y + Height/2; y++)
                {
                    int localWidth = (int)Math.Ceiling(Width * 0.5f * Math.Sqrt(1 - Math.Pow((y - center.Y) / (Height * 0.5f), 2)));
                    for(int x = center.X - localWidth/2; x < center.X + localWidth/2; x++)
                    {    
                        Tile tile = Main.tile[x,y];
                        tile.HasTile = true;
                        tile.TileType = TileID.Stone;
                    }
                }
            }
        }

        public class MushindigoUnderworldGenpass : GenPass
        {
            public MushindigoUnderworldGenpass() : base("Terrain", 1f){}
            protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
            {
                progress.Message = "Making Shroom Hell"; // Sets the text displayed for this pass

            }
        }

        public class MushindigoHardModeGenpass : GenPass
        {
            public MushindigoHardModeGenpass() : base("Terrain", 1f){}
            protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
            {
            }
            
        }
    
        //Same size as a small world (ToDo: make it so that it increases with main world size)
        public override int Width => 4200;
        public override int Height => 1200;
        public override bool ShouldSave => false; //just for testing will be true later

        public override List<GenPass> Tasks => new List<GenPass>()
        {
            new MushindigoBaseGenPass(),
            new MushindigoOceanGenpass(),
            new MushindigoUnderworldGenpass()
        };

        // Sets the time to the middle of the day whenever the subworld loads
        public override void OnLoad()
        {
            Main.dayTime = true;
            Main.time = 27000;
            Main.spawnTileY = Helpers.GenerationHelper.GetHighestBlockY(Main.spawnTileX);
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