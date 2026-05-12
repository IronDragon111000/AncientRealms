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
                GenerateSurface(progress, configuration);
            }

            public void GenerateSurface(GenerationProgress progress, GameConfiguration configuration)
            {
                FastNoiseLite noise = new FastNoiseLite(Main.ActiveWorldFileData.Seed);
                noise.SetNoiseType(NoiseType.OpenSimplex2);
                noise.SetFrequency(0.01f);

                 for(int i = 0; i < Main.maxTilesX; i++)
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
        public class MushindigoStructureGenPass : GenPass
        {
            public MushindigoStructureGenPass() : base("Terrain", 1f) {}
            protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
            {
                progress.Message = "Generating terrain"; // Sets the text displayed for this pass

                
            }
        }
        //Same size as a small world (ToDo: make it so that it increases with main world size)
        public override int Width => 4200;
        public override int Height => 1200;
        public override bool ShouldSave => true;

        public override List<GenPass> Tasks => new List<GenPass>()
        {
            new MushindigoBaseGenPass(),
            new MushindigoStructureGenPass()
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